namespace Atomos.Application.Auth;

public static class AuthSessionRevokeReasons
{
    public const string Logout = "Logout";
    public const string PasswordReset = "Password reset";
    public const string PasswordChanged = "Password changed";
    public const string AccountLocked = "Account locked";
    public const string RoleChanged = "Role changed";
    public const string AccountDeleted = "Account deleted";
}
