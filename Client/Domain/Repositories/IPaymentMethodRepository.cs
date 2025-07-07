using BackendRent2Go.Client.Domain.Model.Entities;

namespace BackendRent2Go.Client.Domain.Repositories;

public interface IPaymentMethodRepository
{
    Task<IEnumerable<PaymentMethod>> GetPaymentMethodsByUserIdAsync(string userId);
    Task<PaymentMethod?> GetPaymentMethodByIdAsync(string paymentMethodId);
    Task<PaymentMethod> CreatePaymentMethodAsync(PaymentMethod paymentMethod);
    Task<PaymentMethod> UpdatePaymentMethodAsync(PaymentMethod paymentMethod);
    Task<bool> DeletePaymentMethodAsync(string paymentMethodId);
    Task<bool> SetDefaultPaymentMethodAsync(string userId, string paymentMethodId);
    Task<PaymentMethod?> GetDefaultPaymentMethodAsync(string userId);
    Task UnsetDefaultForUserAsync(string userId);
    Task AddAsync(PaymentMethod paymentMethod);
}
