namespace Atomos.API.Security;

[AttributeUsage(AttributeTargets.Method)]
public sealed class RequireCookieAntiforgeryAttribute : Attribute;

[AttributeUsage(AttributeTargets.Method)]
public sealed class SkipCookieAntiforgeryAttribute : Attribute;
