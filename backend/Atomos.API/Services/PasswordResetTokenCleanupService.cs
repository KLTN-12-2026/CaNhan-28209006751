using Atomos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Atomos.API.Services;

public sealed class PasswordResetTokenCleanupService(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<PasswordResetTokenCleanupService> logger) : BackgroundService
{
    private const int RetentionDays = 30;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(GetDelayUntilNextMidnight(), stoppingToken);
                await CleanupAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    private static TimeSpan GetDelayUntilNextMidnight()
    {
        var now = DateTime.Now;
        var nextMidnight = now.Date.AddDays(1);

        return nextMidnight - now;
    }

    private async Task CleanupAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = serviceScopeFactory.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AtomosDbContext>();
            var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
            var cutoff = now.AddDays(-RetentionDays);

            var deletedCount = await dbContext.PasswordResetTokens
                .Where(resetToken =>
                    resetToken.NgayTao < cutoff &&
                    (resetToken.DaSuDung || resetToken.HetHan < now))
                .ExecuteDeleteAsync(cancellationToken);

            if (deletedCount > 0)
            {
                logger.LogInformation("Cleaned up {DeletedCount} password reset token audit records.", deletedCount);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Password reset token cleanup failed.");
        }
    }
}
