using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Atomos.API.Security;
using Atomos.API.Controllers;
using Atomos.Application.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Builder;

namespace Atomos.Tests;

public sealed class AuthenticationSecurityTests
{
    [Fact]
    public void Login_UsesDedicatedRateLimitPolicy()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.Login));
        var attribute = Assert.Single(
            method!.GetCustomAttributes(typeof(EnableRateLimitingAttribute), inherit: true)
                .Cast<EnableRateLimitingAttribute>());

        Assert.Equal("auth-login", attribute.PolicyName);
    }

    [Fact]
    public void LegacyPbkdf2Hash_VerifiesAndRequestsRehash()
    {
        const string password = "LegacyPassword123!";
        var salt = RandomNumberGenerator.GetBytes(16);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        var hash = $"PBKDF2-SHA256.100000.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";

        Assert.True(PasswordHasher.Verify(password, hash, out var needsRehash));
        Assert.True(needsRehash);
        Assert.True(PasswordHasher.Verify(password, PasswordHasher.Hash(password), out needsRehash));
        Assert.False(needsRehash);
    }

    [Fact]
    public async Task GoogleVerifier_RejectsMalformedTokenInsteadOfThrowing()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["Authentication:Google:ClientId"] = "client" }).Build();
        var verifier = new GoogleTokenVerifier(configuration, null!, NullLogger<GoogleTokenVerifier>.Instance);

        Assert.Null(await verifier.VerifyAsync("not-a-jwt", CancellationToken.None));
        Assert.Null(await verifier.VerifyAsync("bad.bad.bad", CancellationToken.None));
    }

    [Fact]
    public async Task GoogleVerifierEnforcesRs256SignatureIssuerAudienceExpiryAndVerifiedEmail()
    {
        const string clientId = "atomos-google-client";
        const string keyId = "trusted-google-key";
        using var trustedKey = RSA.Create(2048);
        using var attackerKey = RSA.Create(2048);
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["Authentication:Google:ClientId"] = clientId }).Build();
        var verifier = new GoogleTokenVerifier(
            configuration,
            new StaticHttpClientFactory(GoogleJwks(trustedKey, keyId)),
            NullLogger<GoogleTokenVerifier>.Instance);

        var valid = await verifier.VerifyAsync(GoogleToken(trustedKey, keyId, clientId), default);
        var wrongAudience = await verifier.VerifyAsync(
            GoogleToken(trustedKey, keyId, "other-client"), default);
        var wrongIssuer = await verifier.VerifyAsync(
            GoogleToken(trustedKey, keyId, clientId, issuer: "https://attacker.example"), default);
        var expired = await verifier.VerifyAsync(
            GoogleToken(trustedKey, keyId, clientId, expiresAt: DateTimeOffset.UtcNow.AddMinutes(-1)), default);
        var unverified = await verifier.VerifyAsync(
            GoogleToken(trustedKey, keyId, clientId, emailVerified: false), default);
        var invalidSignature = await verifier.VerifyAsync(
            GoogleToken(attackerKey, keyId, clientId), default);

        Assert.NotNull(valid);
        Assert.Equal("google-subject", valid!.GoogleId);
        Assert.Equal("verified@example.test", valid.Email);
        Assert.Null(wrongAudience);
        Assert.Null(wrongIssuer);
        Assert.Null(expired);
        Assert.Null(unverified);
        Assert.Null(invalidSignature);
    }

    [Fact]
    public async Task ImageUpload_ValidatesCompleteStructureAndRejectsPolyglotTrailingData()
    {
        var pngBytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=");
        var valid = FormFile(pngBytes, "image/png");
        var spoofed = FormFile("not an image"u8.ToArray(), "image/jpeg");
        var mismatched = FormFile(pngBytes, "image/jpeg");
        var truncated = FormFile(pngBytes[..^4], "image/png");
        var polyglot = FormFile([.. pngBytes, .. "<script>alert(1)</script>"u8.ToArray()], "image/png");
        var headerOnlyWebP = FormFile([
            .. "RIFF"u8.ToArray(), 22, 0, 0, 0, .. "WEBPVP8X"u8.ToArray(), 10, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0], "image/webp");

        Assert.Equal(".png", await ImageUploadValidator.GetVerifiedExtensionAsync(valid, false, default));
        Assert.Null(await ImageUploadValidator.GetVerifiedExtensionAsync(spoofed, false, default));
        Assert.Null(await ImageUploadValidator.GetVerifiedExtensionAsync(mismatched, false, default));
        Assert.Null(await ImageUploadValidator.GetVerifiedExtensionAsync(truncated, false, default));
        Assert.Null(await ImageUploadValidator.GetVerifiedExtensionAsync(polyglot, false, default));
        Assert.Null(await ImageUploadValidator.GetVerifiedExtensionAsync(headerOnlyWebP, false, default));
    }

    [Fact]
    public void ForwardedHeaders_TrustOnlyConfiguredProxiesAndNetworks()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Proxy:KnownProxies:0"] = "10.0.0.5",
            ["Proxy:KnownNetworks:0"] = "10.20.0.0/16",
            ["Proxy:KnownNetworks:1"] = "invalid"
        }).Build();
        var options = new ForwardedHeadersOptions();

        TrustedProxyConfiguration.Apply(options, configuration);

        Assert.Equal(1, options.ForwardLimit);
        Assert.Equal("10.0.0.5", Assert.Single(options.KnownProxies).ToString());
        Assert.Equal("10.20.0.0/16", Assert.Single(options.KnownIPNetworks).ToString());
    }

    private static FormFile FormFile(byte[] bytes, string contentType) => new(new MemoryStream(bytes), 0, bytes.Length, "file", "upload.bin")
    {
        Headers = new HeaderDictionary(),
        ContentType = contentType
    };

    private static string GoogleToken(
        RSA signingKey,
        string keyId,
        string audience,
        string issuer = "https://accounts.google.com",
        DateTimeOffset? expiresAt = null,
        bool emailVerified = true)
    {
        var header = Base64Url(JsonSerializer.SerializeToUtf8Bytes(new { alg = "RS256", kid = keyId }));
        var payload = Base64Url(JsonSerializer.SerializeToUtf8Bytes(new
        {
            iss = issuer,
            aud = audience,
            exp = (expiresAt ?? DateTimeOffset.UtcNow.AddMinutes(5)).ToUnixTimeSeconds(),
            sub = "google-subject",
            email = "verified@example.test",
            email_verified = emailVerified,
            name = "Verified User"
        }));
        var signed = $"{header}.{payload}";
        var signature = signingKey.SignData(
            Encoding.ASCII.GetBytes(signed),
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        return $"{signed}.{Base64Url(signature)}";
    }

    private static string GoogleJwks(RSA key, string keyId)
    {
        var parameters = key.ExportParameters(false);
        return JsonSerializer.Serialize(new
        {
            keys = new[]
            {
                new
                {
                    kid = keyId,
                    kty = "RSA",
                    alg = "RS256",
                    use = "sig",
                    n = Base64Url(parameters.Modulus!),
                    e = Base64Url(parameters.Exponent!)
                }
            }
        });
    }

    private static string Base64Url(byte[] bytes) => Convert.ToBase64String(bytes)
        .TrimEnd('=')
        .Replace('+', '-')
        .Replace('/', '_');

    private sealed class StaticHttpClientFactory(string responseBody) : IHttpClientFactory
    {
        private readonly HttpClient client = new(new StaticResponseHandler(responseBody));
        public HttpClient CreateClient(string name) => client;
    }

    private sealed class StaticResponseHandler(string responseBody) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) => Task.FromResult(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            });
    }
}
