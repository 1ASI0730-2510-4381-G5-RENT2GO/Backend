using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rent2Go.API.IAM.Domain.Repositories
{
    using Model;

    public interface IProviderRepository
    {
        Task<Provider?> GetByIdAsync(string id);
        Task<Provider?> GetByUserIdAsync(string userId);
        Task<IEnumerable<Provider>> GetAllAsync();
        Task<Provider> AddAsync(Provider provider);
        Task UpdateAsync(Provider provider);
        Task DeleteAsync(string id);
    }
}
