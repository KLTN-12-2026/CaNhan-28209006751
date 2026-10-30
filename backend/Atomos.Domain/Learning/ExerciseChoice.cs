namespace Atomos.Domain.Learning;

public sealed class ExerciseChoice
{
    public int IDChoice { get; set; }
    public int IDExercise { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsAnswer { get; set; }
    public int Order { get; set; }
    public Exercise? Exercise { get; set; }
}
