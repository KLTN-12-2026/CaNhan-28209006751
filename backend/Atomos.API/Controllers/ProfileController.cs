using System.Security.Claims;
using Atomos.Application.Auth;
using Atomos.Application.Common;
using Atomos.API.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Atomos.API.Controllers;

[ApiController]
[Authorize(Policy = "StudentOnly")]
[Route("api/profile")]
public sealed class ProfileController(
    IApplicationDbContext dbContext,
    IAuthService authService,
    IAuthSessionService sessions,
    IApplicationDbTransactionFactory transactionFactory,
    IWebHostEnvironment environment) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ProfileResponse>> Get(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var user = await dbContext.Users.AsNoTracking()
            .SingleOrDefaultAsync(item => item.IDUser == userId, cancellationToken);
        if (user is null) return NotFound();

        return Ok(ToProfile(user.IDUser, user.Username, user.Email, user.HoTen, user.Avatar, user.MatKhau is null, user.VipExpiresAt));
    }

    [HttpPut]
    public async Task<ActionResult<ProfileResponse>> Update(UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var fullName = string.Join(' ', (request.FullName ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries));
        if (string.IsNullOrWhiteSpace(fullName)) return BadRequest(new { message = "Vui lòng nhập họ tên." });
        if (fullName.Length > 100) return BadRequest(new { message = "Họ tên không được vượt quá 100 ký tự." });
        if (!fullName.All(character => char.IsLetter(character) || char.IsWhiteSpace(character)))
        {
            return BadRequest(new { message = "Họ tên chỉ được gồm chữ." });
        }

        var user = await dbContext.Users.SingleOrDefaultAsync(item => item.IDUser == userId, cancellationToken);
        if (user is null) return NotFound();

        user.HoTen = fullName;
        user.NgayCapNhat = Now();
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToProfile(user.IDUser, user.Username, user.Email, user.HoTen, user.Avatar, user.MatKhau is null, user.VipExpiresAt));
    }

    [HttpPost("avatar")]
    [RequestSizeLimit(2_500_000)]
    public async Task<ActionResult<AvatarResponse>> UploadAvatar(IFormFile file, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        if (file.Length is <= 0 or > 2_000_000) return BadRequest(new { message = "Ảnh đại diện tối đa 2MB." });
        var extension = await ImageUploadValidator.GetVerifiedExtensionAsync(file, allowGif: false, cancellationToken);
        if (extension is null) return BadRequest(new { message = "Tệp không phải ảnh JPG, PNG hoặc WEBP hợp lệ." });

        var user = await dbContext.Users.SingleOrDefaultAsync(item => item.IDUser == userId, cancellationToken);
        if (user is null) return NotFound();

        DeleteLocalAvatar(user.Avatar);

        var folder = Path.Combine(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"), "uploads", "avatars");
        Directory.CreateDirectory(folder);
        var fileName = $"{userId}-{Guid.NewGuid():N}{extension}";
        var path = Path.Combine(folder, fileName);
        await using (var stream = System.IO.File.Create(path))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        user.Avatar = $"/uploads/avatars/{fileName}";
        user.NgayCapNhat = Now();
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new AvatarResponse(user.Avatar));
    }

    [HttpDelete("avatar")]
    public async Task<IActionResult> DeleteAvatar(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var user = await dbContext.Users.SingleOrDefaultAsync(item => item.IDUser == userId, cancellationToken);
        if (user is null) return NotFound();

        DeleteLocalAvatar(user.Avatar);
        user.Avatar = null;
        user.NgayCapNhat = Now();
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("password/change")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8 ||
            !request.NewPassword.Any(char.IsLetter) || !request.NewPassword.Any(char.IsDigit))
        {
            return BadRequest(new { message = "Mật khẩu mới phải có ít nhất 8 ký tự, gồm cả chữ và số." });
        }

        await using var transaction = await transactionFactory.BeginAsync(cancellationToken);
        var user = await dbContext.Users.SingleOrDefaultAsync(item => item.IDUser == userId, cancellationToken);
        if (user is null) return NotFound();
        if (user.MatKhau is null) return Conflict(new { message = "Tài khoản này cần xác thực email trước khi đặt mật khẩu." });
        if (string.IsNullOrWhiteSpace(request.CurrentPassword) || !PasswordHasher.Verify(request.CurrentPassword, user.MatKhau))
        {
            return BadRequest(new { message = "Mật khẩu cũ không đúng." });
        }

        if (IsReusedPassword(request.NewPassword, user.MatKhau, user.MatKhauTruocDo))
        {
            return BadRequest(new
            {
                message = "Mật khẩu mới không được trùng mật khẩu hiện tại hoặc mật khẩu ngay trước đó.",
                ErrorCode = "PASSWORD_REUSED"
            });
        }

        user.MatKhauTruocDo = user.MatKhau;
        user.MatKhau = PasswordHasher.Hash(request.NewPassword);
        user.NgayCapNhat = Now();
        await dbContext.SaveChangesAsync(cancellationToken);
        await sessions.RevokeAllUserSessionsAsync(
            userId,
            AuthSessionRevokeReasons.PasswordChanged,
            cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Ok(new { message = "Đổi mật khẩu thành công." });
    }

    [HttpPost("password/request-code")]
    public async Task<IActionResult> RequestPasswordCode(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var email = await GetUserEmail(userId, cancellationToken);
        if (email is null) return NotFound();

        var result = await authService.ForgotPasswordAsync(new ForgotPasswordRequest
        {
            UsernameOrEmail = email,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString()
        }, cancellationToken);

        return result.Success ? Ok(new { result.Data?.Message }) : BadRequest(new { result.Message, ErrorCode = result.Code });
    }

    [HttpPost("password/verify-code")]
    public async Task<IActionResult> VerifyPasswordCode(VerifyProfilePasswordCodeRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var email = await GetUserEmail(userId, cancellationToken);
        if (email is null) return NotFound();

        var result = await authService.VerifyResetCodeAsync(new VerifyResetCodeRequest
        {
            UsernameOrEmail = email,
            Code = request.Code
        }, cancellationToken);

        return result.Success ? Ok(new { result.Data?.Message }) : BadRequest(new { result.Message, ErrorCode = result.Code });
    }

    [HttpPost("password/set")]
    public async Task<IActionResult> SetPassword(SetProfilePasswordRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var email = await GetUserEmail(userId, cancellationToken);
        if (email is null) return NotFound();

        var result = await authService.ResetPasswordAsync(new ResetPasswordRequest
        {
            UsernameOrEmail = email,
            Code = request.Code,
            Password = request.NewPassword
        }, cancellationToken);

        return result.Success ? Ok(new { result.Data?.Message }) : BadRequest(new { result.Message, ErrorCode = result.Code });
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<ProfileStatisticsResponse>> GetStatistics(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var profile = await dbContext.LearningProfiles.AsNoTracking()
            .SingleOrDefaultAsync(item => item.IDUser == userId, cancellationToken);

        var attempts = await dbContext.LearningAttempts.AsNoTracking()
            .Include(item => item.Lesson)
            .Where(item => item.IDUser == userId && (item.Status == "Submitted" || item.Status == "Expired"))
            .OrderByDescending(item => item.SubmittedAt ?? item.UpdatedAt)
            .ToListAsync(cancellationToken);

        var attemptIds = attempts.Select(attempt => attempt.IDAttempt).ToList();
        var details = await dbContext.LearningAttemptDetails.AsNoTracking()
            .Where(detail => attemptIds.Contains(detail.IDAttempt))
            .ToListAsync(cancellationToken);
        var attemptById = attempts.ToDictionary(item => item.IDAttempt);

        var lessonStats = details
            .Where(detail => attemptById.TryGetValue(detail.IDAttempt, out var attempt) && attempt.IDLesson is not null)
            .GroupBy(detail => attemptById[detail.IDAttempt].IDLesson!.Value)
            .Select(group =>
            {
                var attempt = attemptById[group.First().IDAttempt];
                var lessonAttempts = group
                    .Select(detail => attemptById[detail.IDAttempt])
                    .DistinctBy(item => item.IDAttempt)
                    .ToList();
                var total = group.Count(item => item.IsCorrect is not null);
                var correct = group.Count(item => item.IsCorrect == true);
                var percent = total == 0 ? 0 : Math.Round(correct * 100m / total, 2);
                var durations = lessonAttempts
                    .Where(item => item.DurationSeconds is not null && item.DurationSeconds > 0)
                    .Select(item => item.DurationSeconds!.Value)
                    .ToList();
                var averageDuration = durations.Count == 0 ? null : (int?)Convert.ToInt32(durations.Average());
                return new LessonSkillResponse(
                    group.Key,
                    attempt.Lesson?.Title ?? $"Bài {group.Key}",
                    total,
                    correct,
                    percent,
                    lessonAttempts.Count,
                    averageDuration,
                    BuildInsight(percent, averageDuration));
            })
            .Where(item => item.TotalAnswered > 0)
            .ToList();

        var strengths = lessonStats
            .Where(item => item.PercentCorrect >= 70)
            .OrderByDescending(item => item.PercentCorrect)
            .ThenByDescending(item => item.TotalAnswered)
            .ThenBy(item => item.AverageDurationSeconds ?? int.MaxValue)
            .Take(5)
            .ToList();

        var weaknesses = lessonStats
            .Where(item => item.PercentCorrect < 70)
            .OrderBy(item => item.PercentCorrect)
            .ThenByDescending(item => item.TotalAnswered)
            .ThenByDescending(item => item.AverageDurationSeconds ?? 0)
            .Take(5)
            .ToList();

        return Ok(new ProfileStatisticsResponse(
            profile?.CurrentStreak ?? 0,
            profile?.BestStreak ?? 0,
            Average(attempts),
            Average(attempts.Where(item => item.AttemptType is "Practice" or "DailyReview")),
            Average(attempts.Where(item => item.AttemptType == "Test")),
            strengths,
            weaknesses));
    }

    [HttpGet("attempt-history")]
    public async Task<ActionResult<List<AttemptHistoryResponse>>> GetAttemptHistory(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var history = await dbContext.LearningAttempts.AsNoTracking()
            .Include(item => item.Lesson)
            .Include(item => item.Exam)
            .Where(item => item.IDUser == userId && (item.Status == "Submitted" || item.Status == "Expired"))
            .OrderByDescending(item => item.SubmittedAt ?? item.UpdatedAt)
            .Take(20)
            .Select(item => new AttemptHistoryResponse(
                item.IDAttempt,
                item.AttemptType,
                item.Lesson != null ? item.Lesson.Title : item.Exam != null ? item.Exam.Title : "Ôn tập",
                item.Status,
                item.StartedAt,
                item.SubmittedAt,
                item.DurationSeconds,
                item.CorrectCount,
                item.WrongCount,
                item.PercentCorrect))
            .ToListAsync(cancellationToken);

        return Ok(history);
    }

    [HttpGet("attempt-history/{attemptId:int}/review")]
    public async Task<ActionResult<AttemptReviewResponse>> GetAttemptReview(int attemptId, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var attempt = await dbContext.LearningAttempts.AsNoTracking()
            .Include(item => item.Lesson)
            .Include(item => item.Exam)
            .SingleOrDefaultAsync(item =>
                item.IDAttempt == attemptId &&
                item.IDUser == userId &&
                (item.Status == "Submitted" || item.Status == "Expired"),
                cancellationToken);
        if (attempt is null) return NotFound();

        var items = await dbContext.LearningAttemptDetails.AsNoTracking()
            .Where(item => item.IDAttempt == attemptId)
            .OrderBy(item => item.Order)
            .Select(item => new AttemptReviewItemResponse(
                item.Order,
                item.QuestionSnapshot ?? string.Empty,
                item.Answer,
                item.CorrectAnswerSnapshot,
                item.IsCorrect ?? false,
                item.SolutionSnapshot))
            .ToListAsync(cancellationToken);

        return Ok(new AttemptReviewResponse(
            attempt.IDAttempt,
            attempt.AttemptType,
            attempt.Lesson != null ? attempt.Lesson.Title : attempt.Exam != null ? attempt.Exam.Title : "Ôn tập",
            attempt.CorrectCount,
            attempt.WrongCount,
            attempt.PercentCorrect ?? 0,
            items));
    }

    private async Task<string?> GetUserEmail(int userId, CancellationToken cancellationToken) =>
        await dbContext.Users.AsNoTracking()
            .Where(item => item.IDUser == userId)
            .Select(item => item.Email)
            .SingleOrDefaultAsync(cancellationToken);

    private void DeleteLocalAvatar(string? avatar)
    {
        if (string.IsNullOrWhiteSpace(avatar) || !avatar.StartsWith("/uploads/avatars/", StringComparison.OrdinalIgnoreCase)) return;

        var webRoot = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        var path = Path.Combine(webRoot, avatar.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
    }

    private static decimal Average(IEnumerable<Atomos.Domain.Learning.LearningAttempt> attempts)
    {
        var values = attempts.Where(item => item.PercentCorrect is not null).Select(item => item.PercentCorrect!.Value).ToList();
        return values.Count == 0 ? 0 : Math.Round(values.Average(), 2);
    }

    private static string BuildInsight(decimal percent, int? averageDurationSeconds)
    {
        var speed = averageDurationSeconds switch
        {
            null => "chưa đủ dữ liệu thời gian",
            <= 120 => "làm nhanh",
            <= 600 => "tốc độ ổn",
            _ => "còn mất nhiều thời gian"
        };

        return percent switch
        {
            >= 85 => $"Nắm chắc bài, {speed}.",
            >= 70 => $"Kết quả ổn, {speed}.",
            >= 50 => $"Đang lưng chừng, {speed}; nên ôn thêm.",
            _ => $"Cần ưu tiên ôn lại, {speed}."
        };
    }

    private static ProfileResponse ToProfile(int id, string username, string email, string? fullName, string? avatar, bool requiresEmailPasswordSet, DateTime? vipExpiresAt) =>
        new(id, username, email, fullName, avatar, requiresEmailPasswordSet, vipExpiresAt);

    private static DateTime Now() => DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

    private static bool IsReusedPassword(string password, string? currentHash, string? previousHash)
    {
        return (currentHash is not null && PasswordHasher.Verify(password, currentHash)) ||
               (previousHash is not null && PasswordHasher.Verify(password, previousHash));
    }

    private bool TryGetUserId(out int userId) =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
}

public sealed record ProfileResponse(
    int Id,
    string Username,
    string Email,
    string? FullName,
    string? Avatar,
    bool RequiresEmailPasswordSet,
    DateTime? VipExpiresAt);

public sealed record UpdateProfileRequest(string? FullName);
public sealed record AvatarResponse(string Avatar);
public sealed record ChangePasswordRequest(string? CurrentPassword, string NewPassword);
public sealed record VerifyProfilePasswordCodeRequest(string Code);
public sealed record SetProfilePasswordRequest(string Code, string NewPassword);

public sealed record ProfileStatisticsResponse(
    int CurrentStreak,
    int BestStreak,
    decimal OverallPercent,
    decimal PracticePercent,
    decimal ExamPercent,
    List<LessonSkillResponse> Strengths,
    List<LessonSkillResponse> Weaknesses);

public sealed record LessonSkillResponse(
    int LessonId,
    string LessonTitle,
    int TotalAnswered,
    int CorrectCount,
    decimal PercentCorrect,
    int AttemptCount,
    int? AverageDurationSeconds,
    string Insight);

public sealed record AttemptHistoryResponse(
    int AttemptId,
    string AttemptType,
    string Title,
    string Status,
    DateTime StartedAt,
    DateTime? SubmittedAt,
    int? DurationSeconds,
    int CorrectCount,
    int WrongCount,
    decimal? PercentCorrect);

public sealed record AttemptReviewResponse(
    int AttemptId,
    string AttemptType,
    string Title,
    int CorrectCount,
    int WrongCount,
    decimal PercentCorrect,
    List<AttemptReviewItemResponse> Items);

public sealed record AttemptReviewItemResponse(
    int Order,
    string Question,
    string? Answer,
    string? CorrectAnswer,
    bool IsCorrect,
    string? Solution);
