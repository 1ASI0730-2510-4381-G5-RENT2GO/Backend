using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendRent2Go.Client.Domain.Model;
using BackendRent2Go.Client.Domain.Services;
using BackendRent2Go.Client.Interfaces.ACL;
using BackendRent2Go.Shared.Domain.Services;
using BackendRent2Go.Data;
using Microsoft.EntityFrameworkCore;
using BackendRent2Go.Client.Domain.Model.Entities;

namespace BackendRent2Go.Client.Application.Internal;

public class VehicleService : IVehicleService
{
    private readonly IProviderVehicleService _providerVehicleService;
    private readonly IProviderVehicleImageService _providerVehicleImageService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ApplicationDbContext _context;

    public VehicleService(
        IProviderVehicleService providerVehicleService, 
        IProviderVehicleImageService providerVehicleImageService,
        IDateTimeProvider dateTimeProvider,
        ApplicationDbContext context)
    {
        _providerVehicleService = providerVehicleService;
        _providerVehicleImageService = providerVehicleImageService;
        _dateTimeProvider = dateTimeProvider;
        _context = context;
    }

    public async Task<IEnumerable<VehicleDto>> SearchVehicles(VehicleSearchParamsDto searchParams)
    {
        // Obtener todos los vehículos activos y disponibles
        var vehicles = await _providerVehicleService.FindByStatusAsync("available");

        // Convertir a DTOs para clientes y aplicar filtros
        var vehicleDtos = vehicles.Select(v => new VehicleDto
        {
            Id = v.Id,
            Brand = v.Brand,
            Model = v.Model,
            Year = v.Year,
            Category = v.Type,
            Price = v.DailyRate,
            Seats = v.Seats,
            Luggage = v.LuggageCapacity ?? 0,
            Transmission = v.Transmission,
            Rating = v.Rating.HasValue ? (decimal)v.Rating.Value : 0m,
            ReviewCount = v.ReviewCount ?? 0,
            Status = v.Status,
            Images = new List<string>()
        }).ToList();

        // Cargar imágenes para cada vehículo
        foreach (var vehicleDto in vehicleDtos)
        {
            var images = await _providerVehicleImageService.FindByVehicleIdAsync(vehicleDto.Id);
            vehicleDto.Images = images.Select(img => img.ImageUrl).ToList();
        }

        // Aplicar filtros si se proporcionan
        IEnumerable<VehicleDto> filteredVehicles = vehicleDtos;

        // FILTRO CRÍTICO: Verificar disponibilidad por fechas
        if (searchParams.PickupDate.HasValue && searchParams.ReturnDate.HasValue)
        {
            var availableVehicleIds = await GetAvailableVehicleIds(searchParams.PickupDate.Value, searchParams.ReturnDate.Value);
            filteredVehicles = filteredVehicles.Where(v => availableVehicleIds.Contains(v.Id));
            
            Console.WriteLine($"VehicleService: Filtrados por disponibilidad de fecha. {filteredVehicles.Count()} vehículos disponibles");
        }

        // Filtro por texto de búsqueda (marca o modelo)
        if (!string.IsNullOrEmpty(searchParams.Search))
        {
            var search = searchParams.Search.ToLower();
            filteredVehicles = filteredVehicles.Where(v =>
                v.Brand.ToLower().Contains(search) ||
                v.Model.ToLower().Contains(search));
        }

        // Filtro por categoría
        if (!string.IsNullOrEmpty(searchParams.Category))
        {
            filteredVehicles = filteredVehicles.Where(v =>
                v.Category.ToLower() == searchParams.Category.ToLower());
        }

        // Filtro por marca
        if (!string.IsNullOrEmpty(searchParams.Brand))
        {
            filteredVehicles = filteredVehicles.Where(v =>
                v.Brand.ToLower() == searchParams.Brand.ToLower());
        }

        // Filtro por precio máximo
        if (searchParams.MaxPrice.HasValue)
        {
            filteredVehicles = filteredVehicles.Where(v => v.Price <= searchParams.MaxPrice.Value);
        }

        // Filtro por tipo de transmisión
        if (!string.IsNullOrEmpty(searchParams.Transmission))
        {
            filteredVehicles = filteredVehicles.Where(v =>
                v.Transmission.ToLower() == searchParams.Transmission.ToLower());
        }

        // Filtro por asientos mínimos
        if (searchParams.MinSeats.HasValue)
        {
            filteredVehicles = filteredVehicles.Where(v => v.Seats >= searchParams.MinSeats.Value);
        }

        // Filtro por capacidad mínima de equipaje
        if (searchParams.MinLuggage.HasValue)
        {
            filteredVehicles = filteredVehicles.Where(v => v.Luggage >= searchParams.MinLuggage.Value);
        }

        // Filtro por año mínimo
        if (searchParams.MinYear.HasValue)
        {
            filteredVehicles = filteredVehicles.Where(v => v.Year >= searchParams.MinYear.Value);
        }

        // Filtro por calificación mínima
        if (searchParams.MinRating.HasValue)
        {
            filteredVehicles = filteredVehicles.Where(v => v.Rating >= searchParams.MinRating.Value);
        }

        return filteredVehicles.ToList();
    }

