using System.Threading.Tasks;

namespace Rent2Go.API.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string content);
        Task SendPasswordResetEmailAsync(string to, string resetCode);
        Task SendWelcomeEmailAsync(string to, string name);
        Task SendVerificationEmailAsync(string to, string name, string verificationToken);
        Task SendVerificationCodeEmailAsync(string to, string name, string verificationCode);
    }
}

