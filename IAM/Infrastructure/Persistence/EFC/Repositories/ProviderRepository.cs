using Microsoft.EntityFrameworkCore;
using BackendRent2Go.Data;
using Rent2Go.API.IAM.Domain.Repositories;

namespace Rent2Go.API.IAM.Infrastructure.Persistence.EFC.Repositories
{
    public class ProviderRepository : IProviderRepository
    {
        private readonly ApplicationDbContext _context;

        public ProviderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Rent2Go.API.IAM.Domain.Model.Provider?> GetByIdAsync(string id)
        {
            var provider = await _context.Providers
                .FirstOrDefaultAsync(p => p.Id == id);
            return provider;
        }

        public async Task<Rent2Go.API.IAM.Domain.Model.Provider?> GetByUserIdAsync(string userId)
        {
            var provider = await _context.Providers
                .FirstOrDefaultAsync(p => p.UserId == userId);
            return provider;
        }

        public async Task<IEnumerable<Rent2Go.API.IAM.Domain.Model.Provider>> GetAllAsync()
        {
            return await _context.Providers.ToListAsync();
        }

        public async Task<Rent2Go.API.IAM.Domain.Model.Provider> AddAsync(Rent2Go.API.IAM.Domain.Model.Provider provider)
        {
            await _context.Providers.AddAsync(provider);
            await _context.SaveChangesAsync();
            return provider;
        }

        public async Task UpdateAsync(Rent2Go.API.IAM.Domain.Model.Provider provider)
        {
            var dbProvider = await _context.Providers
                .FirstOrDefaultAsync(p => p.Id == provider.Id);
            
            if (dbProvider != null)
            {
                // Actualizar las propiedades manualmente
                dbProvider.UserId = provider.UserId;
                dbProvider.BusinessName = provider.BusinessName;
                dbProvider.TaxId = provider.TaxId;
                dbProvider.Phone = provider.Phone;
                dbProvider.Status = provider.Status;
                dbProvider.UpdatedAt = provider.UpdatedAt;
                
                _context.Providers.Update(dbProvider);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(string id)
        {
            var provider = await _context.Providers
                .FirstOrDefaultAsync(p => p.Id == id);
            if (provider != null)
            {
                _context.Providers.Remove(provider);
                await _context.SaveChangesAsync();
            }
        }
    }
}
