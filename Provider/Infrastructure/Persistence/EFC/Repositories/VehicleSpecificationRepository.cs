using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rent2Go.API.Provider.Domain.Model;
using Rent2Go.API.Provider.Domain.Repositories;
using BackendRent2Go.Data;

namespace Rent2Go.API.Provider.Infrastructure.Persistence.EFC.Repositories
{
    public class VehicleSpecificationRepository : IVehicleSpecificationRepository
    {
        private readonly ApplicationDbContext _context;

        public VehicleSpecificationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<VehicleSpecification> FindByVehicleIdAsync(string vehicleId)
        {
            return await _context.Set<VehicleSpecification>()
                .FirstOrDefaultAsync(vs => vs.VehicleId == vehicleId);
        }

        public async Task AddAsync(VehicleSpecification specification)
        {
            await _context.Set<VehicleSpecification>().AddAsync(specification);
        }

        public void Update(VehicleSpecification specification)
        {
            _context.Set<VehicleSpecification>().Update(specification);
        }

        public void Remove(VehicleSpecification specification)
        {
            _context.Set<VehicleSpecification>().Remove(specification);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
