using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Rent2Go.API.IAM.Domain.Model;

namespace Rent2Go.API.IAM.Infrastructure.Tokens.JWT
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtTokenService> _logger;
        private readonly string _tokenKey;
        private readonly int _tokenExpirationDays;
        private readonly string _issuer;
        private readonly string _audience;

        public JwtTokenService(IConfiguration configuration, ILogger<JwtTokenService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            
            // Obtener clave del token con validación
            _tokenKey = Environment.GetEnvironmentVariable("TOKEN_KEY") ?? 
                       _configuration["TokenKey"] ?? 
                       throw new ArgumentNullException("TokenKey", "TOKEN_KEY is required and cannot be null");

            // Validar longitud mínima de la clave por seguridad
            if (_tokenKey.Length < 32)
            {
                throw new ArgumentException("TOKEN_KEY must be at least 32 characters long for security", nameof(_tokenKey));
            }

            // Configuración de expiración (por defecto 7 días)
            _tokenExpirationDays = int.TryParse(_configuration["JWT:ExpirationDays"], out var days) && days > 0 ? days : 7;
            
            // Configuración opcional de issuer y audience
            _issuer = _configuration["JWT:Issuer"] ?? "Rent2Go.API";
            _audience = _configuration["JWT:Audience"] ?? "Rent2Go.Client";

            _logger.LogInformation("JwtTokenService initialized with {ExpirationDays} days expiration", _tokenExpirationDays);
        }

        public string GenerateToken(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrEmpty(user.Id))
                throw new ArgumentException("User ID cannot be null or empty", nameof(user));

            if (string.IsNullOrEmpty(user.Email))
                throw new ArgumentException("User Email cannot be null or empty", nameof(user));

            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenKey));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

                // Crear claims con validación
                var claims = new List<Claim>
                {
                    // Claims estándar de JWT
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, 
                        new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), 
                        ClaimValueTypes.Integer64),

                    // Claims personalizados usando los nombres correctos
                    new Claim("nameid", user.Id),
                    new Claim("unique_name", user.Name ?? user.Email),
                    new Claim("email", user.Email),
                    new Claim("role", user.Role ?? "user"),
                    
                    // Claims para compatibilidad con ASP.NET Core Identity
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.Name ?? user.Email),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role ?? "user")
                };

                // Configurar fechas en UTC para evitar problemas de zona horaria
                var now = DateTime.UtcNow;
                var expires = now.AddDays(_tokenExpirationDays);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = expires,
                    NotBefore = now,
                    IssuedAt = now,
                    Issuer = _issuer,
                    Audience = _audience,
                    SigningCredentials = credentials
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                _logger.LogInformation("JWT token generated successfully for user {UserId} ({UserEmail}), expires at {ExpirationTime}", 
                    user.Id, user.Email, expires);

                return tokenString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating JWT token for user {UserId} ({UserEmail})", 
                    user.Id, user.Email);
                throw new InvalidOperationException("Failed to generate JWT token", ex);
            }
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("Token validation failed: token is null or empty");
                return null;
            }

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenKey));

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    ClockSkew = TimeSpan.FromMinutes(5),
                    RequireSignedTokens = true
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                
                if (validatedToken is JwtSecurityToken jwtToken)
                {
                    var userId = principal.FindFirst("nameid")?.Value ?? 
                                principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    
                    _logger.LogDebug("JWT token validated successfully for user {UserId}", userId);
                }

                return principal;
            }
            catch (SecurityTokenExpiredException ex)
            {
                _logger.LogWarning("Token validation failed: token has expired. {Error}", ex.Message);
                return null;
            }
            catch (SecurityTokenNotYetValidException ex)
            {
                _logger.LogWarning("Token validation failed: token is not yet valid. {Error}", ex.Message);
                return null;
            }
            catch (SecurityTokenSignatureKeyNotFoundException ex)
            {
                _logger.LogError("Token validation failed: signature key not found. {Error}", ex.Message);
                return null;
            }
            catch (SecurityTokenInvalidSignatureException ex)
            {
                _logger.LogError("Token validation failed: invalid signature. {Error}", ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during token validation");
                return null;
            }
        }

        public TokenInfo GetTokenInfo(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return new TokenInfo { IsValid = false, Error = "Token is null or empty" };

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                
                if (!tokenHandler.CanReadToken(token))
                {
                    return new TokenInfo { IsValid = false, Error = "Token format is invalid" };
                }

                var jwtToken = tokenHandler.ReadJwtToken(token);
                
                return new TokenInfo
                {
                    IsValid = jwtToken.ValidTo > DateTime.UtcNow,
                    ExpiresAt = jwtToken.ValidTo,
                    IssuedAt = jwtToken.IssuedAt,
                    Subject = jwtToken.Subject,
                    Claims = jwtToken.Claims.ToDictionary(c => c.Type, c => c.Value),
                    Error = jwtToken.ValidTo <= DateTime.UtcNow ? "Token has expired" : null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting token info");
                return new TokenInfo { IsValid = false, Error = ex.Message };
            }
        }
    }

    public interface IJwtTokenService
    {
        string GenerateToken(User user);
        ClaimsPrincipal? ValidateToken(string token);
        TokenInfo GetTokenInfo(string token);
    }

    public class TokenInfo
    {
        public bool IsValid { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime IssuedAt { get; set; }
        public string? Subject { get; set; }
        public Dictionary<string, string>? Claims { get; set; }
        public string? Error { get; set; }
    }
}