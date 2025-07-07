using BackendRent2Go.Provider.Domain.Model;
using BackendRent2Go.Provider.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackendRent2Go.Provider.Interfaces.REST;

[ApiController]
[Route("api/provider/reservations")]
[Authorize] // Requiere autenticación
public class ProviderReservationsController : ControllerBase
{
    private readonly IProviderReservationService _providerReservationService;

    public ProviderReservationsController(IProviderReservationService providerReservationService)
    {
        _providerReservationService = providerReservationService;
    }

    /// <summary>
    /// Obtiene todas las reservas del provider actual
    /// </summary>
    /// <param name="status">Filtro opcional por estado</param>
    /// <param name="vehicleId">Filtro opcional por vehículo</param>
    /// <returns>Lista de reservas del provider</returns>
    [HttpGet]
    public async Task<IActionResult> GetMyReservations(
        [FromQuery] string? status = null, 
        [FromQuery] string? vehicleId = null)
    {
        try
        {
            var userId = GetUserId();
            Console.WriteLine($"ProviderReservationsController: Obteniendo reservas para provider {userId}");

            IEnumerable<ProviderReservationDto> reservations;

            if (!string.IsNullOrEmpty(vehicleId))
            {
                reservations = await _providerReservationService.GetReservationsByVehicle(vehicleId, userId);
            }
            else if (!string.IsNullOrEmpty(status))
            {
                reservations = await _providerReservationService.GetReservationsByStatus(userId, status);
            }
            else
            {
                reservations = await _providerReservationService.GetMyReservations(userId);
            }

            Console.WriteLine($"ProviderReservationsController: Devolviendo {reservations.Count()} reservas");
            return Ok(reservations);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en GetMyReservations: {ex.Message}");
            return BadRequest(new { message = "Error al obtener las reservas", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene una reserva específica por ID
    /// </summary>
    /// <param name="id">ID de la reserva</param>
    /// <returns>Detalles de la reserva</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetReservationById(string id)
    {
        try
        {
            var userId = GetUserId();
            Console.WriteLine($"ProviderReservationsController: Obteniendo reserva {id} para provider {userId}");

            var reservation = await _providerReservationService.GetReservationById(id, userId);

            if (reservation == null)
            {
                return NotFound(new { message = "Reserva no encontrada" });
            }

            return Ok(reservation);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en GetReservationById: {ex.Message}");
            return BadRequest(new { message = "Error al obtener la reserva", error = ex.Message });
        }
    }

    /// <summary>
    /// Actualiza el estado de una reserva (confirmar, completar, etc.)
    /// </summary>
    /// <param name="id">ID de la reserva</param>
    /// <param name="updateStatusDto">Nuevo estado de la reserva</param>
    /// <returns>Reserva actualizada</returns>
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateReservationStatus(string id, [FromBody] UpdateReservationStatusDto updateStatusDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();
            Console.WriteLine($"ProviderReservationsController: Actualizando estado de reserva {id} a {updateStatusDto.Status}");

            var reservation = await _providerReservationService.UpdateReservationStatus(id, userId, updateStatusDto);

            return Ok(reservation);
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Error de operación en UpdateReservationStatus: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en UpdateReservationStatus: {ex}");
            return StatusCode(500, new { message = "Error al actualizar el estado de la reserva", detail = ex.Message });
        }
    }

    /// <summary>
    /// Confirma una reserva pendiente
    /// </summary>
    /// <param name="id">ID de la reserva</param>
    /// <param name="notes">Notas opcionales</param>
    /// <returns>Reserva confirmada</returns>
    [HttpPost("{id}/confirm")]
    public async Task<IActionResult> ConfirmReservation(string id, [FromBody] string? notes = null)
    {
        try
        {
            var userId = GetUserId();
            Console.WriteLine($"ProviderReservationsController: Confirmando reserva {id}");

            var updateStatusDto = new UpdateReservationStatusDto
            {
                Status = "confirmed",
                Notes = notes
            };

            var reservation = await _providerReservationService.UpdateReservationStatus(id, userId, updateStatusDto);

            return Ok(reservation);
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Error al confirmar reserva: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en ConfirmReservation: {ex}");
            return StatusCode(500, new { message = "Error al confirmar la reserva", detail = ex.Message });
        }
    }

    /// <summary>
    /// Rechaza una reserva pendiente
    /// </summary>
    /// <param name="id">ID de la reserva</param>
    /// <param name="rejectReservationDto">Motivo de rechazo</param>
    /// <returns>Reserva rechazada</returns>
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> RejectReservation(string id, [FromBody] RejectReservationDto rejectReservationDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();
            Console.WriteLine($"ProviderReservationsController: Rechazando reserva {id}");

            var reservation = await _providerReservationService.RejectReservation(id, userId, rejectReservationDto);

            return Ok(reservation);
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Error al rechazar reserva: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en RejectReservation: {ex}");
            return StatusCode(500, new { message = "Error al rechazar la reserva", detail = ex.Message });
        }
    }

    /// <summary>
    /// Marca una reserva como en progreso (vehículo entregado)
    /// </summary>
    /// <param name="id">ID de la reserva</param>
    /// <param name="notes">Notas opcionales</param>
    /// <returns>Reserva actualizada</returns>
    [HttpPost("{id}/start")]
    public async Task<IActionResult> StartReservation(string id, [FromBody] string? notes = null)
    {
        try
        {
            var userId = GetUserId();
            Console.WriteLine($"ProviderReservationsController: Iniciando reserva {id}");

            var updateStatusDto = new UpdateReservationStatusDto
            {
                Status = "in_progress",
                Notes = notes
            };

            var reservation = await _providerReservationService.UpdateReservationStatus(id, userId, updateStatusDto);

            return Ok(reservation);
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Error al iniciar reserva: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en StartReservation: {ex}");
            return StatusCode(500, new { message = "Error al iniciar la reserva", detail = ex.Message });
        }
    }

    /// <summary>
    /// Marca una reserva como completada (vehículo devuelto)
    /// </summary>
    /// <param name="id">ID de la reserva</param>
    /// <param name="notes">Notas opcionales</param>
    /// <returns>Reserva completada</returns>
    [HttpPost("{id}/complete")]
    public async Task<IActionResult> CompleteReservation(string id, [FromBody] string? notes = null)
    {
        try
        {
            var userId = GetUserId();
            Console.WriteLine($"ProviderReservationsController: Completando reserva {id}");

            var updateStatusDto = new UpdateReservationStatusDto
            {
                Status = "completed",
                Notes = notes
            };

            var reservation = await _providerReservationService.UpdateReservationStatus(id, userId, updateStatusDto);

            return Ok(reservation);
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Error al completar reserva: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en CompleteReservation: {ex}");
            return StatusCode(500, new { message = "Error al completar la reserva", detail = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene reservas en un rango de fechas específico
    /// </summary>
    /// <param name="startDate">Fecha de inicio</param>
    /// <param name="endDate">Fecha de fin</param>
    /// <returns>Lista de reservas en el rango</returns>
    [HttpGet("date-range")]
    public async Task<IActionResult> GetReservationsByDateRange(
        [FromQuery] DateTime startDate, 
        [FromQuery] DateTime endDate)
    {
        try
        {
            if (startDate >= endDate)
            {
                return BadRequest(new { message = "La fecha de inicio debe ser anterior a la fecha de fin" });
            }

            var userId = GetUserId();
            Console.WriteLine($"ProviderReservationsController: Obteniendo reservas entre {startDate:yyyy-MM-dd} y {endDate:yyyy-MM-dd}");

            var reservations = await _providerReservationService.GetReservationsByDateRange(userId, startDate, endDate);

            return Ok(reservations);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en GetReservationsByDateRange: {ex.Message}");
            return BadRequest(new { message = "Error al obtener las reservas por rango de fechas", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene estadísticas de reservas del provider
    /// </summary>
    /// <returns>Estadísticas de reservas</returns>
    [HttpGet("stats")]
    public async Task<IActionResult> GetReservationStats()
    {
        try
        {
            var userId = GetUserId();
            Console.WriteLine($"ProviderReservationsController: Obteniendo estadísticas para provider {userId}");

            var allReservations = await _providerReservationService.GetMyReservations(userId);
            
            var stats = new ProviderReservationStatsDto
            {
                TotalReservations = allReservations.Count(),
                PendingReservations = allReservations.Count(r => r.Status == "pending"),
                ConfirmedReservations = allReservations.Count(r => r.Status == "confirmed"),
                CompletedReservations = allReservations.Count(r => r.Status == "completed"),
                CancelledReservations = allReservations.Count(r => r.Status == "cancelled"),
                TotalEarnings = allReservations.Where(r => r.Status == "completed").Sum(r => r.ProviderEarnings),
                MonthlyEarnings = allReservations
                    .Where(r => r.Status == "completed" && r.CreatedAt >= DateTime.Now.AddMonths(-1))
                    .Sum(r => r.ProviderEarnings)
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en GetReservationStats: {ex.Message}");
            return BadRequest(new { message = "Error al obtener las estadísticas", error = ex.Message });
        }
    }

    private string GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            throw new InvalidOperationException("Usuario no autenticado");
        return userIdClaim.Value;
    }
}
