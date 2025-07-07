using System.Collections.Generic;
using System.Threading.Tasks;
using Rent2Go.API.Provider.Domain.Model;

namespace Rent2Go.API.Provider.Domain.Repositories
{
    public interface IVehicleImageRepository
    {
        Task<IEnumerable<VehicleImage>> GetByVehicleIdAsync(string vehicleId);
        Task<VehicleImage?> FindByIdAsync(string id);
        Task<VehicleImage> AddAsync(VehicleImage vehicleImage);
        Task<bool> DeleteAsync(string id);
        Task<bool> DeleteAllByVehicleIdAsync(string vehicleId);
        Task<int> SaveChangesAsync();
    }
}
