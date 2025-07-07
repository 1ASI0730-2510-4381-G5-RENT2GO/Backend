using System.ComponentModel.DataAnnotations;
using BackendRent2Go.Client.Domain.Model; // Para VehicleDto

namespace BackendRent2Go.Provider.Domain.Model;

public class ProviderReservationDto
{
    public string Id { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ProviderId { get; set; } = string.Empty;
    public string VehicleId { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty; // pending, confirmed, in_progress, completed, cancelled, rejected
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
    
    // Información del cliente para el provider
    public ClientInfoDto? Client { get; set; }
    
    // Información del vehículo
    public VehicleDto? Vehicle { get; set; }
    
    // Propiedades calculadas específicas para el provider
    public int TotalDays => (EndDate - StartDate).Days;
    public bool CanBeConfirmed => Status == "pending";
    public bool CanBeRejected => Status == "pending";
    public bool CanBeCompleted => Status == "confirmed" && EndDate <= DateTime.Now;
    public bool RequiresAction => Status == "pending";
    
    // Propiedades de ingresos para el provider
    public decimal ProviderEarnings => TotalAmount * 0.85m; // Asumiendo 15% de comisión
    public decimal PlatformFee => TotalAmount * 0.15m;
    
    // Alias para compatibilidad
    public DateTime PickupDate => StartDate;
    public DateTime ReturnDate => EndDate;
    public DateTime BookingDate => CreatedAt;
}

public class ClientInfoDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public DateTime RegistrationDate { get; set; }
    public int TotalReservations { get; set; }
    public double? Rating { get; set; }
}

public class UpdateReservationStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty; // confirmed, in_progress, completed
    
    public string? Notes { get; set; }
}

public class RejectReservationDto
{
    [Required]
    public string Reason { get; set; } = string.Empty;
    
    public string? Notes { get; set; }
}

public class ProviderReservationStatsDto
{
    public int TotalReservations { get; set; }
    public int PendingReservations { get; set; }
    public int ConfirmedReservations { get; set; }
    public int CompletedReservations { get; set; }
    public int CancelledReservations { get; set; }
    public decimal TotalEarnings { get; set; }
    public decimal MonthlyEarnings { get; set; }
    public double AverageRating { get; set; }
}
