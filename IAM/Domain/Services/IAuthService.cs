using System.Threading.Tasks;
using Rent2Go.API.IAM.Domain.Model;
using Rent2Go.API.IAM.Application.Internal.CommandServices.DTOs;

namespace Rent2Go.API.IAM.Domain.Services
{
    public interface IAuthService
    {
        Task<UserDto> RegisterUserAsync(RegisterDto registerDto);
        Task<UserDto> LoginAsync(LoginDto loginDto);
        Task<UserDto> ExternalLoginAsync(ExternalAuthDto externalAuthDto);
        Task<bool> SendPasswordResetAsync(string email);
        Task<bool> VerifyResetCodeAsync(string email, string code);
        Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
        Task<bool> VerifyEmailAsync(string email, string token);
    }
}
