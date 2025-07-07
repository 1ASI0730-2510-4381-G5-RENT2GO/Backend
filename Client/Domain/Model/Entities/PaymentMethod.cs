namespace BackendRent2Go.Client.Domain.Model.Entities;

public class PaymentMethod
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // 'credit_card', 'paypal', 'bank_account'
    public bool IsDefault { get; set; } = false;
    
    // Credit Card fields
    public string? CardHolder { get; set; }
    public string? CardNumberLast4 { get; set; }
    public string? CardExpiry { get; set; }
    public string? CardType { get; set; } // 'visa', 'mastercard', 'amex', etc.
    
    // PayPal fields
    public string? PaypalEmail { get; set; }
    
    // Bank Account fields
    public string? BankName { get; set; }
    public string? AccountNumberLast4 { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
