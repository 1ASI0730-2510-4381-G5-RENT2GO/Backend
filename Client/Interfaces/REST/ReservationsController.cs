using BackendRent2Go.Client.Domain.Model;
using BackendRent2Go.Client.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackendRent2Go.Client.Interfaces.REST;

[ApiController]
[Route("api/client/reservations")]
[Authorize] // Requiere autenticación
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationsController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    /// <summary>
    /// Obtiene todas las reservas del cliente actual
    /// </summary>
    /// <param name="status">Filtro opcional por estado</param>
    /// <returns>Lista de reservas del cliente</returns>
    [HttpGet]
    public async Task<IActionResult> GetMyReservations([FromQuery] string? status = null)
    {
        try
        {
            var clientId = GetClientId();
            Console.WriteLine($"ReservationsController: Obteniendo reservas para cliente {clientId}");

            IEnumerable<ReservationDto> reservations;

            if (!string.IsNullOrEmpty(status))
            {
                reservations = await _reservationService.GetReservationsByStatus(clientId, status);
            }
            else
            {
                reservations = await _reservationService.GetMyReservations(clientId);
            }

            Console.WriteLine($"ReservationsController: Devolviendo {reservations.Count()} reservas");
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
            var clientId = GetClientId();
            Console.WriteLine($"ReservationsController: Obteniendo reserva {id} para cliente {clientId}");

            var reservation = await _reservationService.GetReservationById(id, clientId);

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
    /// Crea una nueva reserva
    /// </summary>
    /// <param name="createReservationDto">Datos de la reserva</param>
    /// <returns>Reserva creada</returns>
    [HttpPost]
    public async Task<IActionResult> CreateReservation([FromBody] CreateReservationDto createReservationDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var clientId = GetClientId();
            Console.WriteLine($"ReservationsController: Creando reserva para cliente {clientId}");

            var reservation = await _reservationService.CreateReservation(clientId, createReservationDto);

            return CreatedAtAction(
                nameof(GetReservationById),
                new { id = reservation.Id },
                reservation
            );
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Error de operación en CreateReservation: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en CreateReservation: {ex}");
            var detail = ex.ToString();
            return StatusCode(500, new { message = "Error al crear la reserva", detail });
        }
    }

    /// <summary>
    /// Actualiza una reserva existente
    /// </summary>
    /// <param name="id">ID de la reserva</param>
    /// <param name="updateReservationDto">Datos a actualizar</param>
    /// <returns>Reserva actualizada</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateReservation(string id, [FromBody] UpdateReservationDto updateReservationDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var clientId = GetClientId();
            Console.WriteLine($"ReservationsController: Actualizando reserva {id} para cliente {clientId}");

            var reservation = await _reservationService.UpdateReservation(id, clientId, updateReservationDto);

            return Ok(reservation);
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Error de operación en UpdateReservation: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en UpdateReservation: {ex.Message}");
            return BadRequest(new { message = "Error al actualizar la reserva", error = ex.Message });
        }
    }

    /// <summary>
    /// Cancela una reserva
    /// </summary>
    /// <param name="id">ID de la reserva</param>
    /// <param name="cancelReservationDto">Motivo de cancelación</param>
    /// <returns>Reserva cancelada</returns>
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelReservation(string id, [FromBody] CancelReservationDto cancelReservationDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var clientId = GetClientId();
            Console.WriteLine($"ReservationsController: Cancelando reserva {id} para cliente {clientId}");

            var reservation = await _reservationService.CancelReservation(id, clientId, cancelReservationDto);

            return Ok(reservation);
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Error de operación en CancelReservation: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en CancelReservation: {ex.Message}");
            return BadRequest(new { message = "Error al cancelar la reserva", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene el ID del cliente desde el token JWT
    /// </summary>
    /// <returns>ID del cliente</returns>
    private string GetClientId()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("Usuario no autenticado");
        }

        // En este caso, asumimos que el clientId es el mismo que el userId
        // Si tienes una lógica diferente para obtener el clientId, ajusta aquí
        return userId;
    }
}
