namespace Rent2Go.API.Provider.Domain.Repositories
{
    using Model;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IVehicleRepository
    {
        Task AddAsync(Vehicle vehicle);
        Task<IEnumerable<Vehicle>> ListByOwnerAsync(string ownerId);
        Task<Vehicle?> FindByIdAsync(string id);
        void Update(Vehicle vehicle);
        void Remove(Vehicle vehicle);
        Task<int> SaveChangesAsync();
    }
}
