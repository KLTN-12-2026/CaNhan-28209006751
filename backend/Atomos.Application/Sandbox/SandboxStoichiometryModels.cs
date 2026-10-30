namespace Atomos.Application.Sandbox;

public sealed class SandboxStoichiometryRequest
{
    public List<SandboxSubstanceInput> Substances { get; set; } = [];
    public bool IsHeated { get; set; }
    public int? MaxSteps { get; set; }
    public bool ConfirmExecution { get; set; }
}

public sealed class SandboxSubstanceInput
{
    public string Formula { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Unit { get; set; } = string.Empty;
}
