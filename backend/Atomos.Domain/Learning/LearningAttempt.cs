using Atomos.Domain.Users;

namespace Atomos.Domain.Learning;

public sealed class LearningAttempt
{
    public int IDAttempt { get; set; }
    public int IDUser { get; set; }
    public int? IDExam { get; set; }
    public int? IDLesson { get; set; }
    public int? IDExerciseSet { get; set; }
    public string AttemptType { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft";
    public DateTime StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public int? DurationSeconds { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DueAt { get; set; }
    public decimal? TotalScore { get; set; }
    public int CorrectCount { get; set; }
    public int WrongCount { get; set; }
    public decimal? PercentCorrect { get; set; }
    public User? User { get; set; }
    public Exam? Exam { get; set; }
    public Lesson? Lesson { get; set; }
    public ExerciseSet? ExerciseSet { get; set; }
}
