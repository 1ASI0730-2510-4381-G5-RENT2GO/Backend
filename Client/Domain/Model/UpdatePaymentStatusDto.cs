using System.ComponentModel.DataAnnotations;

namespace BackendRent2Go.Client.Domain.Model;

public class UpdatePaymentStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty; // 'pending', 'completed', 'failed', 'refunded'
    
    public string? Notes { get; set; }
}
