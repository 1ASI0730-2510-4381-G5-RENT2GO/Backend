// filepath: C:\Users\GIGABYTE\RiderProjects\BackendRent2Go\IAM\Domain\Services\IUserCommandService.cs
using Rent2Go.API.IAM.Domain.Model.Commands;
using System.Threading.Tasks;

namespace Rent2Go.API.IAM.Domain.Services
{
    public interface IUserCommandService
    {
        Task<string> Handle(SignUpCommand command);
        Task<string> Handle(SignInCommand command);
    }
}
