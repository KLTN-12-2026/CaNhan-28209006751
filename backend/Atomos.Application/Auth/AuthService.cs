using Atomos.Application.Common;
using Atomos.Domain.Admin;
using Atomos.Domain.Users;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Cryptography;
using ForgotPasswordResult = Atomos.Application.Common.Result<Atomos.Application.Auth.ForgotPasswordResponse>;
using GoogleAuthResult = Atomos.Application.Common.Result<Atomos.Application.Auth.GoogleAuthResponse>;
using LoginResult = Atomos.Application.Common.Result<Atomos.Application.Auth.LoginResponse>;
using RegisterResult = Atomos.Application.Common.Result<Atomos.Application.Auth.RegisterResponse>;
using ResetPasswordResult = Atomos.Application.Common.Result<Atomos.Application.Auth.ResetPasswordResponse>;
using VerifyResetCodeResult = Atomos.Application.Common.Result<Atomos.Application.Auth.VerifyResetCodeResponse>;

namespace Atomos.Application.Auth;

public sealed class AuthService(
    IApplicationDbContext dbContext,
    IEmailSender emailSender,
    IGoogleTokenVerifier googleTokenVerifier,
    IAuthQueries authQueries,
    IPasswordResetTransactions resetTransactions,
    IAuthSessionService sessions) : IAuthService
{
    private const string StudentRoleName = "Student";
    private static readonly HashSet<int> SqlServerUniqueViolationNumbers = [2601, 2627];
    private const string InvalidLoginMessage = "Tên tài khoản/email hoặc mật khẩu không trùng khớp.";
    private const string InvalidResetCodeMessage = "Mã xác thực không đúng hoặc đã hết hạn.";
    private const string ForgotPasswordMessage = "Nếu thông tin khớp với tài khoản, mã xác thực sẽ được gửi đến email đã đăng ký.";
    private static readonly string DummyPasswordHash = PasswordHasher.Hash("Atomos-Dummy-Password");
    private const int ResetTokenExpirationMinutes = 30;
    private const int ResetCodeCooldownSeconds = 60;
    private const int DailyResetEmailLimit = 10;

    public async Task<RegisterResult> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var username = request.Username.Trim();
        var email = request.Email.Trim().ToLowerInvariant();
        string? fullName = string.IsNullOrWhiteSpace(request.FullName)
            ? SanitizeFullName(username)
            : request.FullName.Trim();

        if (string.IsNullOrWhiteSpace(fullName))
        {
            fullName = null;
        }

        var validationError = Validate(username, email, request.Password, fullName);
        if (validationError is not null)
        {
            return validationError;
        }

        if (request.Email.Any(char.IsWhiteSpace))
        {
            return RegisterResult.Fail("Email không được chứa khoảng trắng.", "EMAIL_HAS_WHITESPACE");
        }

        var emailExists = await dbContext.Users
            .AnyAsync(user => user.Email.ToLower() == email, cancellationToken);

        if (emailExists)
        {
            return RegisterResult.Fail("Email đã được sử dụng.", "EMAIL_ALREADY_EXISTS");
        }

        var normalizedUsername = username.ToLowerInvariant();
        var usernameExists = await dbContext.Users
            .AnyAsync(user => user.Username.ToLower() == normalizedUsername, cancellationToken);

        if (usernameExists)
        {
            return RegisterResult.Fail("Tên đăng nhập đã được sử dụng.", "USERNAME_ALREADY_EXISTS");
        }

        var studentRole = await dbContext.Roles
            .SingleOrDefaultAsync(role => role.TenRole == StudentRoleName, cancellationToken);

        if (studentRole is null)
        {
            return RegisterResult.Fail("Vai trò Student chưa được thiết lập.", "STUDENT_ROLE_NOT_FOUND");
        }

        var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
        var user = new User
        {
            Username = username,
            Email = email,
            MatKhau = PasswordHasher.Hash(request.Password),
            HoTen = fullName,
            IDRole = studentRole.IDRole,
            NgayTao = now,
            NgayCapNhat = now
        };

        dbContext.Users.Add(user);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
        {
            return RegisterResult.Fail(
                "Email hoặc tên đăng nhập đã được sử dụng.",
                "REGISTER_CONFLICT");
        }
        catch (DbUpdateException)
        {
            return RegisterResult.Fail(
                "Không thể tạo tài khoản lúc này. Vui lòng kiểm tra cấu hình database.",
                "REGISTER_SAVE_FAILED");
        }

        dbContext.ActivityLogs.Add(NewActivity(user.IDUser, "REGISTER", "User", user.IDUser));
        await dbContext.SaveChangesAsync(cancellationToken);

        return RegisterResult.Ok(new RegisterResponse
        {
            UserId = user.IDUser,
            Username = user.Username,
            Email = user.Email,
            FullName = user.HoTen,
            Role = studentRole.TenRole
        });
    }

    public async Task<LoginResult> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var usernameOrEmail = request.UsernameOrEmail.Trim();

        if (string.IsNullOrWhiteSpace(usernameOrEmail))
        {
            return LoginResult.Fail("Vui lòng nhập tên đăng nhập hoặc email.", "USERNAME_OR_EMAIL_REQUIRED");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return LoginResult.Fail("Vui lòng nhập mật khẩu.", "PASSWORD_REQUIRED");
        }

        var normalizedUsernameOrEmail = usernameOrEmail.ToLowerInvariant();
        var user = await dbContext.Users
            .Include(currentUser => currentUser.Role)
            .SingleOrDefaultAsync(
                currentUser =>
                    currentUser.Username.ToLower() == normalizedUsernameOrEmail ||
                    currentUser.Email.ToLower() == normalizedUsernameOrEmail,
                cancellationToken);

        if (user is null || user.DaKhoa || user.DaXoa)
        {
            PasswordHasher.Verify(request.Password, DummyPasswordHash);
            return LoginResult.Fail(InvalidLoginMessage, "INVALID_CREDENTIALS");
        }

        if (user.MatKhau is null)
        {
            PasswordHasher.Verify(request.Password, DummyPasswordHash);
            return LoginResult.Fail(InvalidLoginMessage, "INVALID_CREDENTIALS");
        }

        if (!PasswordHasher.Verify(request.Password, user.MatKhau, out var needsRehash))
        {
            return LoginResult.Fail(InvalidLoginMessage, "INVALID_CREDENTIALS");
        }

        if (needsRehash)
        {
            user.MatKhau = PasswordHasher.Hash(request.Password);
            user.NgayCapNhat = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
        }
        dbContext.ActivityLogs.Add(NewActivity(user.IDUser, "LOGIN", "User", user.IDUser));
        await dbContext.SaveChangesAsync(cancellationToken);

        return LoginResult.Ok(new LoginResponse
        {
            UserId = user.IDUser,
            Username = user.Username,
            Email = user.Email,
            FullName = user.HoTen,
            Role = user.Role?.TenRole ?? string.Empty
        });
    }

    public async Task<GoogleAuthResult> GoogleAuthAsync(
        GoogleAuthRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.IdToken))
        {
            return GoogleAuthResult.Fail("Không thể đăng nhập bằng Google lúc này.", "GOOGLE_TOKEN_REQUIRED");
        }

        var googleUser = await googleTokenVerifier.VerifyAsync(request.IdToken, cancellationToken);
        if (googleUser is null)
        {
            return GoogleAuthResult.Fail("Thông tin Google không hợp lệ.", "GOOGLE_TOKEN_INVALID");
        }

        var normalizedEmail = googleUser.Email.ToLowerInvariant();
        var user = await dbContext.Users
            .Include(currentUser => currentUser.Role)
            .SingleOrDefaultAsync(currentUser => currentUser.IDGoogle == googleUser.GoogleId, cancellationToken);

        if (user is null && await dbContext.Users.AnyAsync(
            currentUser => currentUser.Email.ToLower() == normalizedEmail,
            cancellationToken))
        {
            return GoogleAuthResult.Fail(
                "Email này đã thuộc một tài khoản khác. Hãy đăng nhập tài khoản đó trước khi liên kết Google.",
                "GOOGLE_EMAIL_LINK_REQUIRED");
        }

        if (user is not null)
        {
            if (user.DaKhoa || user.DaXoa)
            {
                return GoogleAuthResult.Fail("Tài khoản đang bị khóa.", "ACCOUNT_LOCKED");
            }

            dbContext.ActivityLogs.Add(NewActivity(user.IDUser, "LOGIN", "User", user.IDUser));
            await dbContext.SaveChangesAsync(cancellationToken);

            return GoogleAuthResult.Ok(BuildGoogleAuthResponse(user, false));
        }

        var username = request.Username?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(username))
        {
            return GoogleAuthResult.Ok(new GoogleAuthResponse
            {
                RequiresUsername = true,
                Email = normalizedEmail,
                FullName = googleUser.FullName
            });
        }

        var usernameValidationError = ValidateUsername(username);
        if (usernameValidationError is not null)
        {
            return GoogleAuthResult.Fail(usernameValidationError.Value.Message, usernameValidationError.Value.Code);
        }

        var normalizedUsername = username.ToLowerInvariant();
        var usernameExists = await dbContext.Users
            .AnyAsync(currentUser => currentUser.Username.ToLower() == normalizedUsername, cancellationToken);

        if (usernameExists)
        {
            return GoogleAuthResult.Fail("Tên đăng nhập đã được sử dụng.", "USERNAME_ALREADY_EXISTS");
        }

        var studentRole = await dbContext.Roles
            .SingleOrDefaultAsync(role => role.TenRole == StudentRoleName, cancellationToken);

        if (studentRole is null)
        {
            return GoogleAuthResult.Fail("Vai trò Student chưa được thiết lập.", "STUDENT_ROLE_NOT_FOUND");
        }

        var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
        user = new User
        {
            Username = username,
            Email = normalizedEmail,
            MatKhau = null,
            IDGoogle = googleUser.GoogleId,
            HoTen = googleUser.FullName,
            Avatar = googleUser.Avatar,
            IDRole = studentRole.IDRole,
            Role = studentRole,
            NgayTao = now,
            NgayCapNhat = now
        };

        dbContext.Users.Add(user);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
        {
            return GoogleAuthResult.Fail(
                "Email hoặc tên đăng nhập đã được sử dụng.",
                "GOOGLE_REGISTER_CONFLICT");
        }

        dbContext.ActivityLogs.Add(NewActivity(user.IDUser, "REGISTER", "User", user.IDUser));
        await dbContext.SaveChangesAsync(cancellationToken);

        return GoogleAuthResult.Ok(BuildGoogleAuthResponse(user, false));
    }

    public async Task<ForgotPasswordResult> ForgotPasswordAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var usernameOrEmail = request.UsernameOrEmail.Trim();

        if (string.IsNullOrWhiteSpace(usernameOrEmail))
        {
            return ForgotPasswordResult.Fail(
                "Vui lòng nhập tên đăng nhập hoặc email.",
                "USERNAME_OR_EMAIL_REQUIRED");
        }

        var inputValidationError = ValidateUsernameOrEmail(usernameOrEmail);
        if (inputValidationError is not null)
        {
            return inputValidationError;
        }

        var user = await FindUserByUsernameOrEmailAsync(usernameOrEmail, cancellationToken);
        if (user is null)
        {
            return ForgotPasswordResult.Ok(new ForgotPasswordResponse { Message = ForgotPasswordMessage });
        }

        await using var resetTransaction = await resetTransactions.BeginAsync(user.IDUser, cancellationToken);
        var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
        var latestResetToken = await dbContext.PasswordResetTokens
            .Where(resetToken => resetToken.IDUser == user.IDUser)
            .OrderByDescending(resetToken => resetToken.NgayTao)
            .FirstOrDefaultAsync(cancellationToken);

        if (latestResetToken is not null &&
            latestResetToken.NgayTao.AddSeconds(ResetCodeCooldownSeconds) > now)
        {
            return ForgotPasswordResult.Ok(new ForgotPasswordResponse { Message = ForgotPasswordMessage });
        }

        var dayStart = now.AddDays(-1);
        var emailCountToday = await dbContext.PasswordResetTokens
            .CountAsync(
                resetToken =>
                    resetToken.IDUser == user.IDUser &&
                    resetToken.NgayTao >= dayStart,
                cancellationToken);

        if (emailCountToday >= DailyResetEmailLimit)
        {
            return ForgotPasswordResult.Ok(new ForgotPasswordResponse { Message = ForgotPasswordMessage });
        }

        var activeTokens = await dbContext.PasswordResetTokens
            .Where(resetToken =>
                resetToken.IDUser == user.IDUser &&
                !resetToken.DaSuDung &&
                resetToken.HetHan > now)
            .ToListAsync(cancellationToken);

        foreach (var activeToken in activeTokens)
        {
            activeToken.DaSuDung = true;
        }

        var resetCode = await GenerateUniqueResetCodeAsync(cancellationToken);
        var resetToken = new PasswordResetToken
        {
            IDUser = user.IDUser,
            Token = resetCode,
            HetHan = now.AddMinutes(ResetTokenExpirationMinutes),
            DaSuDung = false,
            NgayTao = now,
            IpAddress = NormalizeAuditValue(request.IpAddress, 45),
            UserAgent = NormalizeAuditValue(request.UserAgent, 512)
        };

        dbContext.PasswordResetTokens.Add(resetToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        await resetTransaction.CommitAsync(cancellationToken);

        try
        {
            await emailSender.SendAsync(
                BuildPasswordResetEmail(user.Email, resetCode),
                cancellationToken);
        }
        catch (EmailDeliveryException)
        {
            await using var failureTransaction = await resetTransactions.BeginAsync(user.IDUser, cancellationToken);
            resetToken.DaSuDung = true;
            await dbContext.SaveChangesAsync(cancellationToken);
            await failureTransaction.CommitAsync(cancellationToken);

            return ForgotPasswordResult.Ok(new ForgotPasswordResponse { Message = ForgotPasswordMessage });
        }

        return ForgotPasswordResult.Ok(new ForgotPasswordResponse { Message = ForgotPasswordMessage });
    }

    public async Task<VerifyResetCodeResult> VerifyResetCodeAsync(
        VerifyResetCodeRequest request,
        CancellationToken cancellationToken)
    {
        var usernameOrEmail = request.UsernameOrEmail.Trim();
        var code = request.Code.Trim();

        if (string.IsNullOrWhiteSpace(usernameOrEmail))
        {
            return VerifyResetCodeResult.Fail(
                "Vui lòng nhập tên đăng nhập hoặc email.",
                "USERNAME_OR_EMAIL_REQUIRED");
        }

        var inputValidationError = ValidateUsernameOrEmail(usernameOrEmail);
        if (inputValidationError is not null)
        {
            return VerifyResetCodeResult.Fail(inputValidationError.Message, inputValidationError.Code);
        }

        var codeValidationError = ValidateResetCode(code);
        if (codeValidationError is not null)
        {
            return VerifyResetCodeResult.Fail(codeValidationError.Value.Message, codeValidationError.Value.Code);
        }

        var normalizedUsernameOrEmail = usernameOrEmail.ToLowerInvariant();
        var user = await authQueries.FindUserForResetVerificationAsync(normalizedUsernameOrEmail, cancellationToken);
        if (user is null)
        {
            return VerifyResetCodeResult.Fail(InvalidResetCodeMessage, "RESET_CODE_INVALID");
        }

        await using var transaction = await resetTransactions.BeginAsync(user.Id, cancellationToken);
        var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
        var resetToken = await FindLatestResetTokenAsync(user.Id, cancellationToken);
        if (resetToken is null || !PasswordResetChallenge.TryAuthenticate(resetToken, code, now, otpOnly: true))
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return VerifyResetCodeResult.Fail(InvalidResetCodeMessage, "RESET_CODE_INVALID");
        }

        var grant = PasswordResetChallenge.ExchangeOtpForGrant(resetToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return VerifyResetCodeResult.Ok(new VerifyResetCodeResponse
        {
            Message = "Mã xác thực chính xác.",
            ResetToken = grant
        });
    }

    public async Task<ResetPasswordResult> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var usernameOrEmail = request.UsernameOrEmail.Trim();
        var code = request.Code.Trim();

        if (string.IsNullOrWhiteSpace(usernameOrEmail))
        {
            return ResetPasswordResult.Fail(
                "Vui lòng nhập tên đăng nhập hoặc email.",
                "USERNAME_OR_EMAIL_REQUIRED");
        }

        var inputValidationError = ValidateUsernameOrEmail(usernameOrEmail);
        if (inputValidationError is not null)
        {
            return ResetPasswordResult.Fail(inputValidationError.Message, inputValidationError.Code);
        }

        var codeValidationError = PasswordResetChallenge.IsGrant(code) ? null : ValidateResetCode(code);
        if (codeValidationError is not null)
        {
            return ResetPasswordResult.Fail(codeValidationError.Value.Message, codeValidationError.Value.Code);
        }

        var passwordValidationError = ValidateNewPassword(request.Password);
        if (passwordValidationError is not null)
        {
            return passwordValidationError;
        }

        var user = await FindUserByUsernameOrEmailAsync(usernameOrEmail, cancellationToken);
        if (user is null)
        {
            return ResetPasswordResult.Fail(InvalidResetCodeMessage, "RESET_CODE_INVALID");
        }

        await using var transaction = await resetTransactions.BeginAsync(user.IDUser, cancellationToken);
        var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
        var resetToken = await FindLatestResetTokenAsync(user.IDUser, cancellationToken);
        if (resetToken is null || !PasswordResetChallenge.TryAuthenticate(resetToken, code, now, otpOnly: false))
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return ResetPasswordResult.Fail(InvalidResetCodeMessage, "RESET_CODE_INVALID");
        }

        if (IsReusedPassword(request.Password, user.MatKhau, user.MatKhauTruocDo))
        {
            return ResetPasswordResult.Fail(
                "Mật khẩu mới không được trùng mật khẩu hiện tại hoặc mật khẩu ngay trước đó.",
                "PASSWORD_REUSED");
        }

        user.MatKhauTruocDo = user.MatKhau;
        user.MatKhau = PasswordHasher.Hash(request.Password);
        user.NgayCapNhat = now;

        var activeTokens = await dbContext.PasswordResetTokens
            .Where(currentToken =>
                currentToken.IDUser == user.IDUser &&
                !currentToken.DaSuDung &&
                currentToken.HetHan > now)
            .ToListAsync(cancellationToken);

        foreach (var activeToken in activeTokens)
        {
            activeToken.DaSuDung = true;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        await sessions.RevokeAllUserSessionsAsync(
            user.IDUser,
            AuthSessionRevokeReasons.PasswordReset,
            cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return ResetPasswordResult.Ok(new ResetPasswordResponse { Message = "Đặt lại mật khẩu thành công." });
    }

    private static RegisterResult? Validate(
        string username,
        string email,
        string password,
        string? fullName)
    {
        var usernameValidationError = ValidateUsername(username);
        if (usernameValidationError is not null)
        {
            return RegisterResult.Fail(usernameValidationError.Value.Message, usernameValidationError.Value.Code);
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return RegisterResult.Fail("Vui lòng nhập email.", "EMAIL_REQUIRED");
        }

        if (email.Length > 100 || !EmailValidator.IsValid(email))
        {
            return RegisterResult.Fail("Email không hợp lệ.", "EMAIL_INVALID");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return RegisterResult.Fail("Vui lòng nhập mật khẩu.", "PASSWORD_REQUIRED");
        }

        if (password.Length < 8)
        {
            return RegisterResult.Fail("Mật khẩu phải có ít nhất 8 ký tự.", "PASSWORD_TOO_SHORT");
        }

        if (!password.Any(char.IsLetter) || !password.Any(char.IsDigit))
        {
            return RegisterResult.Fail("Mật khẩu phải có cả chữ và số.", "PASSWORD_TOO_WEAK");
        }

        if (fullName?.Length > 100)
        {
            return RegisterResult.Fail("Họ tên không được vượt quá 100 ký tự.", "FULL_NAME_TOO_LONG");
        }

        if (!string.IsNullOrWhiteSpace(fullName) && !fullName.All(character => char.IsLetter(character) || char.IsWhiteSpace(character)))
        {
            return RegisterResult.Fail("Họ tên chỉ được gồm chữ.", "FULL_NAME_INVALID_CHARACTERS");
        }

        return null;
    }

    private static GoogleAuthResponse BuildGoogleAuthResponse(User user, bool requiresUsername)
    {
        var role = user.Role?.TenRole ?? string.Empty;

        return new GoogleAuthResponse
        {
            RequiresUsername = requiresUsername,
            UserId = user.IDUser,
            Username = user.Username,
            Email = user.Email,
            FullName = user.HoTen,
            Role = role
        };
    }

    private static (string Message, string Code)? ValidateUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return ("Vui lòng nhập tên đăng nhập.", "USERNAME_REQUIRED");
        }

        if (username.Length is < 3 or > 50)
        {
            return ("Tên đăng nhập phải từ 3 đến 50 ký tự.", "USERNAME_LENGTH_INVALID");
        }

        if (!username.All(char.IsLetterOrDigit))
        {
            return ("Tên đăng nhập chỉ được gồm chữ và số.", "USERNAME_INVALID_CHARACTERS");
        }

        return null;
    }

    private static bool IsReusedPassword(string password, string? currentHash, string? previousHash)
    {
        return (currentHash is not null && PasswordHasher.Verify(password, currentHash)) ||
               (previousHash is not null && PasswordHasher.Verify(password, previousHash));
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        var number = exception.GetBaseException()
            .GetType()
            .GetProperty("Number")
            ?.GetValue(exception.GetBaseException());

        return number is int errorNumber
            && SqlServerUniqueViolationNumbers.Contains(errorNumber);
    }

    private static string SanitizeFullName(string value)
    {
        return new string(value.Where(char.IsLetter).ToArray());
    }

    private async Task<User?> FindUserByUsernameOrEmailAsync(
        string usernameOrEmail,
        CancellationToken cancellationToken)
    {
        var normalizedUsernameOrEmail = usernameOrEmail.ToLowerInvariant();

        return await dbContext.Users
            .SingleOrDefaultAsync(
                currentUser =>
                    !currentUser.DaKhoa &&
                    !currentUser.DaXoa &&
                    (currentUser.Username.ToLower() == normalizedUsernameOrEmail ||
                     currentUser.Email.ToLower() == normalizedUsernameOrEmail),
                cancellationToken);
    }

    private async Task<PasswordResetToken?> FindLatestResetTokenAsync(
        int userId,
        CancellationToken cancellationToken)
    {
        return await dbContext.PasswordResetTokens
            .Where(resetToken => resetToken.IDUser == userId)
            .OrderByDescending(resetToken => resetToken.NgayTao)
            .ThenByDescending(resetToken => resetToken.IDReset)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static (string Message, string Code)? ValidateResetCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return ("Vui lòng nhập mã xác thực.", "RESET_CODE_REQUIRED");
        }

        if (!PasswordResetChallenge.IsOtp(code))
        {
            return ("Mã xác thực phải gồm 6 chữ số.", "RESET_CODE_INVALID_FORMAT");
        }

        return null;
    }

    private static ResetPasswordResult? ValidateNewPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return ResetPasswordResult.Fail("Vui lòng nhập mật khẩu mới.", "PASSWORD_REQUIRED");
        }

        if (password.Length < 8)
        {
            return ResetPasswordResult.Fail("Mật khẩu phải có ít nhất 8 ký tự.", "PASSWORD_TOO_SHORT");
        }

        if (!password.Any(char.IsLetter) || !password.Any(char.IsDigit))
        {
            return ResetPasswordResult.Fail("Mật khẩu phải có cả chữ và số.", "PASSWORD_TOO_WEAK");
        }

        return null;
    }

    private static string GenerateResetCode()
    {
        return RandomNumberGenerator
            .GetInt32(0, 1_000_000)
            .ToString("D6", CultureInfo.InvariantCulture);
    }

    private async Task<string> GenerateUniqueResetCodeAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var code = GenerateResetCode();
            var exists = await dbContext.PasswordResetTokens
                .AnyAsync(resetToken => resetToken.Token == code, cancellationToken);

            if (!exists)
            {
                return code;
            }
        }

        return $"{RandomNumberGenerator.GetInt32(0, 1000):D3}{RandomNumberGenerator.GetInt32(0, 1000):D3}";
    }

    private static string? NormalizeAuditValue(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalizedValue = value.Trim();
        return normalizedValue.Length <= maxLength
            ? normalizedValue
            : normalizedValue[..maxLength];
    }

    private static ActivityLog NewActivity(int userId, string eventType, string? entityType = null, int? entityId = null) => new()
    {
        IDUser = userId,
        EventType = eventType,
        EntityType = entityType,
        EntityId = entityId,
        OccurredAt = DateTime.UtcNow
    };

    private static EmailMessage BuildPasswordResetEmail(string email, string resetCode)
    {
        var plainTextBody = $"""
            Mã đặt lại mật khẩu Atomos của bạn là: {resetCode}

            Mã này có hiệu lực trong {ResetTokenExpirationMinutes} phút. Nếu bạn không yêu cầu đặt lại mật khẩu, hãy bỏ qua email này.
            """;

        var htmlBody = $"""
            <!doctype html>
            <html lang="vi">
            <body style="margin:0;background:#f4fff8;font-family:Segoe UI,Arial,sans-serif;color:#064e3b;">
              <div style="max-width:520px;margin:0 auto;padding:28px 18px;">
                <div style="background:#ffffff;border:1px solid #c9f7d8;border-radius:12px;padding:24px;">
                  <h1 style="font-size:22px;margin:0 0 12px;">Mã đặt lại mật khẩu Atomos</h1>
                  <p style="font-size:15px;line-height:1.6;margin:0 0 18px;">
                    Dùng mã xác thực bên dưới để tiếp tục đặt lại mật khẩu.
                  </p>
                  <div style="font-size:32px;font-weight:800;letter-spacing:8px;background:#ecfff3;border-radius:10px;padding:16px 18px;text-align:center;">
                    {resetCode}
                  </div>
                  <p style="font-size:14px;line-height:1.6;margin:18px 0 0;color:#49685b;">
                    Mã này có hiệu lực trong {ResetTokenExpirationMinutes} phút. Nếu bạn không yêu cầu đặt lại mật khẩu, hãy bỏ qua email này.
                  </p>
                </div>
              </div>
            </body>
            </html>
            """;

        return new EmailMessage
        {
            To = email,
            Subject = "Mã đặt lại mật khẩu Atomos",
            PlainTextBody = plainTextBody,
            HtmlBody = htmlBody
        };
    }

    private static ForgotPasswordResult? ValidateUsernameOrEmail(string usernameOrEmail)
    {
        if (usernameOrEmail.Contains('@', StringComparison.Ordinal))
        {
            if (usernameOrEmail.Any(char.IsWhiteSpace) ||
                usernameOrEmail.Length > 100 ||
                !EmailValidator.IsValid(usernameOrEmail))
            {
                return ForgotPasswordResult.Fail("Email không hợp lệ.", "EMAIL_INVALID");
            }

            return null;
        }

        if (usernameOrEmail.Length is < 3 or > 50)
        {
            return ForgotPasswordResult.Fail("Tên đăng nhập phải từ 3 đến 50 ký tự.", "USERNAME_LENGTH_INVALID");
        }

        if (!usernameOrEmail.All(char.IsLetterOrDigit))
        {
            return ForgotPasswordResult.Fail("Tên đăng nhập chỉ được gồm chữ và số.", "USERNAME_INVALID_CHARACTERS");
        }

        return null;
    }
}
