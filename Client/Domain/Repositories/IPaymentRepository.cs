using BackendRent2Go.Client.Domain.Model.Entities;

namespace BackendRent2Go.Client.Domain.Repositories;

public interface IPaymentRepository
{
    Task<IEnumerable<Payment>> GetPaymentsByUserIdAsync(string userId);
    Task<Payment?> GetPaymentByIdAsync(string paymentId);
    Task<Payment?> GetPaymentByReservationIdAsync(string reservationId);
    Task<Payment> CreatePaymentAsync(Payment payment);
    Task<Payment> UpdatePaymentAsync(Payment payment);
    Task<IEnumerable<Payment>> GetPaymentHistoryByUserIdAsync(string userId, int page = 1, int pageSize = 10);
}
