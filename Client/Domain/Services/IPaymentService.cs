using BackendRent2Go.Client.Domain.Model;
using BackendRent2Go.Client.Domain.Model.Entities;

namespace BackendRent2Go.Client.Domain.Services;

public interface IPaymentService
{
    Task<IEnumerable<PaymentDto>> GetUserPaymentsAsync(string userId);
    Task<PaymentDto?> GetPaymentByIdAsync(string paymentId);
    Task<PaymentDto?> GetPaymentByReservationIdAsync(string reservationId);
    Task<PaymentDto> CreatePaymentAsync(string userId, ProcessPaymentDto createDto);
    Task<PaymentDto> UpdatePaymentStatusAsync(string paymentId, string status, string? notes = null);
    Task<IEnumerable<PaymentDto>> GetPaymentHistoryAsync(string userId, int page = 1, int pageSize = 10);
    Task<PaymentDto> ProcessPaymentAsync(string reservationId, ProcessPaymentDto paymentData);
}
