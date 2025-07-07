using BackendRent2Go.Client.Domain.Model;
using BackendRent2Go.Client.Domain.Model.Entities;
using BackendRent2Go.Client.Domain.Repositories;
using BackendRent2Go.Client.Domain.Services;
using BackendRent2Go.Shared.Domain.Repositories;

namespace BackendRent2Go.Client.Application.Internal;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentMethodRepository _paymentMethodRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IPaymentMethodRepository paymentMethodRepository,
        IUnitOfWork unitOfWork)
    {
        _paymentRepository = paymentRepository;
        _paymentMethodRepository = paymentMethodRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<PaymentDto>> GetUserPaymentsAsync(string userId)
    {
        var payments = await _paymentRepository.GetPaymentsByUserIdAsync(userId);
        var paymentDtos = new List<PaymentDto>();

        foreach (var payment in payments)
        {
            var dto = await MapToDtoAsync(payment);
            paymentDtos.Add(dto);
        }

        return paymentDtos;
    }

    public async Task<PaymentDto?> GetPaymentByIdAsync(string paymentId)
    {
        var payment = await _paymentRepository.GetPaymentByIdAsync(paymentId);
        return payment != null ? await MapToDtoAsync(payment) : null;
    }

    public async Task<PaymentDto?> GetPaymentByReservationIdAsync(string reservationId)
    {
        var payment = await _paymentRepository.GetPaymentByReservationIdAsync(reservationId);
        return payment != null ? await MapToDtoAsync(payment) : null;
    }

    public async Task<PaymentDto> CreatePaymentAsync(string userId, ProcessPaymentDto createDto)
    {
        var payment = new Payment
        {
            Id = Guid.NewGuid().ToString(),
            ReservationId = createDto.ReservationId,
            PaymentMethodId = null, // ProcessPaymentDto no tiene PaymentMethodId
            Amount = createDto.Amount,
            Status = "pending",
            Notes = createDto.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdPayment = await _paymentRepository.CreatePaymentAsync(payment);
        await _unitOfWork.CompleteAsync();
        return await MapToDtoAsync(createdPayment);
    }

    public async Task<PaymentDto> UpdatePaymentStatusAsync(string paymentId, string status, string? notes = null)
    {
        var payment = await _paymentRepository.GetPaymentByIdAsync(paymentId);
        if (payment == null)
            throw new ArgumentException("Pago no encontrado");

        payment.Status = status;
        payment.Notes = notes ?? payment.Notes;
        payment.UpdatedAt = DateTime.UtcNow;

        if (status == "completed")
        {
            payment.PaymentDate = DateTime.UtcNow;
            payment.TransactionId = payment.TransactionId ?? $"TXN_{DateTime.UtcNow:yyyyMMddHHmmss}_{paymentId[..8]}";
        }

        var updatedPayment = await _paymentRepository.UpdatePaymentAsync(payment);
        await _unitOfWork.CompleteAsync();
        return await MapToDtoAsync(updatedPayment);
    }

    public async Task<IEnumerable<PaymentDto>> GetPaymentHistoryAsync(string userId, int page = 1, int pageSize = 10)
    {
        var payments = await _paymentRepository.GetPaymentHistoryByUserIdAsync(userId, page, pageSize);
        var paymentDtos = new List<PaymentDto>();

        foreach (var payment in payments)
        {
            var dto = await MapToDtoAsync(payment);
            paymentDtos.Add(dto);
        }

        return paymentDtos;
    }

    public async Task<PaymentDto> ProcessPaymentAsync(string reservationId, ProcessPaymentDto paymentData)
    {
        var payment = new Payment
        {
            Id = Guid.NewGuid().ToString(),
            ReservationId = reservationId,
            Amount = paymentData.Amount,
            Status = "pending",
            Notes = paymentData.Notes ?? "Pago procesado automáticamente",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Si es pago con tarjeta y se debe guardar, crear método de pago
        if (paymentData.SaveCard && !string.IsNullOrEmpty(paymentData.CardNumber))
        {
            var paymentMethod = new PaymentMethod
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "temp_user", // Este valor debe venir del contexto de autenticación
                Type = "credit_card",
                CardHolder = paymentData.CardHolder,
                CardNumberLast4 = paymentData.CardNumber.Replace(" ", "").Substring(Math.Max(0, paymentData.CardNumber.Replace(" ", "").Length - 4)),
                CardExpiry = paymentData.CardExpiry,
                CardType = DetectCardType(paymentData.CardNumber),
                IsDefault = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _paymentMethodRepository.AddAsync(paymentMethod);
            payment.PaymentMethodId = paymentMethod.Id;
        }

        // Simular procesamiento exitoso para demo
        try
        {
            await SimulatePaymentProcessing(paymentData);
            payment.Status = "completed";
            payment.PaymentDate = DateTime.UtcNow;
            payment.TransactionId = $"TXN_{DateTime.UtcNow:yyyyMMddHHmmss}_{payment.Id[..8]}";
        }
        catch (Exception ex)
        {
            payment.Status = "failed";
            payment.Notes = $"Error en el pago: {ex.Message}";
        }

        var createdPayment = await _paymentRepository.CreatePaymentAsync(payment);
        await _unitOfWork.CompleteAsync();

        return await MapToDtoAsync(createdPayment);
    }

    private async Task<PaymentDto> MapToDtoAsync(Payment payment)
    {
        var dto = new PaymentDto
        {
            Id = payment.Id,
            ReservationId = payment.ReservationId,
            Amount = payment.Amount,
            Status = payment.Status,
            TransactionId = payment.TransactionId,
            PaymentDate = payment.PaymentDate,
            Notes = payment.Notes,
            CreatedAt = payment.CreatedAt
        };

        // Obtener información del método de pago si está disponible
        if (!string.IsNullOrEmpty(payment.PaymentMethodId))
        {
            var paymentMethod = await _paymentMethodRepository.GetPaymentMethodByIdAsync(payment.PaymentMethodId);
            if (paymentMethod != null)
            {
                dto.PaymentMethodType = paymentMethod.Type;
                dto.CardNumberLast4 = paymentMethod.CardNumberLast4;
                dto.CardType = paymentMethod.CardType;
            }
        }

        return dto;
    }

    private static async Task SimulatePaymentProcessing(ProcessPaymentDto paymentData)
    {
        // Simular delay de procesamiento
        await Task.Delay(1000);

        // Simular diferentes escenarios basados en el número de tarjeta
        if (!string.IsNullOrEmpty(paymentData.CardNumber))
        {
            var cardNumber = paymentData.CardNumber.Replace(" ", "");
            
            // Tarjeta que simula rechazo
            if (cardNumber.EndsWith("0002"))
            {
                throw new Exception("payment_declined");
            }
            
            // Tarjeta que simula fondos insuficientes
            if (cardNumber.EndsWith("9995"))
            {
                throw new Exception("insufficient_funds");
            }
            
            // Tarjeta que simula error de procesamiento
            if (cardNumber.EndsWith("0341"))
            {
                throw new Exception("processing_error");
            }
        }

        // Si llegamos aquí, el pago es exitoso
    }

    private string DetectCardType(string cardNumber)
    {
        var number = cardNumber.Replace(" ", "");
        if (number.StartsWith("4")) return "visa";
        if (number.StartsWith("5") || number.StartsWith("2")) return "mastercard";
        if (number.StartsWith("3")) return "amex";
        return "unknown";
    }
}
