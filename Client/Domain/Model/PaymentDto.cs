namespace BackendRent2Go.Client.Domain.Model;

public class PaymentDto
{
    public string Id { get; set; } = string.Empty;
    public string ReservationId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Payment method info (for history display)
    public string? PaymentMethodType { get; set; }
    public string? CardNumberLast4 { get; set; }
    public string? CardType { get; set; }
    
    // Reservation info (for history display)
    public string? VehicleBrand { get; set; }
    public string? VehicleModel { get; set; }
    public DateTime? ReservationStartDate { get; set; }
    public DateTime? ReservationEndDate { get; set; }
}

public class CreatePaymentDto
{
    public string ReservationId { get; set; } = string.Empty;
    public string? PaymentMethodId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "PEN";
    public string PaymentMethod { get; set; } = string.Empty; // 'credit_card', 'cash', 'paypal', 'bank_transfer'
    public string? Notes { get; set; }
    
    // For new credit card payments
    public string? CardNumber { get; set; }
    public string? CardHolder { get; set; }
    public string? CardExpiry { get; set; }
    public string? CardCvv { get; set; }
    public bool SaveCard { get; set; } = false;
    
    // For PayPal payments
    public string? PaypalEmail { get; set; }
    
    // For bank transfers
    public string? BankAccount { get; set; }
    public string? BankName { get; set; }
}
