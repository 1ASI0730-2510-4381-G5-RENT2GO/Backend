namespace BackendRent2Go.Client.Domain.Model;

public class PaymentMethodDto
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public string? CardHolder { get; set; }
    public string? CardNumberLast4 { get; set; }
    public string? CardExpiry { get; set; }
    public string? CardType { get; set; }
    public string? PaypalEmail { get; set; }
    public string? BankName { get; set; }
    public string? AccountNumberLast4 { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreatePaymentMethodDto
{
    public string Type { get; set; } = string.Empty;
    public bool SetAsDefault { get; set; } = false;
    
    // Credit Card fields
    public string? CardNumber { get; set; }
    public string? CardHolder { get; set; }
    public string? CardExpiry { get; set; }
    public string? CardCvv { get; set; }
    
    // PayPal fields
    public string? PaypalEmail { get; set; }
    
    // Bank Account fields
    public string? BankName { get; set; }
    public string? AccountNumber { get; set; }
}

public class UpdatePaymentMethodDto
{
    public bool? SetAsDefault { get; set; }
    public string? CardExpiry { get; set; }
}
