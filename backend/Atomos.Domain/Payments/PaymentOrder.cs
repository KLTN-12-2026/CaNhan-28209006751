using Atomos.Domain.Users;

namespace Atomos.Domain.Payments;

public sealed class PaymentOrder
{
    public int IDPaymentOrder { get; set; }
    public int IDUser { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public string Status { get; set; } = "Pending";
    public string Provider { get; set; } = "ManualBankTransfer";
    public string Description { get; set; } = string.Empty;
    public string? ProviderOrderCode { get; set; }
    public string? ProviderPaymentId { get; set; }
    public string? ProviderTransactionId { get; set; }
    public string? CheckoutUrl { get; set; }
    public string? QrPayload { get; set; }
    public string? PaymentContent { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public string? MetadataJson { get; set; }
    public User? User { get; set; }
    public ICollection<PaymentTransaction> Transactions { get; set; } = [];
}
