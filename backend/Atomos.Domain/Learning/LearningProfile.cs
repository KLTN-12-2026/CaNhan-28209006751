using Atomos.Domain.Users;

namespace Atomos.Domain.Learning;

public sealed class LearningProfile
{
    public int IDUser { get; set; }
    public int CurrentStreak { get; set; }
    public int BestStreak { get; set; }
    public DateOnly? LastDailyReviewDate { get; set; }
    public User? User { get; set; }
}
