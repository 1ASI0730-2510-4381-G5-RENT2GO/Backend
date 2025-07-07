using BackendRent2Go.Client.Domain.Model;

namespace BackendRent2Go.Client.Domain.Services;

public interface IVehicleService
{
    /// <summary>
    /// Busca vehículos disponibles según los criterios proporcionados
    /// </summary>
    /// <param name="searchParams">Parámetros de búsqueda</param>
    /// <returns>Lista de vehículos que cumplen con los criterios</returns>
    Task<IEnumerable<VehicleDto>> SearchVehicles(VehicleSearchParamsDto searchParams);
    
    /// <summary>
    /// Obtiene los detalles completos de un vehículo específico
    /// </summary>
    /// <param name="id">ID del vehículo</param>
    /// <param name="pickupDate">Fecha de recogida opcional para verificar disponibilidad</param>
    /// <param name="returnDate">Fecha de devolución opcional para verificar disponibilidad</param>
    /// <returns>Detalles del vehículo</returns>
    Task<VehicleDto?> GetVehicleDetails(string id, DateTime? pickupDate = null, DateTime? returnDate = null);
    
    /// <summary>
    /// Verifica la disponibilidad de un vehículo en un rango de fechas
    /// </summary>
    /// <param name="id">ID del vehículo</param>
    /// <param name="pickupDate">Fecha de recogida</param>
    /// <param name="returnDate">Fecha de devolución</param>
    /// <returns>Información de disponibilidad</returns>
    Task<bool> CheckVehicleAvailability(string id, DateTime pickupDate, DateTime returnDate);
}
