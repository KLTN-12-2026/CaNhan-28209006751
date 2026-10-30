namespace Atomos.Application.Learning;

public interface ILearningQueries
{
    Task<IReadOnlyList<ChapterReadModel>> GetChaptersAsync(int userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<LessonProgressReadModel>> GetProgressAsync(int userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<LessonReactionIllustration>> GetLessonIllustrationsAsync(int lessonId, CancellationToken cancellationToken);
}

public sealed record ChapterReadModel(
    int Id,
    string Title,
    string? Description,
    int Order,
    IReadOnlyList<LessonReadModel> Lessons);

public sealed record LessonReadModel(
    int Id,
    string Title,
    string Content,
    IReadOnlyList<LessonReactionIllustration> ReactionIllustrations,
    int Order,
    byte CompletionPercent,
    bool IsCompleted,
    DateTime? LastStudiedAt);

public sealed record LessonProgressReadModel(
    int LessonId,
    byte CompletionPercent,
    bool IsCompleted,
    DateTime? LastStudiedAt);
