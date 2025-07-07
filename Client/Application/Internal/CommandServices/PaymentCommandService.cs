using BackendRent2Go.Client.Domain.Model;
using BackendRent2Go.Client.Domain.Model.Entities;
using BackendRent2Go.Client.Domain.Repositories;
using BackendRent2Go.Client.Domain.Services;
using BackendRent2Go.Shared.Domain.Repositories;
using BackendRent2Go.Data;
using Microsoft.EntityFrameworkCore;

namespace BackendRent2Go.Client.Application.Internal.CommandServices;

public class PaymentCommandService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentMethodRepository _paymentMethodRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _context;

    public PaymentCommandService(
        IPaymentRepository paymentRepository,
        IPaymentMethodRepository paymentMethodRepository,
        IUnitOfWork unitOfWork,
        ApplicationDbContext context)
    {
        _paymentRepository = paymentRepository;
        _paymentMethodRepository = paymentMethodRepository;
        _unitOfWork = unitOfWork;
        _context = context;
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
            Notes = $"Pago creado por usuario {userId}",
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
            throw new ArgumentException($"Payment with ID {paymentId} not found");

        payment.Status = status;
        payment.PaymentDate = status == "completed" ? DateTime.UtcNow : payment.PaymentDate;
        payment.UpdatedAt = DateTime.UtcNow;
        
        if (!string.IsNullOrEmpty(notes))
        {
            payment.Notes = notes;
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
            PaymentMethodId = null, // Se asigna después si se crea un nuevo método de pago
            Amount = paymentData.Amount,
            Status = "pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Si es un pago con nueva tarjeta y se debe guardar
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

        try
        {
            // Simular procesamiento del pago
            await SimulatePaymentProcessing(paymentData);
            
            payment.Status = "completed";
            payment.PaymentDate = DateTime.UtcNow;
            payment.TransactionId = GenerateTransactionId();
            payment.Notes = $"Pago procesado exitosamente - tarjeta";
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
            CreatedAt = payment.CreatedAt,
            
            // Información del método de pago si está disponible
            PaymentMethodType = payment.PaymentMethod?.Type,
            CardNumberLast4 = payment.PaymentMethod?.CardNumberLast4,
            CardType = payment.PaymentMethod?.CardType
        };

        // Obtener información del vehículo desde la reserva si está disponible
        if (payment.Reservation != null)
        {
            try
            {
                var vehicle = await _context.Set<Rent2Go.API.Provider.Domain.Model.Vehicle>()
                    .FirstOrDefaultAsync(v => v.Id == payment.Reservation.VehicleId);

                if (vehicle != null)
                {
                    dto.VehicleBrand = vehicle.Brand;
                    dto.VehicleModel = vehicle.Model;
                }

                dto.ReservationStartDate = payment.Reservation.StartDate;
                dto.ReservationEndDate = payment.Reservation.EndDate;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener información del vehículo: {ex.Message}");
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

    private static string GenerateTransactionId()
    {
        return $"TXN_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
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
