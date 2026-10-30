namespace Atomos.Domain.Learning;

public sealed class ExerciseSet
{
    public int IDExerciseSet { get; set; }
    public int IDLesson { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Order { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public DateTime UpdatedAt { get; set; }
    public Lesson? Lesson { get; set; }
    public ICollection<ExerciseSetQuestion> Questions { get; set; } = [];
}
