using BackendRent2Go.Client.Domain.Model;
using BackendRent2Go.Client.Domain.Model.Entities;
using BackendRent2Go.Client.Domain.Services;
using BackendRent2Go.Data;
using Microsoft.EntityFrameworkCore;
using Rent2Go.API.IAM.Domain.Model; // for Client entity
using Rent2Go.API.Provider.Domain.Model; // for Vehicle entity

namespace BackendRent2Go.Client.Application.Internal.CommandServices;

public class ReservationService : IReservationService
{
    private readonly ApplicationDbContext _context;
    private readonly IVehicleService _vehicleService;
    private readonly IPaymentService _paymentService;

    public ReservationService(ApplicationDbContext context, IVehicleService vehicleService, IPaymentService paymentService)
    {
        _context = context;
        _vehicleService = vehicleService;
        _paymentService = paymentService;
    }

    // helper to map userId to clientId
    private async Task<string> GetClientIdFromUserAsync(string userId)
    {
        var client = await _context.Clients.FirstOrDefaultAsync(c => c.UserId == userId);
        if (client == null)
            throw new InvalidOperationException("Cliente no encontrado para el usuario");
        return client.Id;
    }

    public async Task<IEnumerable<ReservationDto>> GetMyReservations(string userId)
    {
        var clientId = await GetClientIdFromUserAsync(userId);

        try
        {
            Console.WriteLine($"ReservationService: Obteniendo reservas para cliente {clientId}");

            var reservations = await _context.Set<Reservation>()
                .Where(r => r.ClientId == clientId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            Console.WriteLine($"ReservationService: Encontradas {reservations.Count} reservas");

            var reservationDtos = new List<ReservationDto>();

            foreach (var reservation in reservations)
            {
                var reservationDto = await MapToDto(reservation);
                reservationDtos.Add(reservationDto);
            }

            return reservationDtos;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener reservas: {ex.Message}");
            throw;
        }
    }

    public async Task<ReservationDto?> GetReservationById(string id, string userId)
    {
        var clientId = await GetClientIdFromUserAsync(userId);

        try
        {
            var reservation = await _context.Set<Reservation>()
                .FirstOrDefaultAsync(r => r.Id == id && r.ClientId == clientId);

            if (reservation == null)
                return null;

            return await MapToDto(reservation);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener reserva {id}: {ex.Message}");
            throw;
        }
    }

    public async Task<ReservationDto> CreateReservation(string userId, CreateReservationDto createReservationDto)
    {
        var clientId = await GetClientIdFromUserAsync(userId);

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            Console.WriteLine($"ReservationService: Creando reserva para cliente {clientId}");

            // 1. Verificar que el cliente no tenga reservas superpuestas del mismo vehículo
            var hasClientOverlap = await _context.Set<Reservation>()
                .Where(r => r.ClientId == clientId && 
                           r.VehicleId == createReservationDto.VehicleId && 
                           r.Status != "cancelled" && r.Status != "completed")
                .AnyAsync(r => r.StartDate < createReservationDto.EndDate && r.EndDate > createReservationDto.StartDate);
            
            if (hasClientOverlap)
                throw new InvalidOperationException("Ya tienes una reserva para este vehículo en las fechas seleccionadas");

            // 2. Verificar disponibilidad del vehículo (solo reservas confirmadas o en progreso bloquean)
            var hasVehicleOverlap = await _context.Set<Reservation>()
                .Where(r => r.VehicleId == createReservationDto.VehicleId && 
                           (r.Status == "confirmed" || r.Status == "in_progress"))
                .AnyAsync(r => r.StartDate < createReservationDto.EndDate && r.EndDate > createReservationDto.StartDate);
            
            if (hasVehicleOverlap)
                throw new InvalidOperationException("El vehículo no está disponible en las fechas seleccionadas");

            // 3. Verificar que el vehículo esté disponible para reservar
            var vehicleEntity = await _context.Set<Vehicle>()
                .FirstOrDefaultAsync(v => v.Id == createReservationDto.VehicleId);
            
            if (vehicleEntity == null)
                throw new InvalidOperationException("Vehículo no encontrado");

            if (vehicleEntity.Status != "available")
                throw new InvalidOperationException($"El vehículo no está disponible para reservar. Estado actual: {vehicleEntity.Status}");

            // 4. Obtener detalles del vehículo para calcular el precio
            var vehicle = await _vehicleService.GetVehicleDetails(createReservationDto.VehicleId);
            if (vehicle == null)
                throw new InvalidOperationException("No se pudieron obtener los detalles del vehículo");

            var totalDays = (createReservationDto.EndDate - createReservationDto.StartDate).Days;
            if (totalDays <= 0)
                throw new InvalidOperationException("Las fechas de reserva no son válidas");

            var totalAmount = vehicle.Price * totalDays;

            // 5. Crear la reserva
            var reservation = new Reservation
            {
                Id = Guid.NewGuid().ToString(),
                ClientId = clientId,
                ProviderId = vehicleEntity.OwnerId,
                VehicleId = createReservationDto.VehicleId,
                StartDate = createReservationDto.StartDate,
                EndDate = createReservationDto.EndDate,
                Status = "pending", // Siempre empezar como pending hasta que el provider apruebe
                PaymentStatus = "pending",
                PaymentMethod = createReservationDto.PaymentMethod,
                TotalAmount = totalAmount,
                VehiclePrice = vehicle.Price,
                Location = createReservationDto.Location,
                Notes = createReservationDto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Set<Reservation>().Add(reservation);
            await _context.SaveChangesAsync();

            // 6. Procesar pago de forma asíncrona (sin bloquear la respuesta)
            _ = Task.Run(async () =>
            {
                try
                {
                    Console.WriteLine($"ReservationService: Procesando pago para reserva {reservation.Id}");
                    
                    var paymentData = new ProcessPaymentDto
                    {
                        ReservationId = reservation.Id,
                        Amount = totalAmount,
                        PaymentMethod = createReservationDto.PaymentMethod ?? "credit_card"
                    };

                    var payment = await _paymentService.ProcessPaymentAsync(reservation.Id, paymentData);
                    
                    // Actualizar solo el estado de pago, NO el estado de la reserva
                    using var scope = _context.Database.BeginTransaction();
                    var reservationToUpdate = await _context.Set<Reservation>()
                        .FirstOrDefaultAsync(r => r.Id == reservation.Id);
                    
                    if (reservationToUpdate != null)
                    {
                        reservationToUpdate.PaymentStatus = payment.Status;
                        reservationToUpdate.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                        await scope.CommitAsync();
                    }
                    
                    Console.WriteLine($"ReservationService: Pago procesado para reserva {reservation.Id} - Estado: {payment.Status}");
                }
                catch (Exception paymentEx)
                {
                    Console.WriteLine($"Error al procesar el pago para reserva {reservation.Id}: {paymentEx.Message}");
                    
                    // Actualizar estado de pago como fallido
                    using var scope = _context.Database.BeginTransaction();
                    var reservationToUpdate = await _context.Set<Reservation>()
                        .FirstOrDefaultAsync(r => r.Id == reservation.Id);
                    
                    if (reservationToUpdate != null)
                    {
                        reservationToUpdate.PaymentStatus = "failed";
                        reservationToUpdate.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                        await scope.CommitAsync();
                    }
                }
            });

            await transaction.CommitAsync();
            Console.WriteLine($"ReservationService: Reserva {reservation.Id} creada exitosamente con estado pending");

            return await MapToDto(reservation);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"Error al crear reserva: {ex.Message}");
            throw;
        }
    }

