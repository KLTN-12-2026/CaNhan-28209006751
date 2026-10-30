namespace Atomos.Domain.Chemistry;

public sealed class ReactionParticipant
{
    public int IDReaction { get; set; }
    public int IDChemical { get; set; }
    public string Role { get; set; } = string.Empty;
    public int Coefficient { get; set; }
    public Reaction? Reaction { get; set; }
    public Chemical? Chemical { get; set; }
}
