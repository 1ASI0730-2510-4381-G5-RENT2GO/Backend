using Microsoft.EntityFrameworkCore;
using BackendRent2Go.Data;
using Rent2Go.API.Provider.Domain.Model;
using Rent2Go.API.Provider.Domain.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rent2Go.API.Provider.Infrastructure.Persistence.EFC.Repositories
{
    public class EFCVehicleImageRepository : IVehicleImageRepository
    {
        private readonly ApplicationDbContext _context;

        public EFCVehicleImageRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<VehicleImage>> GetByVehicleIdAsync(string vehicleId)
        {
            return await _context.Set<VehicleImage>()
                .Where(i => i.VehicleId == vehicleId)
                .OrderByDescending(i => i.IsPrimary)
                .ThenByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task<VehicleImage?> FindByIdAsync(string id)
        {
            return await _context.Set<VehicleImage>().FindAsync(id);
        }

        public async Task<VehicleImage> AddAsync(VehicleImage vehicleImage)
        {
            await _context.Set<VehicleImage>().AddAsync(vehicleImage);
            return vehicleImage;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var vehicleImage = await FindByIdAsync(id);
            if (vehicleImage == null)
                return false;

            _context.Set<VehicleImage>().Remove(vehicleImage);
            return true;
        }

        public async Task<bool> DeleteAllByVehicleIdAsync(string vehicleId)
        {
            var vehicleImages = await _context.Set<VehicleImage>()
                .Where(i => i.VehicleId == vehicleId)
                .ToListAsync();

            if (!vehicleImages.Any())
                return false;

            _context.Set<VehicleImage>().RemoveRange(vehicleImages);
            return true;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
