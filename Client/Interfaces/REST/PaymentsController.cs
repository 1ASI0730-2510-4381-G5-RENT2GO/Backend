using BackendRent2Go.Client.Domain.Model;
using BackendRent2Go.Client.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackendRent2Go.Client.Interfaces.REST;

[ApiController]
[Route("api/client/v1/payments")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetUserPayments()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Usuario no autenticado");

            var payments = await _paymentService.GetUserPaymentsAsync(userId);
            return Ok(payments);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener historial de pagos", error = ex.Message });
        }
    }

    [HttpGet("history")]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPaymentHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Usuario no autenticado");

            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 50) pageSize = 10;

            var payments = await _paymentService.GetPaymentHistoryAsync(userId, page, pageSize);
            return Ok(payments);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener historial de pagos", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PaymentDto>> GetPayment(string id)
    {
        try
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
                return NotFound(new { message = "Pago no encontrado" });

            return Ok(payment);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener pago", error = ex.Message });
        }
    }

    [HttpGet("reservation/{reservationId}")]
    public async Task<ActionResult<PaymentDto>> GetPaymentByReservation(string reservationId)
    {
        try
        {
            var payment = await _paymentService.GetPaymentByReservationIdAsync(reservationId);
            if (payment == null)
                return NotFound(new { message = "Pago no encontrado para esta reserva" });

            return Ok(payment);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener pago de reserva", error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<PaymentDto>> CreatePayment([FromBody] ProcessPaymentDto createDto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Usuario no autenticado");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var payment = await _paymentService.CreatePaymentAsync(userId, createDto);
            return CreatedAtAction(nameof(GetPayment), new { id = payment.Id }, payment);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al crear pago", error = ex.Message });
        }
    }

    [HttpPost("process")]
    public async Task<ActionResult<PaymentDto>> ProcessPayment([FromBody] ProcessPaymentDto processDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var payment = await _paymentService.ProcessPaymentAsync(processDto.ReservationId, processDto);
            return Ok(payment);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al procesar pago", error = ex.Message });
        }
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<PaymentDto>> UpdatePaymentStatus(string id, [FromBody] UpdatePaymentStatusDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var payment = await _paymentService.UpdatePaymentStatusAsync(id, updateDto.Status, updateDto.Notes);
            return Ok(payment);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al actualizar estado de pago", error = ex.Message });
        }
    }
}
