using BackendRent2Go.Client.Domain.Model;
using BackendRent2Go.Client.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BackendRent2Go.Client.Interfaces.REST;

[ApiController]
[Route("api/client/vehicles")]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleService _vehicleService;

    public VehiclesController(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    /// <summary>
    /// Busca vehículos disponibles según los criterios proporcionados
    /// </summary>
    /// <param name="searchParams">Parámetros de búsqueda</param>
    /// <returns>Lista de vehículos que cumplen con los criterios</returns>
    [HttpGet("search")]
    public async Task<IActionResult> SearchVehicles(
        [FromQuery] string? search = null,
        [FromQuery] string? category = null,
        [FromQuery] string? brand = null,
        [FromQuery] string? maxPrice = null,
        [FromQuery] string? transmission = null,
        [FromQuery] string? pickupDate = null,
        [FromQuery] string? returnDate = null,
        [FromQuery] string? minSeats = null,
        [FromQuery] string? minLuggage = null,
        [FromQuery] string? minYear = null,
        [FromQuery] string? minRating = null)
    {
        try
        {
            Console.WriteLine("VehiclesController: Recibiendo petición de búsqueda de vehículos");
            Console.WriteLine($"Parámetros: search={search}, category={category}, brand={brand}, maxPrice={maxPrice}, transmission={transmission}");
            Console.WriteLine($"Parámetros adicionales: minSeats={minSeats}, minLuggage={minLuggage}, minYear={minYear}, minRating={minRating}");
            
            // Comprobar si todos los parámetros están vacíos (carga inicial)
            bool isInitialLoad = string.IsNullOrEmpty(search) && 
                                string.IsNullOrEmpty(category) && 
                                string.IsNullOrEmpty(brand) && 
                                string.IsNullOrEmpty(maxPrice) && 
                                string.IsNullOrEmpty(transmission) && 
                                string.IsNullOrEmpty(minSeats) && 
                                string.IsNullOrEmpty(minLuggage) && 
                                string.IsNullOrEmpty(minYear) && 
                                string.IsNullOrEmpty(minRating);
            
            if (isInitialLoad) {
                Console.WriteLine("VehiclesController: Carga inicial detectada - devolviendo todos los vehículos disponibles");
                var searchParams = new VehicleSearchParamsDto(); // Objeto vacío para la carga inicial
                var vehicles = await _vehicleService.SearchVehicles(searchParams);
                return Ok(vehicles);
            }
            
            // Para búsquedas con filtros, crear objeto DTO y manejar las conversiones de manera segura
            var filteredParams = new VehicleSearchParamsDto
            {
                Search = search,
                Category = category,
                Brand = brand,
                Transmission = transmission,
                MaxPrice = !string.IsNullOrWhiteSpace(maxPrice) && decimal.TryParse(maxPrice, out var price) ? price : null,
                MinSeats = !string.IsNullOrWhiteSpace(minSeats) && int.TryParse(minSeats, out var seats) ? seats : null,
                MinLuggage = !string.IsNullOrWhiteSpace(minLuggage) && int.TryParse(minLuggage, out var luggage) ? luggage : null,
                MinYear = !string.IsNullOrWhiteSpace(minYear) && int.TryParse(minYear, out var year) ? year : null,
                MinRating = !string.IsNullOrWhiteSpace(minRating) && decimal.TryParse(minRating, out var rating) ? rating : null
            };

            // Procesar fechas solo si ambas están presentes
            if (!string.IsNullOrWhiteSpace(pickupDate) && !string.IsNullOrWhiteSpace(returnDate))
            {
                if (DateTime.TryParse(pickupDate, out var pickup))
                    filteredParams.PickupDate = pickup;
                if (DateTime.TryParse(returnDate, out var returnDt))
                    filteredParams.ReturnDate = returnDt;
            }

            Console.WriteLine($"VehiclesController: Buscando vehículos con filtros: Search={filteredParams.Search}, Category={filteredParams.Category}");
            var filteredVehicles = await _vehicleService.SearchVehicles(filteredParams);
            return Ok(filteredVehicles);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"VehiclesController: Error interno al buscar vehículos: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            return StatusCode(500, new { message = $"Error interno al buscar vehículos: {ex.Message}" });
        }
    }

    /// <summary>
    /// Obtiene los detalles completos de un vehículo específico
    /// </summary>
    /// <param name="id">ID del vehículo</param>
    /// <param name="pickupDate">Fecha de recogida opcional para verificar disponibilidad</param>
    /// <param name="returnDate">Fecha de devolución opcional para verificar disponibilidad</param>
    /// <returns>Detalles del vehículo</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetVehicleDetails(string id, [FromQuery] DateTime? pickupDate = null, [FromQuery] DateTime? returnDate = null)
    {
        try
        {
            var vehicle = await _vehicleService.GetVehicleDetails(id, pickupDate, returnDate);

            if (vehicle == null)
                return NotFound("Vehículo no encontrado o no disponible");

            return Ok(vehicle);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Error interno al obtener detalles del vehículo: {ex.Message}" });
        }
    }

    /// <summary>
    /// Verifica la disponibilidad de un vehículo en un rango de fechas
    /// </summary>
    /// <param name="id">ID del vehículo</param>
    /// <param name="pickupDate">Fecha de recogida</param>
    /// <param name="returnDate">Fecha de devolución</param>
    /// <returns>Información de disponibilidad</returns>
    [HttpGet("{id}/availability")]
    public async Task<IActionResult> CheckVehicleAvailability(string id, [FromQuery] DateTime pickupDate, [FromQuery] DateTime returnDate)
    {
        try
        {
            var isAvailable = await _vehicleService.CheckVehicleAvailability(id, pickupDate, returnDate);
            return Ok(new { isAvailable });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Error interno al verificar disponibilidad: {ex.Message}" });
        }
    }
}

