using System;
using System.Threading.Tasks;
using Rent2Go.API.IAM.Domain.Model.Aggregates;
using Rent2Go.API.IAM.Domain.Model.Commands;
using Rent2Go.API.IAM.Domain.Repositories;
using Rent2Go.API.IAM.Domain.Services;

namespace Rent2Go.API.IAM.Application.Internal.CommandServices
{
    public class UserCommandService : IUserCommandService
    {
        private readonly IUserRepository _userRepository;
        
        public UserCommandService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<string> Handle(SignUpCommand command)
        {
            // Validar si el usuario ya existe
            var existingUser = await _userRepository.FindByEmailAsync(command.Email);
            if (existingUser != null)
                throw new Exception("Ya existe un usuario con este email.");

            // Crear nuevo usuario
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Name = command.Name,
                Email = command.Email,
                Password = command.Password, // Nota: implementar hash de contraseña más adelante
                Role = command.Role,
                Status = "active",
                EmailVerified = false,
                RegistrationDate = DateTime.UtcNow
            };

            // Persistir el usuario
            await _userRepository.AddAsync(user);
            
            // Generar token (implementación temporal)
            return "token-provisional";
        }

        public async Task<string> Handle(SignInCommand command)
        {
            // Buscar el usuario por email
            var user = await _userRepository.FindByEmailAsync(command.Email);
            if (user == null)
                throw new Exception("Usuario no encontrado.");

            // Verificar contraseña (implementación temporal sin hash)
            if (user.Password != command.Password)
                throw new Exception("Contraseña incorrecta.");

            // Generar token (implementación temporal)
            return "token-provisional";
        }
    }
}
