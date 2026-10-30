using Atomos.Domain.Users;

namespace Atomos.Domain.Payments;

public sealed class PaymentTransaction
{
    public long IDPaymentTransaction { get; set; }
    public int IDPaymentOrder { get; set; }
    public string Provider { get; set; } = "ManualBankTransfer";
    public string ProviderTransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public string Status { get; set; } = "Confirmed";
    public DateTime TransactionTime { get; set; }
    public string? RawReference { get; set; }
    public string? MetadataJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public int? ConfirmedByAdminId { get; set; }
    public PaymentOrder? PaymentOrder { get; set; }
    public User? ConfirmedByAdmin { get; set; }
}
