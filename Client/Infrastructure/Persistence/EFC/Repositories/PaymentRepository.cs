using BackendRent2Go.Client.Domain.Model.Entities;
using BackendRent2Go.Client.Domain.Repositories;
using BackendRent2Go.Data;
using Microsoft.EntityFrameworkCore;

namespace BackendRent2Go.Client.Infrastructure.Persistence.EFC.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly ApplicationDbContext _context;

    public PaymentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Payment>> GetPaymentsByUserIdAsync(string userId)
    {
        // Primero obtenemos el clientId del userId
        var client = await _context.Clients.FirstOrDefaultAsync(c => c.UserId == userId);
        if (client == null) return new List<Payment>();

        return await _context.Set<Payment>()
            .Include(p => p.Reservation)
            .Include(p => p.PaymentMethod)
            .Where(p => p.Reservation!.ClientId == client.Id)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Payment?> GetPaymentByIdAsync(string paymentId)
    {
        return await _context.Set<Payment>()
            .Include(p => p.Reservation)
            .Include(p => p.PaymentMethod)
            .FirstOrDefaultAsync(p => p.Id == paymentId);
    }

    public async Task<Payment?> GetPaymentByReservationIdAsync(string reservationId)
    {
        return await _context.Set<Payment>()
            .Include(p => p.Reservation)
            .Include(p => p.PaymentMethod)
            .FirstOrDefaultAsync(p => p.ReservationId == reservationId);
    }

    public async Task<Payment> CreatePaymentAsync(Payment payment)
    {
        payment.Id = Guid.NewGuid().ToString();
        payment.CreatedAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;

        // Generar un ID de transacción único si no se proporciona
        if (string.IsNullOrEmpty(payment.TransactionId))
        {
            payment.TransactionId = $"TXN_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N")[..8]}";
        }

        _context.Set<Payment>().Add(payment);
        await _context.SaveChangesAsync();

        return payment;
    }

    public async Task<Payment> UpdatePaymentAsync(Payment payment)
    {
        payment.UpdatedAt = DateTime.UtcNow;

        _context.Set<Payment>().Update(payment);
        await _context.SaveChangesAsync();

        return payment;
    }

    public async Task<IEnumerable<Payment>> GetPaymentHistoryByUserIdAsync(string userId, int page = 1, int pageSize = 10)
    {
        var skip = (page - 1) * pageSize;

        // Primero obtenemos el clientId del userId
        var client = await _context.Clients.FirstOrDefaultAsync(c => c.UserId == userId);
        if (client == null) return new List<Payment>();

        return await _context.Set<Payment>()
            .Include(p => p.Reservation)
            .Include(p => p.PaymentMethod)
            .Where(p => p.Reservation!.ClientId == client.Id)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();
    }
}
