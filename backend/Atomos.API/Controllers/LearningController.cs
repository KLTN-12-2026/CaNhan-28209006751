using System.Security.Claims;
using Atomos.API.Security;
using Atomos.Application.Common;
using Atomos.Application.Learning;
using Atomos.Domain.Admin;
using Atomos.Domain.Learning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Atomos.API.Controllers;

[ApiController]
[Authorize(Policy = "StudentOnly")]
[Route("api/learning")]
public sealed class LearningController(
    IApplicationDbContext dbContext,
    ILearningQueries learningQueries) : ControllerBase
{
    [HttpGet("chapters")]
    public async Task<ActionResult<List<ChapterResponse>>> GetChapters(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var chapters = await learningQueries.GetChaptersAsync(userId, cancellationToken);
        return Ok(chapters.Select(chapter => new ChapterResponse(
            chapter.Id,
            chapter.Title,
            chapter.Description,
            chapter.Order,
            chapter.Lessons.Select(lesson => new LessonResponse(
                lesson.Id,
                lesson.Title,
                RichContentSanitizer.Sanitize(lesson.Content),
                lesson.ReactionIllustrations,
                lesson.Order,
                lesson.CompletionPercent,
                lesson.IsCompleted,
                lesson.LastStudiedAt)).ToList())).ToList());
    }

    [HttpGet("progress")]
    public async Task<ActionResult<List<LessonProgressResponse>>> GetProgress(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var progress = await learningQueries.GetProgressAsync(userId, cancellationToken);

        return Ok(progress.Select(item => new LessonProgressResponse(
            item.LessonId,
            item.CompletionPercent,
            item.IsCompleted,
            item.LastStudiedAt)).ToList());
    }

    [HttpPut("lessons/{lessonId:int}/progress")]
    public async Task<ActionResult<LessonProgressResponse>> SaveProgress(
        int lessonId,
        SaveLessonProgressRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var lessonExists = await dbContext.Lessons
            .AsNoTracking()
            .AnyAsync(lesson => lesson.IDLesson == lessonId, cancellationToken);
        if (!lessonExists) return NotFound();

        var percent = Math.Clamp(request.CompletionPercent, 0, 100);
        var isCompleted = request.IsCompleted || percent >= 100;
        if (isCompleted) percent = 100;

        var progress = await dbContext.LessonProgresses
            .SingleOrDefaultAsync(item => item.IDUser == userId && item.IDLesson == lessonId, cancellationToken);
        if (progress is null)
        {
            progress = new LessonProgress
            {
                IDUser = userId,
                IDLesson = lessonId
            };
            dbContext.LessonProgresses.Add(progress);
        }

        progress.CompletionPercent = (byte)Math.Max((int)progress.CompletionPercent, percent);
        progress.IsCompleted = progress.IsCompleted || isCompleted;
        progress.LastStudiedAt = DateTime.UtcNow;
        dbContext.ActivityLogs.Add(new ActivityLog
        {
            IDUser = userId,
            EventType = "OPEN_LESSON",
            EntityType = "Lesson",
            EntityId = lessonId,
            OccurredAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new LessonProgressResponse(
            progress.IDLesson,
            progress.CompletionPercent,
            progress.IsCompleted,
            progress.LastStudiedAt));
    }

    private bool TryGetUserId(out int userId) =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
}

public sealed record ChapterResponse(
    int Id,
    string Title,
    string? Description,
    int Order,
    List<LessonResponse> Lessons);

public sealed record LessonResponse(
    int Id,
    string Title,
    string Content,
    IReadOnlyList<LessonReactionIllustration> ReactionIllustrations,
    int Order,
    byte CompletionPercent,
    bool IsCompleted,
    DateTime? LastStudiedAt);

public sealed record LessonProgressResponse(
    int LessonId,
    byte CompletionPercent,
    bool IsCompleted,
    DateTime? LastStudiedAt);

public sealed record SaveLessonProgressRequest(int CompletionPercent, bool IsCompleted);
