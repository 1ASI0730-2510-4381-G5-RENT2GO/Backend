using BCrypt.Net;

namespace Rent2Go.API.IAM.Infrastructure.Hashing.BCrypt.Services
{
    public class PasswordHashingService : IPasswordHashingService
    {
        public string HashPassword(string password)
        {
            // Usamos la forma completa para resolver posibles ambigüedades
            return global::BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string hash)
        {
            // Usamos la forma completa para resolver posibles ambigüedades
            return global::BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }

    public interface IPasswordHashingService
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }
}
