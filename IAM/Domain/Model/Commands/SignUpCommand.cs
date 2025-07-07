// filepath: C:\Users\GIGABYTE\RiderProjects\BackendRent2Go\IAM\Domain\Model\Commands\SignUpCommand.cs
namespace Rent2Go.API.IAM.Domain.Model.Commands
{
    public class SignUpCommand
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}

