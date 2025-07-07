using System.Threading.Tasks;
using System.Collections.Generic;
using Rent2Go.API.IAM.Domain.Model;

namespace Rent2Go.API.IAM.Domain.Repositories
{
    public interface IClientRepository
    {
        Task<Client?> GetByIdAsync(string id);
        Task<Client?> GetByUserIdAsync(string userId);
        Task<IEnumerable<Client>> GetAllAsync();
        Task<Client> AddAsync(Client client);
        Task UpdateAsync(Client client);
        Task DeleteAsync(string id);
    }
}
