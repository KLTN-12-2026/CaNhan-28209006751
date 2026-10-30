using Atomos.Application.Learning;
using Atomos.Infrastructure.Persistence;
using Dapper;

namespace Atomos.Infrastructure.Queries;

public sealed class LearningQueries(ISqlConnectionFactory connectionFactory) : ILearningQueries
{
    public async Task<IReadOnlyList<ChapterReadModel>> GetChaptersAsync(int userId, CancellationToken cancellationToken)
    {
        const string chaptersSql = """
            SELECT
                IDChuong AS Id,
                TenChuong AS Title,
                MoTa AS Description,
                ThuTu AS [Order]
            FROM dbo.ChuongHoc
            ORDER BY ThuTu;
            """;

        const string lessonsSql = """
            SELECT
                IDBaiHoc AS Id,
                IDChuong AS ChapterId,
                TenBaiHoc AS Title,
                NoiDung AS Content,
                MinhHoaPhanUngJson AS ReactionIllustrationsJson,
                ThuTu AS [Order]
            FROM dbo.BaiHoc
            ORDER BY IDChuong, ThuTu;
            """;

        const string progressSql = """
            SELECT
                IDBaiHoc AS LessonId,
                PhanTramHoanThanh AS CompletionPercent,
                DaHoanThanh AS IsCompleted,
                LanHocGanNhat AS LastStudiedAt
            FROM dbo.TienDoHocTap
            WHERE IDUser = @UserId;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var chapters = (await connection.QueryAsync<ChapterRow>(
            new CommandDefinition(chaptersSql, cancellationToken: cancellationToken))).ToList();
        var lessons = (await connection.QueryAsync<LessonRow>(
            new CommandDefinition(lessonsSql, cancellationToken: cancellationToken))).ToList();
        var progress = (await connection.QueryAsync<ProgressRow>(
            new CommandDefinition(progressSql, new { UserId = userId }, cancellationToken: cancellationToken)))
            .ToDictionary(item => item.LessonId);

        var lessonsByChapter = lessons
            .GroupBy(lesson => lesson.ChapterId)
            .ToDictionary(group => group.Key, group => group
                .Select(lesson =>
                {
                    progress.TryGetValue(lesson.Id, out var lessonProgress);
                    return new LessonReadModel(
                        lesson.Id,
                        lesson.Title,
                        lesson.Content ?? string.Empty,
                        LessonReactionIllustrations.Deserialize(lesson.ReactionIllustrationsJson),
                        lesson.Order,
                        lessonProgress?.CompletionPercent ?? 0,
                        lessonProgress?.IsCompleted ?? false,
                        lessonProgress?.LastStudiedAt);
                })
                .ToList());

        return chapters
            .Select(chapter => new ChapterReadModel(
                chapter.Id,
                chapter.Title,
                chapter.Description,
                chapter.Order,
                lessonsByChapter.TryGetValue(chapter.Id, out var chapterLessons)
                    ? chapterLessons
                    : []))
            .ToList();
    }

    public async Task<IReadOnlyList<LessonProgressReadModel>> GetProgressAsync(int userId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                IDBaiHoc AS LessonId,
                PhanTramHoanThanh AS CompletionPercent,
                DaHoanThanh AS IsCompleted,
                LanHocGanNhat AS LastStudiedAt
            FROM dbo.TienDoHocTap
            WHERE IDUser = @UserId
            ORDER BY IDBaiHoc;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<LessonProgressReadModel>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));
        return rows.ToList();
    }

    public async Task<IReadOnlyList<LessonReactionIllustration>> GetLessonIllustrationsAsync(
        int lessonId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT MinhHoaPhanUngJson
            FROM dbo.BaiHoc
            WHERE IDBaiHoc = @LessonId;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var json = await connection.QuerySingleOrDefaultAsync<string?>(
            new CommandDefinition(sql, new { LessonId = lessonId }, cancellationToken: cancellationToken));
        return LessonReactionIllustrations.Deserialize(json);
    }

    private sealed record ChapterRow(int Id, string Title, string? Description, int Order);

    private sealed record LessonRow(
        int Id,
        int ChapterId,
        string Title,
        string? Content,
        string? ReactionIllustrationsJson,
        int Order);

    private sealed record ProgressRow(int LessonId, byte CompletionPercent, bool IsCompleted, DateTime? LastStudiedAt);
}
