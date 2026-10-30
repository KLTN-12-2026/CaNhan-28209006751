using Atomos.Domain.Users;

namespace Atomos.Domain.Learning;

public sealed class LessonProgress
{
    public int IDUser { get; set; }
    public int IDLesson { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? LastStudiedAt { get; set; }
    public byte CompletionPercent { get; set; }
    public User? User { get; set; }
    public Lesson? Lesson { get; set; }
}