    public async Task<ReservationDto> UpdateReservation(string id, string userId, UpdateReservationDto updateReservationDto)
    {
        var clientId = await GetClientIdFromUserAsync(userId);

        try
        {
            var reservation = await _context.Set<Reservation>()
                .FirstOrDefaultAsync(r => r.Id == id && r.ClientId == clientId);

            if (reservation == null)
            {
                throw new InvalidOperationException("Reserva no encontrada");
            }

            if (!reservation.Status.Equals("pending") && !reservation.Status.Equals("confirmed"))
            {
                throw new InvalidOperationException("No se puede modificar una reserva que no esté pendiente o confirmada");
            }

            // Verificar si es muy tarde para modificar (48 horas antes)
            if (reservation.StartDate <= DateTime.Now.AddHours(48))
            {
                throw new InvalidOperationException("No se puede modificar una reserva con menos de 48 horas de anticipación");
            }

            // Actualizar campos
            if (updateReservationDto.StartDate.HasValue)
                reservation.StartDate = updateReservationDto.StartDate.Value;
            
            if (updateReservationDto.EndDate.HasValue)
                reservation.EndDate = updateReservationDto.EndDate.Value;
            
            if (!string.IsNullOrEmpty(updateReservationDto.Location))
                reservation.Location = updateReservationDto.Location;
            
            if (!string.IsNullOrEmpty(updateReservationDto.Notes))
                reservation.Notes = updateReservationDto.Notes;

            // Recalcular precio si las fechas cambiaron
            if (updateReservationDto.StartDate.HasValue || updateReservationDto.EndDate.HasValue)
            {
                var totalDays = (reservation.EndDate - reservation.StartDate).Days;
                reservation.TotalAmount = reservation.VehiclePrice * totalDays;
            }

            reservation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await MapToDto(reservation);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al actualizar reserva {id}: {ex.Message}");
            throw;
        }
    }

