namespace BackendRent2Go.Client.Domain.Model.Entities;

public class Payment
{
    public string Id { get; set; } = string.Empty;
    public string ReservationId { get; set; } = string.Empty;
    public string? PaymentMethodId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "pending"; // 'pending', 'completed', 'failed', 'refunded'
    public string? TransactionId { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public Reservation? Reservation { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
}
