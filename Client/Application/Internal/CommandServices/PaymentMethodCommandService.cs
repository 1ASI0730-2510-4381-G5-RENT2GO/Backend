using BackendRent2Go.Client.Domain.Model;
using BackendRent2Go.Client.Domain.Model.Entities;
using BackendRent2Go.Client.Domain.Repositories;
using BackendRent2Go.Client.Domain.Services;

namespace BackendRent2Go.Client.Application.Internal.CommandServices;

public class PaymentMethodCommandService : IPaymentMethodService
{
    private readonly IPaymentMethodRepository _paymentMethodRepository;

    public PaymentMethodCommandService(IPaymentMethodRepository paymentMethodRepository)
    {
        _paymentMethodRepository = paymentMethodRepository;
    }

    public async Task<IEnumerable<PaymentMethodDto>> GetUserPaymentMethodsAsync(string userId)
    {
        var paymentMethods = await _paymentMethodRepository.GetPaymentMethodsByUserIdAsync(userId);
        return paymentMethods.Select(MapToDto);
    }

    public async Task<PaymentMethodDto?> GetPaymentMethodByIdAsync(string paymentMethodId)
    {
        var paymentMethod = await _paymentMethodRepository.GetPaymentMethodByIdAsync(paymentMethodId);
        return paymentMethod != null ? MapToDto(paymentMethod) : null;
    }

    public async Task<PaymentMethodDto> CreatePaymentMethodAsync(string userId, CreatePaymentMethodDto createDto)
    {
        var paymentMethod = new PaymentMethod
        {
            UserId = userId,
            Type = createDto.Type,
            IsDefault = createDto.SetAsDefault,
            CardHolder = createDto.CardHolder,
            CardExpiry = createDto.CardExpiry,
            PaypalEmail = createDto.PaypalEmail,
            BankName = createDto.BankName
        };

        // Procesar información de tarjeta de crédito
        if (createDto.Type == "credit_card" && !string.IsNullOrEmpty(createDto.CardNumber))
        {
            // Guardar solo los últimos 4 dígitos por seguridad
            paymentMethod.CardNumberLast4 = createDto.CardNumber.Replace(" ", "").Substring(Math.Max(0, createDto.CardNumber.Replace(" ", "").Length - 4));
            
            // Determinar el tipo de tarjeta basado en el número
            paymentMethod.CardType = DetectCardType(createDto.CardNumber);
        }

        // Procesar información de cuenta bancaria
        if (createDto.Type == "bank_account" && !string.IsNullOrEmpty(createDto.AccountNumber))
        {
            paymentMethod.AccountNumberLast4 = createDto.AccountNumber.Substring(Math.Max(0, createDto.AccountNumber.Length - 4));
        }

        var createdPaymentMethod = await _paymentMethodRepository.CreatePaymentMethodAsync(paymentMethod);
        return MapToDto(createdPaymentMethod);
    }

    public async Task<PaymentMethodDto> UpdatePaymentMethodAsync(string paymentMethodId, UpdatePaymentMethodDto updateDto)
    {
        var paymentMethod = await _paymentMethodRepository.GetPaymentMethodByIdAsync(paymentMethodId);
        if (paymentMethod == null)
            throw new ArgumentException($"Payment method with ID {paymentMethodId} not found");

        // Actualizar campos modificables
        if (updateDto.SetAsDefault.HasValue)
        {
            paymentMethod.IsDefault = updateDto.SetAsDefault.Value;
        }

        if (!string.IsNullOrEmpty(updateDto.CardExpiry))
        {
            paymentMethod.CardExpiry = updateDto.CardExpiry;
        }

        var updatedPaymentMethod = await _paymentMethodRepository.UpdatePaymentMethodAsync(paymentMethod);
        return MapToDto(updatedPaymentMethod);
    }

    public async Task<bool> DeletePaymentMethodAsync(string paymentMethodId)
    {
        return await _paymentMethodRepository.DeletePaymentMethodAsync(paymentMethodId);
    }

    public async Task<bool> SetDefaultPaymentMethodAsync(string userId, string paymentMethodId)
    {
        return await _paymentMethodRepository.SetDefaultPaymentMethodAsync(userId, paymentMethodId);
    }

    public async Task<PaymentMethodDto?> GetDefaultPaymentMethodAsync(string userId)
    {
        var paymentMethod = await _paymentMethodRepository.GetDefaultPaymentMethodAsync(userId);
        return paymentMethod != null ? MapToDto(paymentMethod) : null;
    }

    private static PaymentMethodDto MapToDto(PaymentMethod paymentMethod)
    {
        return new PaymentMethodDto
        {
            Id = paymentMethod.Id,
            Type = paymentMethod.Type,
            IsDefault = paymentMethod.IsDefault,
            CardHolder = paymentMethod.CardHolder,
            CardNumberLast4 = paymentMethod.CardNumberLast4,
            CardExpiry = paymentMethod.CardExpiry,
            CardType = paymentMethod.CardType,
            PaypalEmail = paymentMethod.PaypalEmail,
            BankName = paymentMethod.BankName,
            AccountNumberLast4 = paymentMethod.AccountNumberLast4,
            CreatedAt = paymentMethod.CreatedAt
        };
    }

    private static string DetectCardType(string cardNumber)
    {
        var cleanedNumber = cardNumber.Replace(" ", "").Replace("-", "");
        
        if (cleanedNumber.StartsWith("4"))
            return "visa";
        else if (cleanedNumber.StartsWith("5") || (cleanedNumber.Length >= 4 && int.Parse(cleanedNumber.Substring(0, 4)) >= 2221 && int.Parse(cleanedNumber.Substring(0, 4)) <= 2720))
            return "mastercard";
        else if (cleanedNumber.StartsWith("34") || cleanedNumber.StartsWith("37"))
            return "amex";
        else if (cleanedNumber.StartsWith("6011") || cleanedNumber.StartsWith("65"))
            return "discover";
        else
            return "unknown";
    }
}
