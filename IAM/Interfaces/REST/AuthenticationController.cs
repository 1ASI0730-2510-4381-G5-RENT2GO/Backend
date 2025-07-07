using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rent2Go.API.IAM.Domain.Repositories;
using Rent2Go.API.IAM.Domain.Model;
using Rent2Go.API.Shared.Domain.Services;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using UserAggregate = Rent2Go.API.IAM.Domain.Model.Aggregates.User;
using IAMClient = Rent2Go.API.IAM.Domain.Model.Client;
using IAMProvider = Rent2Go.API.IAM.Domain.Model.Provider;

namespace BackendRent2Go.IAM.Interfaces.REST
{
    public class UserDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        // Ruta de imagen de perfil
        public string? ProfileImage { get; set; }
    }

    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class RegisterDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string Role { get; set; }
        public string BusinessName { get; set; }
        public string TaxId { get; set; }
        public string Phone { get; set; }
        public string Dni { get; set; }
    }

    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IProviderRepository _providerRepository;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(
            IUserRepository userRepository,
            IClientRepository clientRepository,
            IProviderRepository providerRepository,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<AuthenticationController> logger = null)
        {
            _userRepository = userRepository;
            _clientRepository = clientRepository;
            _providerRepository = providerRepository;
            _emailService = emailService;
            _config = configuration;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await UserExists(registerDto.Email))
                return BadRequest("El email ya está registrado");

            if (string.IsNullOrEmpty(registerDto.Name) || string.IsNullOrEmpty(registerDto.Email) || string.IsNullOrEmpty(registerDto.Password))
            {
                return BadRequest("Los campos nombre, email y contraseña son obligatorios");
            }

            if (registerDto.Password != registerDto.ConfirmPassword)
            {
                return BadRequest("Las contraseñas no coinciden");
            }

            var user = new UserAggregate
            {
                Id = Guid.NewGuid().ToString(),
                Name = registerDto.Name,
                Email = registerDto.Email.ToLower(),
                Password = HashPassword(registerDto.Password),
                Role = registerDto.Role ?? "client",
                Status = "pending",
                RegistrationDate = DateTime.UtcNow,
                OAuthProvider = null,
                OAuthId = null,
                IsOAuthUser = false,
                EmailVerified = false
            };

            await _userRepository.AddAsync(user);

            if (user.Role == "provider")
            {
                var provider = new IAMProvider
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = user.Id,
                    BusinessName = registerDto.BusinessName ?? "",
                    TaxId = registerDto.TaxId ?? "",
                    Phone = registerDto.Phone ?? "",
                    Status = "pending",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _providerRepository.AddAsync(provider);
            }
            else if (user.Role == "client")
            {
                var client = new IAMClient
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = user.Id,
                    Dni = registerDto.Dni ?? "",
                    Phone = registerDto.Phone ?? "",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _clientRepository.AddAsync(client);
            }

            try
            {
                await _emailService.SendWelcomeEmailAsync(user.Email, user.Name);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar correo de bienvenida: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
            }

            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<object>> Login(LoginDto loginDto)
        {
            try
            {
                _logger?.LogInformation($"Intento de login para email: {loginDto.Email}");
                
                if (string.IsNullOrEmpty(loginDto.Email) || string.IsNullOrEmpty(loginDto.Password))
                {
                    _logger?.LogWarning("Intento de login con credenciales incompletas");
                    return BadRequest("Email y contraseña son requeridos");
                }

                _logger?.LogInformation($"Buscando usuario por email: {loginDto.Email.ToLower()}");
                var user = await _userRepository.FindByEmailAsync(loginDto.Email.ToLower());
                
                if (user == null)
                {
                    _logger?.LogWarning($"Usuario no encontrado para email: {loginDto.Email.ToLower()}");
                    return Unauthorized("Credenciales inválidas");
                }
                
                _logger?.LogInformation($"Usuario encontrado: {user.Id}, verificando contraseña");
                _logger?.LogDebug($"Hash almacenado: {user.Password}");
                
                try
                {
                    if (!VerifyPasswordSafe(loginDto.Password, user.Password))
                    {
                        _logger?.LogWarning($"Contraseña incorrecta para usuario {user.Id}");
                        return Unauthorized("Credenciales inválidas");
                    }
                }
                catch (Exception pwdEx)
                {
                    _logger?.LogError($"Error al verificar contraseña: {pwdEx.Message}");
                    return StatusCode(500, "Error al verificar credenciales");
                }

                _logger?.LogInformation($"Contraseña correcta para usuario {user.Id}, generando token");

                string token;
                try
                {
                    token = GenerateJwtTokenSafe(user);
                }
                catch (Exception tokenEx)
                {
                    _logger?.LogError($"Error al generar token JWT: {tokenEx.Message}");
                    return StatusCode(500, "Error al generar token de acceso");
                }
                
                _logger?.LogInformation($"Login exitoso para usuario {user.Id}");
                
                return Ok(new
                {
                    token,
                    user = new UserDto
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Email = user.Email,
                        Role = user.Role,
                        ProfileImage = user.ProfileImage
                    }
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error inesperado en login: {ex.Message}");
                _logger?.LogError($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<object>> Me()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var user = await _userRepository.FindByIdAsync(userId);
            if (user == null) return NotFound();
            return Ok(new
            {
                user = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role,
                    ProfileImage = user.ProfileImage
                }
            });
        }

        private async Task<bool> UserExists(string email)
        {
            var user = await _userRepository.FindByEmailAsync(email.ToLower());
            return user != null;
        }

        private string HashPassword(string password)
        {
            // Usar BCrypt para hashear la contraseña de manera segura
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPasswordSafe(string inputPassword, string storedPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(storedPassword))
                {
                    _logger?.LogWarning("El hash almacenado está vacío");
                    return false;
                }
                // Si es hash de BCrypt
                if (storedPassword.StartsWith("$2"))
                {
                    return BCrypt.Net.BCrypt.Verify(inputPassword, storedPassword);
                }
                // Fallback para esquemas antiguos Base64
                try
                {
                    var decodedBytes = Convert.FromBase64String(storedPassword);
                    var decodedHash = Encoding.UTF8.GetString(decodedBytes);
                    return inputPassword.Equals(decodedHash);
                }
                catch (FormatException)
                {
                    _logger?.LogWarning("StoredPassword no es Base64 válido ni BCrypt");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error en VerifyPasswordSafe: {ex.Message}");
                throw;
            }
        }

        private string GenerateJwtTokenSafe(UserAggregate user)
        {
            try
            {
                var secretKey = _config["TokenKey"];
                if (string.IsNullOrEmpty(secretKey))
                {
                    _logger?.LogWarning("TokenKey no encontrada en configuración, usando valor por defecto");
                    secretKey = "defaultSecretKeyForDevelopment12345678";
                }
                
                var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
                var creds = new Microsoft.IdentityModel.Tokens.SigningCredentials(key, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha512Signature);
                
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                };
                
                var tokenDescriptor = new JwtSecurityToken(
                    expires: DateTime.UtcNow.AddDays(7),
                    signingCredentials: creds,
                    claims: claims
                );
                
                return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error en GenerateJwtTokenSafe: {ex.Message}");
                throw;
            }
        }
    }
}
