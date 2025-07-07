using System;
using System.ComponentModel.DataAnnotations;

namespace Rent2Go.API.IAM.Interfaces.REST.Resources
{
    // DTO para el registro de usuarios
    public class SignUpResource
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
        // Licencia de conducir (opcional para client y provider)
        public string? DriverLicense { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
    }

    // DTO para el inicio de sesión
    public class SignInResource
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
