using BackendRent2Go.Client.Domain.Model;
using BackendRent2Go.Client.Domain.Model.Entities;
using BackendRent2Go.Client.Domain.Repositories;
using BackendRent2Go.Client.Domain.Services;
using BackendRent2Go.Shared.Domain.Repositories;

namespace BackendRent2Go.Client.Application.Internal;

public class PaymentMethodService : IPaymentMethodService
{
    private readonly IPaymentMethodRepository _paymentMethodRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentMethodService(IPaymentMethodRepository paymentMethodRepository, IUnitOfWork unitOfWork)
    {
        _paymentMethodRepository = paymentMethodRepository;
        _unitOfWork = unitOfWork;
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
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Type = createDto.Type,
            IsDefault = createDto.SetAsDefault,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Configurar campos específicos según el tipo
        switch (createDto.Type)
        {
            case "credit_card":
                paymentMethod.CardHolder = createDto.CardHolder;
                if (!string.IsNullOrEmpty(createDto.CardNumber))
                {
                    var cleanCardNumber = createDto.CardNumber.Replace(" ", "");
                    paymentMethod.CardNumberLast4 = cleanCardNumber.Length >= 4 ? 
                        cleanCardNumber.Substring(cleanCardNumber.Length - 4) : cleanCardNumber;
                }
                paymentMethod.CardExpiry = createDto.CardExpiry;
                paymentMethod.CardType = DetectCardType(createDto.CardNumber);
                break;
            case "paypal":
                paymentMethod.PaypalEmail = createDto.PaypalEmail;
                break;
            case "bank_account":
                paymentMethod.BankName = createDto.BankName;
                if (!string.IsNullOrEmpty(createDto.AccountNumber))
                {
                    paymentMethod.AccountNumberLast4 = createDto.AccountNumber.Length >= 4 ? 
                        createDto.AccountNumber.Substring(createDto.AccountNumber.Length - 4) : createDto.AccountNumber;
                }
                break;
        }

        // Si se marca como predeterminado, desmarcar otros métodos del usuario
        if (createDto.SetAsDefault)
        {
            await _paymentMethodRepository.UnsetDefaultForUserAsync(userId);
        }

        var createdPaymentMethod = await _paymentMethodRepository.CreatePaymentMethodAsync(paymentMethod);
        await _unitOfWork.CompleteAsync();

        return MapToDto(createdPaymentMethod);
    }

    public async Task<PaymentMethodDto> UpdatePaymentMethodAsync(string paymentMethodId, UpdatePaymentMethodDto updateDto)
    {
        var paymentMethod = await _paymentMethodRepository.GetPaymentMethodByIdAsync(paymentMethodId);
        if (paymentMethod == null)
            throw new ArgumentException("Método de pago no encontrado");

        if (updateDto.SetAsDefault.HasValue && updateDto.SetAsDefault.Value)
        {
            await _paymentMethodRepository.UnsetDefaultForUserAsync(paymentMethod.UserId);
            paymentMethod.IsDefault = true;
        }

        if (!string.IsNullOrEmpty(updateDto.CardExpiry))
        {
            paymentMethod.CardExpiry = updateDto.CardExpiry;
        }

        paymentMethod.UpdatedAt = DateTime.UtcNow;

        var updatedPaymentMethod = await _paymentMethodRepository.UpdatePaymentMethodAsync(paymentMethod);
        await _unitOfWork.CompleteAsync();

        return MapToDto(updatedPaymentMethod);
    }

    public async Task<bool> DeletePaymentMethodAsync(string paymentMethodId)
    {
        var result = await _paymentMethodRepository.DeletePaymentMethodAsync(paymentMethodId);
        if (result)
        {
            await _unitOfWork.CompleteAsync();
        }
        return result;
    }

    public async Task<bool> SetDefaultPaymentMethodAsync(string userId, string paymentMethodId)
    {
        var paymentMethod = await _paymentMethodRepository.GetPaymentMethodByIdAsync(paymentMethodId);
        if (paymentMethod == null || paymentMethod.UserId != userId)
            return false;

        // Desmarcar otros métodos como predeterminado
        await _paymentMethodRepository.UnsetDefaultForUserAsync(userId);

        // Marcar este método como predeterminado
        paymentMethod.IsDefault = true;
        paymentMethod.UpdatedAt = DateTime.UtcNow;

        await _paymentMethodRepository.UpdatePaymentMethodAsync(paymentMethod);
        await _unitOfWork.CompleteAsync();

        return true;
    }

    public async Task<PaymentMethodDto?> GetDefaultPaymentMethodAsync(string userId)
    {
        var paymentMethod = await _paymentMethodRepository.GetDefaultPaymentMethodAsync(userId);
        return paymentMethod != null ? MapToDto(paymentMethod) : null;
    }

    private PaymentMethodDto MapToDto(PaymentMethod paymentMethod)
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

    private string DetectCardType(string? cardNumber)
    {
        if (string.IsNullOrEmpty(cardNumber))
            return "unknown";

        var number = cardNumber.Replace(" ", "");
        if (number.StartsWith("4")) return "visa";
        if (number.StartsWith("5") || number.StartsWith("2")) return "mastercard";
        if (number.StartsWith("3")) return "amex";
        return "unknown";
    }
}
