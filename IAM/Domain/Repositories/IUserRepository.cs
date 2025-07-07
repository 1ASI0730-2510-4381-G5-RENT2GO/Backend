// filepath: C:\Users\GIGABYTE\RiderProjects\BackendRent2Go\IAM\Domain\Repositories\IUserRepository.cs
using Rent2Go.API.IAM.Domain.Model.Aggregates;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rent2Go.API.IAM.Domain.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> ListAsync();
        Task<User> FindByIdAsync(string id);
        Task<User?> FindByEmailAsync(string email);
        Task AddAsync(User user);
        void Update(User user);
        void Remove(User user);
    }
}
