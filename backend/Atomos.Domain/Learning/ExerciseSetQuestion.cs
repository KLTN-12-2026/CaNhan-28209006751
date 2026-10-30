namespace Atomos.Domain.Learning;

public sealed class ExerciseSetQuestion
{
    public int IDExerciseSet { get; set; }
    public int IDExercise { get; set; }
    public int Order { get; set; }
    public decimal MaxScore { get; set; } = 1;
    public ExerciseSet? ExerciseSet { get; set; }
    public Exercise? Exercise { get; set; }
}
