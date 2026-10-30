namespace Atomos.Domain.Learning;

public sealed class Chapter
{
    public int IDChapter { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Order { get; set; }
    public ICollection<Lesson> Lessons { get; set; } = [];
}
