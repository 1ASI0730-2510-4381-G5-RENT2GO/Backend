using BackendRent2Go.Client.Domain.Model;
using BackendRent2Go.Client.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackendRent2Go.Client.Interfaces.REST;

[ApiController]
[Route("api/client/v1/payment-methods")]
[Authorize]
public class PaymentMethodsController : ControllerBase
{
    private readonly IPaymentMethodService _paymentMethodService;

    public PaymentMethodsController(IPaymentMethodService paymentMethodService)
    {
        _paymentMethodService = paymentMethodService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaymentMethodDto>>> GetUserPaymentMethods()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Usuario no autenticado");

            var paymentMethods = await _paymentMethodService.GetUserPaymentMethodsAsync(userId);
            return Ok(paymentMethods);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener métodos de pago", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PaymentMethodDto>> GetPaymentMethod(string id)
    {
        try
        {
            var paymentMethod = await _paymentMethodService.GetPaymentMethodByIdAsync(id);
            if (paymentMethod == null)
                return NotFound(new { message = "Método de pago no encontrado" });

            return Ok(paymentMethod);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener método de pago", error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<PaymentMethodDto>> CreatePaymentMethod([FromBody] CreatePaymentMethodDto createDto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Usuario no autenticado");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validaciones básicas
            if (string.IsNullOrEmpty(createDto.Type))
                return BadRequest(new { message = "El tipo de método de pago es requerido" });

            if (createDto.Type == "credit_card")
            {
                if (string.IsNullOrEmpty(createDto.CardNumber) || string.IsNullOrEmpty(createDto.CardHolder) || string.IsNullOrEmpty(createDto.CardExpiry))
                    return BadRequest(new { message = "Información de tarjeta incompleta" });
            }
            else if (createDto.Type == "paypal")
            {
                if (string.IsNullOrEmpty(createDto.PaypalEmail))
                    return BadRequest(new { message = "Email de PayPal es requerido" });
            }
            else if (createDto.Type == "bank_account")
            {
                if (string.IsNullOrEmpty(createDto.BankName) || string.IsNullOrEmpty(createDto.AccountNumber))
                    return BadRequest(new { message = "Información bancaria incompleta" });
            }

            var paymentMethod = await _paymentMethodService.CreatePaymentMethodAsync(userId, createDto);
            return CreatedAtAction(nameof(GetPaymentMethod), new { id = paymentMethod.Id }, paymentMethod);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al crear método de pago", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PaymentMethodDto>> UpdatePaymentMethod(string id, [FromBody] UpdatePaymentMethodDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var paymentMethod = await _paymentMethodService.UpdatePaymentMethodAsync(id, updateDto);
            return Ok(paymentMethod);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al actualizar método de pago", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletePaymentMethod(string id)
    {
        try
        {
            var result = await _paymentMethodService.DeletePaymentMethodAsync(id);
            if (!result)
                return NotFound(new { message = "Método de pago no encontrado" });

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al eliminar método de pago", error = ex.Message });
        }
    }

    [HttpPost("{id}/set-default")]
    public async Task<ActionResult<PaymentMethodDto>> SetDefaultPaymentMethod(string id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Usuario no autenticado");

            var result = await _paymentMethodService.SetDefaultPaymentMethodAsync(userId, id);
            if (!result)
                return NotFound(new { message = "Método de pago no encontrado" });

            var paymentMethod = await _paymentMethodService.GetPaymentMethodByIdAsync(id);
            return Ok(paymentMethod);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al establecer método predeterminado", error = ex.Message });
        }
    }

    [HttpGet("default")]
    public async Task<ActionResult<PaymentMethodDto>> GetDefaultPaymentMethod()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Usuario no autenticado");

            var paymentMethod = await _paymentMethodService.GetDefaultPaymentMethodAsync(userId);
            if (paymentMethod == null)
                return NotFound(new { message = "No hay método de pago predeterminado" });

            return Ok(paymentMethod);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener método predeterminado", error = ex.Message });
        }
    }
}
