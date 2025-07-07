using BackendRent2Go.Client.Domain.Model;

namespace BackendRent2Go.Client.Domain.Services;

public interface IReservationService
{
    /// <summary>
    /// Obtiene todas las reservas del cliente actual
    /// </summary>
    /// <param name="clientId">ID del cliente</param>
    /// <returns>Lista de reservas del cliente</returns>
    Task<IEnumerable<ReservationDto>> GetMyReservations(string clientId);
    
    /// <summary>
    /// Obtiene una reserva específica por ID
    /// </summary>
    /// <param name="id">ID de la reserva</param>
    /// <param name="clientId">ID del cliente para verificar permisos</param>
    /// <returns>Detalles de la reserva</returns>
    Task<ReservationDto?> GetReservationById(string id, string clientId);
    
    /// <summary>
    /// Crea una nueva reserva
    /// </summary>
    /// <param name="clientId">ID del cliente</param>
    /// <param name="createReservationDto">Datos de la reserva</param>
    /// <returns>Reserva creada</returns>
    Task<ReservationDto> CreateReservation(string clientId, CreateReservationDto createReservationDto);
    
    /// <summary>
    /// Actualiza una reserva existente
    /// </summary>
    /// <param name="id">ID de la reserva</param>
    /// <param name="clientId">ID del cliente</param>
    /// <param name="updateReservationDto">Datos a actualizar</param>
    /// <returns>Reserva actualizada</returns>
    Task<ReservationDto> UpdateReservation(string id, string clientId, UpdateReservationDto updateReservationDto);
    
    /// <summary>
    /// Cancela una reserva
    /// </summary>
    /// <param name="id">ID de la reserva</param>
    /// <param name="clientId">ID del cliente</param>
    /// <param name="cancelReservationDto">Motivo de cancelación</param>
    /// <returns>Reserva cancelada</returns>
    Task<ReservationDto> CancelReservation(string id, string clientId, CancelReservationDto cancelReservationDto);
    
    /// <summary>
    /// Obtiene reservas filtradas por estado
    /// </summary>
    /// <param name="clientId">ID del cliente</param>
    /// <param name="status">Estado de las reservas</param>
    /// <returns>Lista de reservas filtradas</returns>
    Task<IEnumerable<ReservationDto>> GetReservationsByStatus(string clientId, string status);
}
