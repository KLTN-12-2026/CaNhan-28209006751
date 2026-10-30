namespace Atomos.Domain.Learning;

public sealed class LearningAttemptDetail
{
    public int IDAttempt { get; set; }
    public int IDExercise { get; set; }
    public string? Answer { get; set; }
    public decimal Score { get; set; }
    public int Order { get; set; }
    public bool? IsCorrect { get; set; }
    public decimal MaxScore { get; set; } = 1;
    public bool HintViewed { get; set; }
    public bool SolutionViewed { get; set; }
    public string? QuestionSnapshot { get; set; }
    public string? CorrectAnswerSnapshot { get; set; }
    public string? SolutionSnapshot { get; set; }
    public string? ChoiceSnapshot { get; set; }
    public string? ExerciseTypeSnapshot { get; set; }
    public decimal? AllowedErrorSnapshot { get; set; }
    public LearningAttempt? Attempt { get; set; }
    public Exercise? Exercise { get; set; }
}
