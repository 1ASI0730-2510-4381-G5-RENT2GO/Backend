using BackendRent2Go.Provider.Domain.Model;
using BackendRent2Go.Provider.Domain.Services;
using BackendRent2Go.Client.Domain.Model.Entities;
using BackendRent2Go.Data;
using Microsoft.EntityFrameworkCore;
using Rent2Go.API.Provider.Domain.Model;
using ClientVehicleDto = BackendRent2Go.Client.Domain.Model.VehicleDto; // Alias para evitar ambigüedad

namespace BackendRent2Go.Provider.Application.Internal.CommandServices;

public class ProviderReservationService : IProviderReservationService
{
    private readonly ApplicationDbContext _context;

    public ProviderReservationService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Helper para obtener el providerId desde el userId
    private async Task<string> GetProviderIdFromUserAsync(string userId)
    {
        var provider = await _context.Providers.FirstOrDefaultAsync(p => p.UserId == userId);
        if (provider == null)
            throw new InvalidOperationException("Provider no encontrado para el usuario");
        return provider.Id;
    }

    public async Task<IEnumerable<ProviderReservationDto>> GetMyReservations(string userId)
    {
        var providerId = await GetProviderIdFromUserAsync(userId);

        try
        {
            Console.WriteLine($"ProviderReservationService: Obteniendo reservas para provider {providerId}");

            var reservations = await _context.Set<Reservation>()
                .Where(r => r.ProviderId == providerId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            Console.WriteLine($"ProviderReservationService: Encontradas {reservations.Count} reservas");

            var reservationDtos = new List<ProviderReservationDto>();

            foreach (var reservation in reservations)
            {
                var reservationDto = await MapToProviderDto(reservation);
                reservationDtos.Add(reservationDto);
            }

            return reservationDtos;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener reservas del provider: {ex.Message}");
            throw;
        }
    }

    public async Task<ProviderReservationDto?> GetReservationById(string id, string userId)
    {
        var providerId = await GetProviderIdFromUserAsync(userId);

        try
        {
            var reservation = await _context.Set<Reservation>()
                .FirstOrDefaultAsync(r => r.Id == id && r.ProviderId == providerId);

            if (reservation == null)
                return null;

            return await MapToProviderDto(reservation);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener reserva {id}: {ex.Message}");
            throw;
        }
    }

    public async Task<ProviderReservationDto> UpdateReservationStatus(string id, string userId, UpdateReservationStatusDto updateStatusDto)
    {
        var providerId = await GetProviderIdFromUserAsync(userId);

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var reservation = await _context.Set<Reservation>()
                .FirstOrDefaultAsync(r => r.Id == id && r.ProviderId == providerId);

            if (reservation == null)
                throw new InvalidOperationException("Reserva no encontrada");

            // Validar transiciones de estado válidas
            ValidateStatusTransition(reservation.Status, updateStatusDto.Status);

            var oldStatus = reservation.Status;
            reservation.Status = updateStatusDto.Status;
            reservation.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(updateStatusDto.Notes))
            {
                reservation.Notes = updateStatusDto.Notes;
            }

            // Si se confirma la reserva, actualizar el estado del pago
            if (updateStatusDto.Status == "confirmed" && reservation.PaymentStatus == "pending")
            {
                reservation.PaymentStatus = "paid";
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            Console.WriteLine($"Reserva {id} actualizada de {oldStatus} a {updateStatusDto.Status}");

            return await MapToProviderDto(reservation);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"Error al actualizar estado de reserva {id}: {ex.Message}");
            throw;
        }
    }

    public async Task<ProviderReservationDto> RejectReservation(string id, string userId, RejectReservationDto rejectReservationDto)
    {
        var providerId = await GetProviderIdFromUserAsync(userId);

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var reservation = await _context.Set<Reservation>()
                .FirstOrDefaultAsync(r => r.Id == id && r.ProviderId == providerId);

            if (reservation == null)
                throw new InvalidOperationException("Reserva no encontrada");

            if (reservation.Status != "pending")
                throw new InvalidOperationException("Solo se pueden rechazar reservas pendientes");

            reservation.Status = "cancelled";
            reservation.CancellationReason = rejectReservationDto.Reason;
            reservation.CancellationDate = DateTime.UtcNow;
            reservation.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(rejectReservationDto.Notes))
            {
                reservation.Notes = rejectReservationDto.Notes;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            Console.WriteLine($"Reserva {id} rechazada por el provider");

            return await MapToProviderDto(reservation);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"Error al rechazar reserva {id}: {ex.Message}");
            throw;
        }
    }

    public async Task<IEnumerable<ProviderReservationDto>> GetReservationsByStatus(string userId, string status)
    {
        var providerId = await GetProviderIdFromUserAsync(userId);

        try
        {
            var reservations = await _context.Set<Reservation>()
                .Where(r => r.ProviderId == providerId && r.Status == status)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var reservationDtos = new List<ProviderReservationDto>();

            foreach (var reservation in reservations)
            {
                var reservationDto = await MapToProviderDto(reservation);
                reservationDtos.Add(reservationDto);
            }

            return reservationDtos;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener reservas por estado {status}: {ex.Message}");
            throw;
        }
    }

    public async Task<IEnumerable<ProviderReservationDto>> GetReservationsByVehicle(string vehicleId, string userId)
    {
        var providerId = await GetProviderIdFromUserAsync(userId);

        try
        {
            // Verificar que el vehículo pertenece al provider
            var vehicle = await _context.Set<Vehicle>()
                .FirstOrDefaultAsync(v => v.Id == vehicleId && v.OwnerId == providerId);

            if (vehicle == null)
                throw new InvalidOperationException("Vehículo no encontrado o no pertenece al provider");

            var reservations = await _context.Set<Reservation>()
                .Where(r => r.VehicleId == vehicleId && r.ProviderId == providerId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var reservationDtos = new List<ProviderReservationDto>();

            foreach (var reservation in reservations)
            {
                var reservationDto = await MapToProviderDto(reservation);
                reservationDtos.Add(reservationDto);
            }

            return reservationDtos;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener reservas del vehículo {vehicleId}: {ex.Message}");
            throw;
        }
    }

    public async Task<IEnumerable<ProviderReservationDto>> GetReservationsByDateRange(string userId, DateTime startDate, DateTime endDate)
    {
        var providerId = await GetProviderIdFromUserAsync(userId);

        try
        {
            var reservations = await _context.Set<Reservation>()
                .Where(r => r.ProviderId == providerId && 
                           r.StartDate <= endDate && r.EndDate >= startDate)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var reservationDtos = new List<ProviderReservationDto>();

            foreach (var reservation in reservations)
            {
                var reservationDto = await MapToProviderDto(reservation);
                reservationDtos.Add(reservationDto);
            }

            return reservationDtos;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener reservas por rango de fechas: {ex.Message}");
            throw;
        }
    }

    private void ValidateStatusTransition(string currentStatus, string newStatus)
    {
        var validTransitions = new Dictionary<string, string[]>
        {
            { "pending", new[] { "confirmed", "cancelled" } },
            { "confirmed", new[] { "in_progress", "cancelled" } },
            { "in_progress", new[] { "completed", "cancelled" } },
            { "completed", new string[0] }, // No se puede cambiar desde completed
            { "cancelled", new string[0] }  // No se puede cambiar desde cancelled
        };

        if (!validTransitions.ContainsKey(currentStatus) || 
            !validTransitions[currentStatus].Contains(newStatus))
        {
            throw new InvalidOperationException(
                $"Transición de estado no válida: {currentStatus} -> {newStatus}");
        }
    }

    private async Task<ProviderReservationDto> MapToProviderDto(Reservation reservation)
    {
        var dto = new ProviderReservationDto
        {
            Id = reservation.Id,
            ClientId = reservation.ClientId,
            ProviderId = reservation.ProviderId,
            VehicleId = reservation.VehicleId,
            StartDate = reservation.StartDate,
            EndDate = reservation.EndDate,
            Status = reservation.Status,
            PaymentStatus = reservation.PaymentStatus,
            PaymentMethod = reservation.PaymentMethod,
            TotalAmount = reservation.TotalAmount,
            VehiclePrice = reservation.VehiclePrice,
            Location = reservation.Location,
            Notes = reservation.Notes,
            CancellationReason = reservation.CancellationReason,
            CancellationDate = reservation.CancellationDate,
            CreatedAt = reservation.CreatedAt,
            UpdatedAt = reservation.UpdatedAt
        };

        // Obtener información del cliente
        try
        {
            Console.WriteLine($"Buscando cliente con ID: {reservation.ClientId}");
            Console.WriteLine("=== DEBUGGING DETALLADO ===");
            
            bool clientMapped = false;
            
            // OPCIÓN 1: Verificar si existe un cliente con ese clientId (ESTRUCTURA CORRECTA)
            var clientById = await _context.Set<Rent2Go.API.IAM.Domain.Model.Client>()
                .FirstOrDefaultAsync(c => c.Id == reservation.ClientId);
                
            Console.WriteLine($"¿El clientId es un client.id? {clientById != null}");
            if (clientById != null)
            {
                Console.WriteLine($"Cliente encontrado con ID: {clientById.Id}, UserId: {clientById.UserId}");
                
                // Buscar el usuario asociado a este cliente
                var userFromClient = await _context.Set<Rent2Go.API.IAM.Domain.Model.Aggregates.User>()
                    .FirstOrDefaultAsync(u => u.Id == clientById.UserId);
                    
                if (userFromClient != null)
                {
                    Console.WriteLine($"Usuario del cliente: {userFromClient.Name} ({userFromClient.Email})");
                    
                    dto.Client = new ClientInfoDto
                    {
                        Id = clientById.Id,
                        Name = userFromClient.Name,
                        Email = userFromClient.Email,
                        Phone = clientById.Phone,
                        ProfileImageUrl = userFromClient.ProfileImage,
                        RegistrationDate = userFromClient.RegistrationDate,
                        TotalReservations = 0,
                        Rating = null
                    };
                    
                    Console.WriteLine($"Cliente mapeado correctamente desde tabla clients: {dto.Client.Name}");
                    clientMapped = true; // Marcar como mapeado exitosamente
                }
            }
            
            // OPCIÓN 2: Solo si no se mapeó correctamente arriba, verificar si el clientId es realmente un userId (ESTRUCTURA INCORRECTA)
            if (!clientMapped)
            {
                var userById = await _context.Set<Rent2Go.API.IAM.Domain.Model.Aggregates.User>()
                    .FirstOrDefaultAsync(u => u.Id == reservation.ClientId);
                
                Console.WriteLine($"¿El clientId es un userId directo? {userById != null}");
                if (userById != null)
                {
                    Console.WriteLine($"Usuario encontrado: {userById.Name} ({userById.Email}) - Rol: {userById.Role}");
                    Console.WriteLine("ADVERTENCIA: El clientId apunta directamente a un usuario, no a la tabla clients");
                    
                    dto.Client = new ClientInfoDto
                    {
                        Id = reservation.ClientId,
                        Name = userById.Name,
                        Email = userById.Email,
                        Phone = "", // No tenemos teléfono si no hay registro en clients
                        ProfileImageUrl = userById.ProfileImage,
                        RegistrationDate = userById.RegistrationDate,
                        TotalReservations = 0,
                        Rating = null
                    };
                    
                    Console.WriteLine($"Cliente mapeado desde users (estructura incorrecta): {dto.Client.Name}");
                    clientMapped = true;
                }
            }
            
            // OPCIÓN 3: Solo si ninguna de las anteriores funcionó, crear cliente por defecto
            if (!clientMapped)
            {
                Console.WriteLine($"No se encontró ningún usuario ni cliente para ID: {reservation.ClientId}");
                
                // Mostrar información de debug sobre qué hay en las tablas
                var totalUsers = await _context.Set<Rent2Go.API.IAM.Domain.Model.Aggregates.User>().CountAsync();
                var totalClients = await _context.Set<Rent2Go.API.IAM.Domain.Model.Client>().CountAsync();
                
                Console.WriteLine($"Total usuarios en BD: {totalUsers}");
                Console.WriteLine($"Total clientes en BD: {totalClients}");
                
                if (totalClients > 0)
                {
                    var sampleClients = await _context.Set<Rent2Go.API.IAM.Domain.Model.Client>()
                        .Take(3)
                        .Select(c => new { c.Id, c.UserId })
                        .ToListAsync();
                        
                    Console.WriteLine("Ejemplos de clientes en BD:");
                    foreach (var client in sampleClients)
                    {
                        Console.WriteLine($"  - Client ID: {client.Id}, User ID: {client.UserId}");
                    }
                }
                
                // Crear un cliente por defecto
                dto.Client = new ClientInfoDto
                {
                    Id = reservation.ClientId,
                    Name = "Cliente no encontrado",
                    Email = "",
                    Phone = "",
                    ProfileImageUrl = null,
                    RegistrationDate = DateTime.UtcNow,
                    TotalReservations = 0,
                    Rating = null
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar información del cliente: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            
            // Crear un cliente por defecto en caso de error
            dto.Client = new ClientInfoDto
            {
                Id = reservation.ClientId,
                Name = "Error al cargar cliente",
                Email = "",
                Phone = "",
                ProfileImageUrl = null,
                RegistrationDate = DateTime.UtcNow,
                TotalReservations = 0,
                Rating = null
            };
        }

        // Obtener información del vehículo
        try
        {
            var vehicle = await _context.Set<Vehicle>()
                .FirstOrDefaultAsync(v => v.Id == reservation.VehicleId);

            if (vehicle != null)
            {
                // Cargar imágenes del vehículo
                var vehicleImages = await _context.Set<VehicleImage>()
                    .Where(vi => vi.VehicleId == vehicle.Id)
                    .OrderByDescending(vi => vi.IsPrimary)
                    .ThenByDescending(vi => vi.CreatedAt)
                    .Select(vi => vi.ImageUrl)
                    .ToListAsync();

                Console.WriteLine($"Cargando vehículo {vehicle.Id}: {vehicle.Brand} {vehicle.Model}");
                Console.WriteLine($"Imágenes encontradas para vehículo: {vehicleImages.Count}");

                dto.Vehicle = new ClientVehicleDto
                {
                    Id = vehicle.Id,
                    Brand = vehicle.Brand,
                    Model = vehicle.Model,
                    Year = vehicle.Year,
                    Price = vehicle.DailyRate,
                    Status = vehicle.Status,
                    Category = vehicle.Type, // Usar Type en lugar de Category
                    Seats = vehicle.Seats,
                    Luggage = 0, // La entidad Vehicle no tiene Luggage, usar 0
                    Transmission = vehicle.Transmission,
                    Rating = 0, // Calcular después si es necesario
                    ReviewCount = 0, // Calcular después si es necesario
                    Images = vehicleImages // Asignar las imágenes cargadas
                };
                
                Console.WriteLine($"DTO del vehículo creado con {dto.Vehicle.Images.Count} imágenes");
            }
            else
            {
                Console.WriteLine($"Vehículo no encontrado para ID: {reservation.VehicleId}");
                // Crear un vehículo por defecto si no se encuentra
                dto.Vehicle = new ClientVehicleDto
                {
                    Id = reservation.VehicleId,
                    Brand = "Vehículo no encontrado",
                    Model = "",
                    Year = 0,
                    Price = 0,
                    Status = "unknown",
                    Category = "",
                    Seats = 0,
                    Luggage = 0,
                    Transmission = "",
                    Rating = 0,
                    ReviewCount = 0,
                    Images = new List<string>()
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar información del vehículo: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            
            // Crear un vehículo por defecto en caso de error
            dto.Vehicle = new ClientVehicleDto
            {
                Id = reservation.VehicleId,
                Brand = "Error al cargar vehículo",
                Model = "",
                Year = 0,
                Price = 0,
                Status = "unknown",
                Category = "",
                Seats = 0,
                Luggage = 0,
                Transmission = "",
                Rating = 0,
                ReviewCount = 0,
                Images = new List<string>()
            };
        }

        return dto;
    }
}
