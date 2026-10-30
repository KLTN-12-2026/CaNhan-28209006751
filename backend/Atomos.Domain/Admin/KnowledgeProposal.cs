namespace Atomos.Domain.Admin;

public sealed class KnowledgeProposal
{
    public int IDProposal { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CandidateKey { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public string? RequestedSourcesJson { get; set; }
    public string? PayloadJson { get; set; }
    public int SchemaVersion { get; set; } = 1;
    public string? ValidationJson { get; set; }
    public int Revision { get; set; }
    public string Origin { get; set; } = "Admin";
    public int? RequestedBy { get; set; }
    public int? ReviewedBy { get; set; }
    public string? DecisionReason { get; set; }
    public int? TargetId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DecidedAt { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ICollection<KnowledgeProposalSource> Sources { get; set; } = [];
    public ICollection<KnowledgeProposalRevision> Revisions { get; set; } = [];
    public ICollection<KnowledgeProposalJob> Jobs { get; set; } = [];
}

public sealed class KnowledgeProposalSource
{
    public long IDSource { get; set; }
    public int IDProposal { get; set; }
    public string Organization { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Identifier { get; set; }
    public DateTime AccessedAt { get; set; }
    public string? Version { get; set; }
    public string ContentHash { get; set; } = string.Empty;
    public string? Snippet { get; set; }
    public string? FieldsJson { get; set; }
}

public sealed class KnowledgeProposalRevision
{
    public long IDRevision { get; set; }
    public int IDProposal { get; set; }
    public int Revision { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public int? EditedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class KnowledgeProposalJob
{
    public long IDJob { get; set; }
    public int IDProposal { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Attempts { get; set; }
    public DateTime? LeaseUntil { get; set; }
    public DateTime? NotBefore { get; set; }
    public string? Error { get; set; }
    public string? Provider { get; set; }
    public string? Model { get; set; }
    public string? PromptVersion { get; set; }
    public int? DurationMs { get; set; }
    public int? InputTokens { get; set; }
    public int? OutputTokens { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
}
