// filepath: C:\Users\GIGABYTE\RiderProjects\BackendRent2Go\IAM\Domain\Services\IUserQueryService.cs
using Rent2Go.API.IAM.Domain.Model.Aggregates;
using Rent2Go.API.IAM.Domain.Model.Queries;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rent2Go.API.IAM.Domain.Services
{
    public interface IUserQueryService
    {
        Task<IEnumerable<User>> Handle(GetAllUsersQuery query);
        Task<User> Handle(GetUserByIdQuery query);
        Task<User> Handle(GetUserByUsernameQuery query);
    }
}
