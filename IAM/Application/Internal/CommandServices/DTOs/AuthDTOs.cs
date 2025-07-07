using System;
using System.ComponentModel.DataAnnotations;

namespace Rent2Go.API.IAM.Application.Internal.CommandServices.DTOs
{
    // DTO para el registro de usuarios
    public class RegisterDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // Rol opcional (por defecto "client")
        public string Role { get; set; } = "client";

        // Campos adicionales para proveedores
        public string BusinessName { get; set; } = string.Empty;
        public string TaxId { get; set; } = string.Empty;
        
        // Campos adicionales para clientes
        public string Dni { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }

    // DTO para el inicio de sesión
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    // DTO para la respuesta de usuario
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }

    // DTO para solicitud de recuperación de contraseña
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    // DTO para verificación de código
    public class VerifyCodeDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Code { get; set; } = string.Empty;
    }

    // DTO para cambio de contraseña
    public class ResetPasswordDto
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    // DTO para autenticación externa (Google, Facebook, etc.)
    public class ExternalAuthDto
    {
        [Required]
        public string Provider { get; set; } = string.Empty; // "google", "facebook", etc.
        
        [Required]
        public string Id { get; set; } = string.Empty; // ID del usuario en el proveedor
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string ProfileImage { get; set; } = string.Empty;
    }
}
