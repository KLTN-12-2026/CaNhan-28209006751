namespace Atomos.Domain.Payments;

public sealed class PaymentConfiguration
{
    public int IDPaymentConfiguration { get; set; }
    public string Provider { get; set; } = "ManualBankTransfer";
    public decimal VipPrice { get; set; }
    public string Currency { get; set; } = "VND";
    public string? ReceiverName { get; set; }
    public string? ReceiverAccount { get; set; }
    public string? ReceiverBank { get; set; }
    public string? BankCode { get; set; }
    public string? ProviderMerchantId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
