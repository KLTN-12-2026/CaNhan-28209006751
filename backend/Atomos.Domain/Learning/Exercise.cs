namespace Atomos.Domain.Learning;

public sealed class Exercise
{
    public int IDExercise { get; set; }
    public int? IDLesson { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public string? Answer { get; set; }
    public string? Solution { get; set; }
    public string? ReviewHint { get; set; }
    public string? AnswerUnit { get; set; }
    public decimal? AllowedError { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime UpdatedAt { get; set; }
    public Lesson? Lesson { get; set; }
    public ICollection<ExerciseChoice> Choices { get; set; } = [];
    public ICollection<ExerciseSetQuestion> ExerciseSetQuestions { get; set; } = [];
    public ICollection<ExamExercise> ExamExercises { get; set; } = [];
}
