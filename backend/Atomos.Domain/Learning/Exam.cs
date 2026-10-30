namespace Atomos.Domain.Learning;

public sealed class Exam
{
    public int IDExam { get; set; }
    public int? IDChapter { get; set; }
    public int? IDLesson { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMinutes { get; set; } = 30;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public string QuestionSelectionMode { get; set; } = "Fixed";
    public int? QuestionCount { get; set; }
    public Chapter? Chapter { get; set; }
    public Lesson? Lesson { get; set; }
}
