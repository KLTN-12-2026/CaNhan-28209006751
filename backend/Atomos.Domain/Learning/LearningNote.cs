using Atomos.Domain.Users;

namespace Atomos.Domain.Learning;

public sealed class LearningNote
{
    public int IDNote { get; set; }
    public int IDUser { get; set; }
    public int? IDLesson { get; set; }
    public string? Title { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public User? User { get; set; }
}