    /// <summary>
    /// Obtiene los IDs de vehículos disponibles para las fechas especificadas
    /// </summary>
    private async Task<List<string>> GetAvailableVehicleIds(DateTime startDate, DateTime endDate)
    {
        try
        {
            // Obtener vehículos que tienen reservas que se superponen con las fechas solicitadas
            var unavailableVehicleIds = await _context.Set<Reservation>()
                .Where(r => (r.Status == "confirmed" || r.Status == "in_progress") &&
                           r.StartDate < endDate && r.EndDate > startDate)
                .Select(r => r.VehicleId)
                .Distinct()
                .ToListAsync();

            // Obtener todos los vehículos disponibles
            var allAvailableVehicles = await _providerVehicleService.FindByStatusAsync("available");
            var allVehicleIds = allAvailableVehicles.Select(v => v.Id).ToList();

            // Retornar solo los que NO están en la lista de no disponibles
            var availableVehicleIds = allVehicleIds.Except(unavailableVehicleIds).ToList();

            Console.WriteLine($"VehicleService: {allVehicleIds.Count} vehículos totales, {unavailableVehicleIds.Count} no disponibles, {availableVehicleIds.Count} disponibles para {startDate:yyyy-MM-dd} a {endDate:yyyy-MM-dd}");

            return availableVehicleIds;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al verificar disponibilidad de vehículos: {ex.Message}");
            // En caso de error, devolver lista vacía para ser conservadores
            return new List<string>();
        }
    }

    public async Task<VehicleDto?> GetVehicleDetails(string id, DateTime? pickupDate = null, DateTime? returnDate = null)
    {
        var vehicle = await _providerVehicleService.GetVehicleByIdAsync(id);
        if (vehicle == null)
        {
            return null;
        }

        var vehicleDto = new VehicleDto
        {
            Id = vehicle.Id, // Mantener el ID como string
            Brand = vehicle.Brand,
            Model = vehicle.Model,
            Year = vehicle.Year,
            Category = vehicle.Type,
            Price = vehicle.DailyRate,
            Seats = vehicle.Seats,
            Luggage = vehicle.LuggageCapacity ?? 0,
            Transmission = vehicle.Transmission,
            Rating = vehicle.Rating.HasValue ? (decimal)vehicle.Rating.Value : 0m, // Conversión explícita de double? a decimal
            ReviewCount = vehicle.ReviewCount ?? 0,
            Status = vehicle.Status,
            Images = new List<string>() // Inicializar lista vacía
        };

        // Cargar imágenes
        var images = await _providerVehicleImageService.FindByVehicleIdAsync(id);
        vehicleDto.Images = images.Select(img => img.ImageUrl).ToList();

        // Si se proporcionaron fechas, verificar disponibilidad
        if (pickupDate.HasValue && returnDate.HasValue)
        {
            bool isAvailable = await CheckVehicleAvailability(id, pickupDate.Value, returnDate.Value);
            if (!isAvailable)
            {
                vehicleDto.Status = "reserved";
            }
        }

        return vehicleDto;
    }

    public async Task<bool> CheckVehicleAvailability(string id, DateTime pickupDate, DateTime returnDate)
    {
        var vehicle = await _providerVehicleService.GetVehicleByIdAsync(id);
        if (vehicle == null)
        {
            return false;
        }

        // Aquí deberías verificar si el vehículo está reservado en las fechas proporcionadas
        // Por ahora, simplemente verificamos que el vehículo esté disponible y que las fechas sean válidas

        var now = _dateTimeProvider.Now;

        // Verificar que las fechas sean válidas
        if (pickupDate < now || returnDate <= pickupDate)
        {
            return false;
        }

        // En una implementación real, verificarías las reservas existentes
        // Puedes consultar la tabla de reservas para este vehículo y verificar
        // que no haya superposición de fechas

        return true;
    }
}
