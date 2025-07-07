using System.Threading.Tasks;
using Rent2Go.API.Provider.Domain.Model;

namespace Rent2Go.API.Provider.Domain.Repositories
{
    public interface IVehicleSpecificationRepository
    {
        Task<VehicleSpecification> FindByVehicleIdAsync(string vehicleId);
        Task AddAsync(VehicleSpecification specification);
        void Update(VehicleSpecification specification);
        void Remove(VehicleSpecification specification);
        Task<int> SaveChangesAsync();
    }
}
