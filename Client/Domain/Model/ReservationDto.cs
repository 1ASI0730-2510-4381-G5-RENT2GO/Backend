using System.ComponentModel.DataAnnotations;

namespace BackendRent2Go.Client.Domain.Model;

public class ReservationDto
{
    public string Id { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ProviderId { get; set; } = string.Empty;
    public string VehicleId { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty; // pending, confirmed, in_progress, completed, cancelled
    public string PaymentStatus { get; set; } = string.Empty; // pending, paid, refunded
    public string? PaymentMethod { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal VehiclePrice { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? CancellationDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Información del vehículo
    public VehicleDto? Vehicle { get; set; }
    
    // Propiedades calculadas para el frontend
    public int TotalDays => (EndDate - StartDate).Days;
    public bool CanBeCancelled => Status is "pending" or "confirmed" && StartDate > DateTime.Now.AddHours(24);
    public bool CanBeModified => Status is "pending" or "confirmed" && StartDate > DateTime.Now.AddHours(48);
    public bool CanBeReviewed => Status == "completed" && !Reviewed;
    public bool Reviewed { get; set; } = false;
    
    // Propiedades adicionales para compatibilidad con el frontend
    public decimal TotalPrice => TotalAmount; // Alias para el frontend
    public DateTime PickupDate => StartDate; // Alias para el frontend
    public DateTime ReturnDate => EndDate; // Alias para el frontend
    public DateTime BookingDate => CreatedAt; // Alias para el frontend
}

public class CreateReservationDto
{
    [Required]
    public string VehicleId { get; set; } = string.Empty;
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    public string? PaymentMethod { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
}

public class UpdateReservationDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
}

public class CancelReservationDto
{
    [Required]
    public string Reason { get; set; } = string.Empty;
}
