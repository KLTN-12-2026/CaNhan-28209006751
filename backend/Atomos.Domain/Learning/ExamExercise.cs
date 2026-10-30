namespace Atomos.Domain.Learning;

public sealed class ExamExercise
{
    public int IDExam { get; set; }
    public int IDExercise { get; set; }
    public int Order { get; set; }
    public decimal MaxScore { get; set; } = 1;
    public Exam? Exam { get; set; }
    public Exercise? Exercise { get; set; }
}
