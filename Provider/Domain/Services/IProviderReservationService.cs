using BackendRent2Go.Provider.Domain.Model;

namespace BackendRent2Go.Provider.Domain.Services;

public interface IProviderReservationService
{
    /// <summary>
    /// Obtiene todas las reservas del provider actual
    /// </summary>
    /// <param name="providerId">ID del provider</param>
    /// <returns>Lista de reservas del provider</returns>
    Task<IEnumerable<ProviderReservationDto>> GetMyReservations(string providerId);
    
    /// <summary>
    /// Obtiene una reserva específica por ID
    /// </summary>
    /// <param name="id">ID de la reserva</param>
    /// <param name="providerId">ID del provider para verificar permisos</param>
    /// <returns>Detalles de la reserva</returns>
    Task<ProviderReservationDto?> GetReservationById(string id, string providerId);
    
    /// <summary>
    /// Actualiza el estado de una reserva (confirmar, completar, etc.)
    /// </summary>
    /// <param name="id">ID de la reserva</param>
    /// <param name="providerId">ID del provider</param>
    /// <param name="updateStatusDto">Nuevo estado de la reserva</param>
    /// <returns>Reserva actualizada</returns>
    Task<ProviderReservationDto> UpdateReservationStatus(string id, string providerId, UpdateReservationStatusDto updateStatusDto);
    
    /// <summary>
    /// Rechaza una reserva
    /// </summary>
    /// <param name="id">ID de la reserva</param>
    /// <param name="providerId">ID del provider</param>
    /// <param name="rejectReservationDto">Motivo de rechazo</param>
    /// <returns>Reserva rechazada</returns>
    Task<ProviderReservationDto> RejectReservation(string id, string providerId, RejectReservationDto rejectReservationDto);
    
    /// <summary>
    /// Obtiene reservas filtradas por estado
    /// </summary>
    /// <param name="providerId">ID del provider</param>
    /// <param name="status">Estado de las reservas</param>
    /// <returns>Lista de reservas filtradas</returns>
    Task<IEnumerable<ProviderReservationDto>> GetReservationsByStatus(string providerId, string status);
    
    /// <summary>
    /// Obtiene reservas por vehículo específico
    /// </summary>
    /// <param name="vehicleId">ID del vehículo</param>
    /// <param name="providerId">ID del provider para verificar permisos</param>
    /// <returns>Lista de reservas del vehículo</returns>
    Task<IEnumerable<ProviderReservationDto>> GetReservationsByVehicle(string vehicleId, string providerId);
    
    /// <summary>
    /// Obtiene reservas en un rango de fechas
    /// </summary>
    /// <param name="providerId">ID del provider</param>
    /// <param name="startDate">Fecha de inicio</param>
    /// <param name="endDate">Fecha de fin</param>
    /// <returns>Lista de reservas en el rango</returns>
    Task<IEnumerable<ProviderReservationDto>> GetReservationsByDateRange(string providerId, DateTime startDate, DateTime endDate);
}
