using BackendRent2Go.Client.Domain.Model;
using BackendRent2Go.Client.Domain.Model.Entities;

namespace BackendRent2Go.Client.Domain.Services;

public interface IReservationManagementService
{
    /// <summary>
    /// Aprobar una reserva pendiente (cambia estado a confirmed y vehículo a reserved)
    /// </summary>
    Task<ReservationDto> ApproveReservationAsync(string reservationId, string providerId);
    
    /// <summary>
    /// Rechazar una reserva pendiente (cambia estado a cancelled y libera vehículo)
    /// </summary>
    Task<ReservationDto> RejectReservationAsync(string reservationId, string providerId, string reason);
    
    /// <summary>
    /// Iniciar una reserva confirmada (cambia estado a in_progress y vehículo a in_use)
    /// </summary>
    Task<ReservationDto> StartReservationAsync(string reservationId, string providerId);
    
    /// <summary>
    /// Completar inspección post-uso y habilitar vehículo nuevamente
    /// </summary>
    Task<bool> CompleteInspectionAsync(string vehicleId, string providerId, bool approved);
    
    /// <summary>
    /// Obtener reservas pendientes de un provider
    /// </summary>
    Task<IEnumerable<ReservationDto>> GetPendingReservationsAsync(string providerId);
    
    /// <summary>
    /// Obtener vehículos en inspección de un provider
    /// </summary>
    Task<IEnumerable<string>> GetVehiclesInInspectionAsync(string providerId);
}
