using System.ComponentModel.DataAnnotations;

namespace BackendRent2Go.Client.Domain.Model;

public class ProcessPaymentDto
{
    [Required]
    public string ReservationId { get; set; } = string.Empty;
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    public decimal Amount { get; set; }
    
    public string Currency { get; set; } = "PEN";
    
    [Required]
    public string PaymentMethod { get; set; } = string.Empty; // 'credit_card', 'cash', 'paypal'
    
    public string? Notes { get; set; }
    
    // Para pagos con tarjeta de crédito
    public string? CardNumber { get; set; }
    public string? CardHolder { get; set; }
    public string? CardExpiry { get; set; }
    public string? CardCvv { get; set; }
    public bool SaveCard { get; set; } = false;
    
    // Para pagos con PayPal
    public string? PaypalEmail { get; set; }
    
    // Para transferencias bancarias
    public string? BankAccount { get; set; }
    public string? BankName { get; set; }
}
