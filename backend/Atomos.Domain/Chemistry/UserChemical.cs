using Atomos.Domain.Users;

namespace Atomos.Domain.Chemistry;

public sealed class UserChemical
{
    public int IDUser { get; set; }
    public int IDChemical { get; set; }
    public DateTime UnlockedAt { get; set; }
    public User? User { get; set; }
    public Chemical? Chemical { get; set; }
}
