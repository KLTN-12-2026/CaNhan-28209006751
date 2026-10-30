namespace Atomos.Domain.Learning;

public sealed class Lesson
{
    public int IDLesson { get; set; }
    public int IDChapter { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? ReactionIllustrationsJson { get; set; }
    public int Order { get; set; }
    public Chapter? Chapter { get; set; }
}
