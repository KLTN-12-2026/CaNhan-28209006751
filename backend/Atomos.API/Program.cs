using System.Diagnostics;
using Atomos.API.Email;
using Atomos.Application.Auth;
using Atomos.Application.Common;
using Atomos.Application.Learning;
using Atomos.Application.Lookup;
using Atomos.Infrastructure.Persistence;
using Atomos.Infrastructure.Queries;
using Atomos.API.Services;
using Atomos.API.Security;
using Atomos.API.Seeding;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging.Console;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options =>
{
    options.ColorBehavior = LoggerColorBehavior.Enabled;
    options.SingleLine = true;
    options.TimestampFormat = "HH:mm:ss ";
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddAntiforgery(AuthAntiforgeryConfiguration.Configure);
builder.Services.Configure<ForwardedHeadersOptions>(options =>
    TrustedProxyConfiguration.Apply(options, builder.Configuration));
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth-login", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

var frontendOrigins = FrontendCorsPolicy.ReadAllowedOrigins(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        FrontendCorsPolicy.Name,
        policy => FrontendCorsPolicy.Configure(policy, frontendOrigins));
});

builder.Services.AddDbContext<AtomosDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "Connection string DefaultConnection chưa được cấu hình.");
    }

    options.UseSqlServer(connectionString, sqlServer =>
        sqlServer.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
});

builder.Services.AddScoped<IApplicationDbContext>(provider =>
    provider.GetRequiredService<AtomosDbContext>());
builder.Services.AddScoped<IApplicationDbTransactionFactory, EfApplicationDbTransactionFactory>();
builder.Services.AddScoped<ISqlConnectionFactory, SqlConnectionFactory>();
builder.Services.AddScoped<DapperTransactionContext>();
builder.Services.AddScoped<ITransactionRunner, DapperTransactionRunner>();
builder.Services.AddScoped<IPasswordResetTransactions, PasswordResetTransactions>();
builder.Services.AddScoped<IAuthSessionMutations, AuthSessionMutations>();
var authSessionOptions = builder.Configuration
    .GetSection(AuthSessionOptions.SectionName)
    .Get<AuthSessionOptions>() ?? new AuthSessionOptions();
if (authSessionOptions.LifetimeMinutes <= 0)
    throw new InvalidOperationException("AuthSession:LifetimeMinutes must be positive.");
builder.Services.AddSingleton(authSessionOptions);
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IAuthSessionService, AuthSessionService>();
builder.Services.AddScoped<AuthSessionCookieEvents>();
builder.Services.AddScoped<AuthSessionCookieIssuer>();
builder.Services.AddScoped<IGoogleTokenVerifier, GoogleTokenVerifier>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAuthQueries, AuthQueries>();
builder.Services.AddScoped<ILookupQueries, LookupQueries>();
builder.Services.AddScoped<LookupService>();
builder.Services.AddScoped<ILearningQueries, LearningQueries>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddHostedService<PasswordResetTokenCleanupService>();
builder.Services.AddAuthentication(AtomosAuthenticationDefaults.CookieScheme)
    .AddCookie(
        AtomosAuthenticationDefaults.CookieScheme,
        options => AuthCookieConfiguration.Configure(options, authSessionOptions.Lifetime));
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("StudentOnly", policy => policy.RequireRole("Student"));
});

var app = builder.Build();
await app.EnsureAuthRolesAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var apiLogger = app.Services
    .GetRequiredService<ILoggerFactory>()
    .CreateLogger("Atomos.API.Request");

app.UseForwardedHeaders();
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    if (!context.Request.Path.StartsWithSegments("/api"))
    {
        await next();
        return;
    }

    var stopwatch = Stopwatch.StartNew();
    var requestId = context.TraceIdentifier;

    apiLogger.LogInformation(
        "HTTP {Method} {Path} started | RequestId: {RequestId}",
        context.Request.Method,
        context.Request.Path,
        requestId);

    try
    {
        await next();

        var logLevel = context.Response.StatusCode >= 500
            ? LogLevel.Error
            : context.Response.StatusCode >= 400
                ? LogLevel.Warning
                : LogLevel.Information;

        apiLogger.Log(
            logLevel,
            "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds} ms | RequestId: {RequestId}",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds,
            requestId);
    }
    catch (Exception exception)
    {
        apiLogger.LogError(
            exception,
            "HTTP {Method} {Path} failed in {ElapsedMilliseconds} ms | RequestId: {RequestId}",
            context.Request.Method,
            context.Request.Path,
            stopwatch.ElapsedMilliseconds,
            requestId);
        throw;
    }
});

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseRouting();
app.UseCors(FrontendCorsPolicy.Name);
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();
app.UseAntiforgery();
app.UseMiddleware<CookieAntiforgeryMiddleware>();
app.MapControllers();

app.Run();