    public async Task<ReservationDto> CancelReservation(string id, string userId, CancelReservationDto cancelReservationDto)
    {
        var clientId = await GetClientIdFromUserAsync(userId);

        try
        {
            var reservation = await _context.Set<Reservation>()
                .FirstOrDefaultAsync(r => r.Id == id && r.ClientId == clientId);

            if (reservation == null)
            {
                throw new InvalidOperationException("Reserva no encontrada");
            }

            if (reservation.Status == "cancelled")
            {
                throw new InvalidOperationException("La reserva ya está cancelada");
            }

            if (reservation.Status == "completed")
            {
                throw new InvalidOperationException("No se puede cancelar una reserva completada");
            }

            // Verificar si es muy tarde para cancelar (24 horas antes)
            if (reservation.StartDate <= DateTime.Now.AddHours(24))
            {
                throw new InvalidOperationException("No se puede cancelar una reserva con menos de 24 horas de anticipación");
            }

            reservation.Status = "cancelled";
            reservation.PaymentStatus = "refunded";
            reservation.CancellationReason = cancelReservationDto.Reason;
            reservation.CancellationDate = DateTime.UtcNow;
            reservation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            Console.WriteLine($"ReservationService: Reserva {id} cancelada exitosamente");

            return await MapToDto(reservation);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cancelar reserva {id}: {ex.Message}");
            throw;
        }
    }

    public async Task<IEnumerable<ReservationDto>> GetReservationsByStatus(string userId, string status)
    {
        var clientId = await GetClientIdFromUserAsync(userId);

        try
        {
            var reservations = await _context.Set<Reservation>()
                .Where(r => r.ClientId == clientId && r.Status == status)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var reservationDtos = new List<ReservationDto>();

            foreach (var reservation in reservations)
            {
                var reservationDto = await MapToDto(reservation);
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

    private async Task<ReservationDto> MapToDto(Reservation reservation)
    {
        var reservationDto = new ReservationDto
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

        // Obtener información del vehículo
        try
        {
            var vehicle = await _vehicleService.GetVehicleDetails(reservation.VehicleId);
            if (vehicle != null)
            {
                reservationDto.Vehicle = vehicle;
                
                // Asegurar que el vehículo tenga al menos una imagen por defecto
                if (vehicle.Images == null || vehicle.Images.Count == 0)
                {
                    vehicle.Images = new List<string> { "/img/car-placeholder.jpg" };
                }

                Console.WriteLine($"Vehículo mapeado para reserva {reservation.Id}: {vehicle.Brand} {vehicle.Model}");
                Console.WriteLine($"Imágenes del vehículo: {string.Join(", ", vehicle.Images)}");
            }
            else
            {
                Console.WriteLine($"No se pudo obtener información del vehículo {reservation.VehicleId}");

                // Crear un vehículo básico para evitar errores en el frontend
                reservationDto.Vehicle = new BackendRent2Go.Client.Domain.Model.VehicleDto
                {
                    Id = reservation.VehicleId,
                    Brand = "Vehículo",
                    Model = "No disponible",
                    Year = DateTime.Now.Year,
                    Category = "Desconocido",
                    Price = reservation.VehiclePrice,
                    Images = new List<string> { "/img/car-placeholder.jpg" }
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener vehículo {reservation.VehicleId}: {ex.Message}");
            
            // Crear un vehículo básico para evitar errores en el frontend
            reservationDto.Vehicle = new BackendRent2Go.Client.Domain.Model.VehicleDto
            {
                Id = reservation.VehicleId,
                Brand = "Vehículo",
                Model = "No disponible",
                Year = DateTime.Now.Year,
                Category = "Desconocido",
                Price = reservation.VehiclePrice,
                Images = new List<string> { "/img/car-placeholder.jpg" }
            };
        }

        return reservationDto;
    }

    public async Task<ReservationDto> CompleteReservation(string id, string userId)
    {
        var clientId = await GetClientIdFromUserAsync(userId);

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var reservation = await _context.Set<Reservation>()
                .FirstOrDefaultAsync(r => r.Id == id && r.ClientId == clientId);

            if (reservation == null)
                throw new InvalidOperationException("Reserva no encontrada");

            if (reservation.Status != "in_progress")
                throw new InvalidOperationException("Solo se pueden completar reservas en progreso");

            // Actualizar estado de la reserva
            reservation.Status = "completed";
            reservation.UpdatedAt = DateTime.UtcNow;

            // Cambiar estado del vehículo a inspección
            var vehicle = await _context.Set<Vehicle>()
                .FirstOrDefaultAsync(v => v.Id == reservation.VehicleId);
            
            if (vehicle != null)
            {
                vehicle.Status = "inspection";
                vehicle.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            Console.WriteLine($"ReservationService: Reserva {id} completada. Vehículo {reservation.VehicleId} en inspección");

            return await MapToDto(reservation);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"Error al completar reserva {id}: {ex.Message}");
            throw;
        }
    }
}
