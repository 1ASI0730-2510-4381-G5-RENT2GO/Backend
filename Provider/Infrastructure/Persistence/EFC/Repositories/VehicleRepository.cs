using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rent2Go.API.Provider.Domain.Model;
using Rent2Go.API.Provider.Domain.Repositories;
using BackendRent2Go.Data;

namespace Rent2Go.API.Provider.Infrastructure.Persistence.EFC.Repositories
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly ApplicationDbContext _context;

        public VehicleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Vehicle vehicle)
        {
            await _context.Set<Vehicle>().AddAsync(vehicle);
        }

        public async Task<IEnumerable<Vehicle>> ListByOwnerAsync(string ownerId)
        {
            var query = _context.Set<Vehicle>().AsNoTracking();
            
            // Si ownerId es null, devuelve todos los vehículos
            if (ownerId != null)
            {
                query = query.Where(v => v.OwnerId == ownerId);
            }
            
            return await query.ToListAsync();
        }

        public async Task<Vehicle?> FindByIdAsync(string id)
        {
            return await _context.Set<Vehicle>()
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public void Update(Vehicle vehicle)
        {
            _context.Set<Vehicle>().Update(vehicle);
        }

        public void Remove(Vehicle vehicle)
        {
            _context.Set<Vehicle>().Remove(vehicle);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
