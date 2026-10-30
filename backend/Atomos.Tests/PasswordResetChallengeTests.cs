using Atomos.Application.Auth;
using Atomos.Domain.Users;

namespace Atomos.Tests;

public sealed class PasswordResetChallengeTests
{
    private static readonly DateTime Now = new(2026, 9, 23, 12, 0, 0);
    private static PasswordResetToken NewToken() => new()
    {
        IDUser = 1, Token = "123456", NgayTao = Now, HetHan = Now.AddMinutes(30)
    };

    [Fact]
    public void WrongAttemptsExhaustBudgetAndCorrectCodeCannotUnlock()
    {
        var token = NewToken();
        for (var attempt = 1; attempt <= PasswordResetChallenge.MaxFailedAttempts; attempt++)
        {
            Assert.False(PasswordResetChallenge.TryAuthenticate(token, "654321", Now, true));
            Assert.Equal(attempt, token.FailedAttempts);
        }
        Assert.True(token.DaSuDung);
        Assert.False(PasswordResetChallenge.TryAuthenticate(token, "123456", Now, true));
        Assert.False(PasswordResetChallenge.TryAuthenticate(token, "123456", Now, false));
        Assert.Equal(5, token.FailedAttempts);
    }

    [Fact]
    public void VerificationExchangesOtpForHashedSingleUseGrant()
    {
        var token = NewToken();
        Assert.True(PasswordResetChallenge.TryAuthenticate(token, "123456", Now, true));
        var grant = PasswordResetChallenge.ExchangeOtpForGrant(token);
        Assert.True(PasswordResetChallenge.IsGrant(grant));
        Assert.NotEqual(grant, token.Token);
        Assert.False(PasswordResetChallenge.TryAuthenticate(token, "123456", Now, true));
        Assert.False(PasswordResetChallenge.TryAuthenticate(token, token.Token, Now, false));
        Assert.True(PasswordResetChallenge.TryAuthenticate(token, grant, Now, false));
        token.DaSuDung = true;
        Assert.False(PasswordResetChallenge.TryAuthenticate(token, grant, Now, false));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ExpiredOtpAndGrantAreRejected(int minutes)
    {
        var token = NewToken();
        token.HetHan = Now.AddMinutes(minutes);
        Assert.False(PasswordResetChallenge.TryAuthenticate(token, "123456", Now, true));
        var grant = PasswordResetChallenge.ExchangeOtpForGrant(token);
        Assert.False(PasswordResetChallenge.TryAuthenticate(token, grant, Now, false));
    }

    [Fact]
    public void AttemptsAreIsolatedAndNewChallengeHasItsOwnBudget()
    {
        var first = NewToken();
        var second = NewToken();
        second.IDUser = 2;
        for (var i = 0; i < 5; i++) PasswordResetChallenge.TryAuthenticate(first, "654321", Now, false);
        Assert.True(PasswordResetChallenge.TryAuthenticate(second, "123456", Now, true));
        var replacement = NewToken();
        replacement.Token = "654321";
        Assert.False(PasswordResetChallenge.TryAuthenticate(first, "123456", Now, true));
        Assert.True(PasswordResetChallenge.TryAuthenticate(replacement, "654321", Now, true));
        Assert.Equal(0, replacement.FailedAttempts);
    }

    [Theory]
    [InlineData("１２３４５６")]
    [InlineData("12345")]
    [InlineData("1234567")]
    [InlineData("12 456")]
    [InlineData("abcdef")]
    public void OtpFormatIsAsciiAndFixedLength(string value) => Assert.False(PasswordResetChallenge.IsOtp(value));
}
