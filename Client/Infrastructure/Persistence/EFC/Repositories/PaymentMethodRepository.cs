using BackendRent2Go.Client.Domain.Model.Entities;
using BackendRent2Go.Client.Domain.Repositories;
using BackendRent2Go.Data;
using Microsoft.EntityFrameworkCore;

namespace BackendRent2Go.Client.Infrastructure.Persistence.EFC.Repositories;

public class PaymentMethodRepository : IPaymentMethodRepository
{
    private readonly ApplicationDbContext _context;

    public PaymentMethodRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PaymentMethod>> GetPaymentMethodsByUserIdAsync(string userId)
    {
        return await _context.Set<PaymentMethod>()
            .Where(pm => pm.UserId == userId)
            .OrderByDescending(pm => pm.IsDefault)
            .ThenByDescending(pm => pm.CreatedAt)
            .ToListAsync();
    }

    public async Task<PaymentMethod?> GetPaymentMethodByIdAsync(string paymentMethodId)
    {
        return await _context.Set<PaymentMethod>()
            .FirstOrDefaultAsync(pm => pm.Id == paymentMethodId);
    }

    public async Task<PaymentMethod> CreatePaymentMethodAsync(PaymentMethod paymentMethod)
    {
        // Si es el primer método de pago del usuario, marcarlo como predeterminado
        var existingMethods = await GetPaymentMethodsByUserIdAsync(paymentMethod.UserId);
        if (!existingMethods.Any())
        {
            paymentMethod.IsDefault = true;
        }

        // Si se está marcando como predeterminado, desmarcar los otros
        if (paymentMethod.IsDefault)
        {
            await UnsetDefaultForUserAsync(paymentMethod.UserId);
        }

        paymentMethod.Id = Guid.NewGuid().ToString();
        paymentMethod.CreatedAt = DateTime.UtcNow;
        paymentMethod.UpdatedAt = DateTime.UtcNow;

        _context.Set<PaymentMethod>().Add(paymentMethod);
        await _context.SaveChangesAsync();

        return paymentMethod;
    }

    public async Task<PaymentMethod> UpdatePaymentMethodAsync(PaymentMethod paymentMethod)
    {
        paymentMethod.UpdatedAt = DateTime.UtcNow;

        // Si se está marcando como predeterminado, desmarcar los otros
        if (paymentMethod.IsDefault)
        {
            await UnsetDefaultForUserAsync(paymentMethod.UserId);
        }

        _context.Set<PaymentMethod>().Update(paymentMethod);
        await _context.SaveChangesAsync();

        return paymentMethod;
    }

    public async Task<bool> DeletePaymentMethodAsync(string paymentMethodId)
    {
        var paymentMethod = await GetPaymentMethodByIdAsync(paymentMethodId);
        if (paymentMethod == null)
            return false;

        _context.Set<PaymentMethod>().Remove(paymentMethod);
        await _context.SaveChangesAsync();

        // Si era el método predeterminado, asignar otro como predeterminado
        if (paymentMethod.IsDefault)
        {
            var firstMethod = await _context.Set<PaymentMethod>()
                .Where(pm => pm.UserId == paymentMethod.UserId)
                .FirstOrDefaultAsync();

            if (firstMethod != null)
            {
                firstMethod.IsDefault = true;
                firstMethod.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        return true;
    }

    public async Task<bool> SetDefaultPaymentMethodAsync(string userId, string paymentMethodId)
    {
        var paymentMethod = await GetPaymentMethodByIdAsync(paymentMethodId);
        if (paymentMethod == null || paymentMethod.UserId != userId)
            return false;

        // Desmarcar otros métodos como predeterminado
        await UnsetDefaultForUserAsync(userId);

        // Marcar este método como predeterminado
        paymentMethod.IsDefault = true;
        paymentMethod.UpdatedAt = DateTime.UtcNow;

        _context.Set<PaymentMethod>().Update(paymentMethod);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<PaymentMethod?> GetDefaultPaymentMethodAsync(string userId)
    {
        return await _context.Set<PaymentMethod>()
            .FirstOrDefaultAsync(pm => pm.UserId == userId && pm.IsDefault);
    }

    public async Task UnsetDefaultForUserAsync(string userId)
    {
        var defaultMethods = await _context.Set<PaymentMethod>()
            .Where(pm => pm.UserId == userId && pm.IsDefault)
            .ToListAsync();

        foreach (var method in defaultMethods)
        {
            method.IsDefault = false;
            method.UpdatedAt = DateTime.UtcNow;
        }

        if (defaultMethods.Any())
        {
            await _context.SaveChangesAsync();
        }
    }

    public async Task AddAsync(PaymentMethod paymentMethod)
    {
        paymentMethod.Id = Guid.NewGuid().ToString();
        paymentMethod.CreatedAt = DateTime.UtcNow;
        paymentMethod.UpdatedAt = DateTime.UtcNow;

        _context.Set<PaymentMethod>().Add(paymentMethod);
        await _context.SaveChangesAsync();
    }
}
