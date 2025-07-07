using BackendRent2Go.Client.Domain.Model;
using BackendRent2Go.Client.Domain.Model.Entities;

namespace BackendRent2Go.Client.Domain.Services;

public interface IPaymentMethodService
{
    Task<IEnumerable<PaymentMethodDto>> GetUserPaymentMethodsAsync(string userId);
    Task<PaymentMethodDto?> GetPaymentMethodByIdAsync(string paymentMethodId);
    Task<PaymentMethodDto> CreatePaymentMethodAsync(string userId, CreatePaymentMethodDto createDto);
    Task<PaymentMethodDto> UpdatePaymentMethodAsync(string paymentMethodId, UpdatePaymentMethodDto updateDto);
    Task<bool> DeletePaymentMethodAsync(string paymentMethodId);
    Task<bool> SetDefaultPaymentMethodAsync(string userId, string paymentMethodId);
    Task<PaymentMethodDto?> GetDefaultPaymentMethodAsync(string userId);
}
