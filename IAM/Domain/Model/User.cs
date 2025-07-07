using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rent2Go.API.IAM.Domain.Model
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("id")]
        public string Id { get; set; } = string.Empty;
        
        [Required]
        [Column("name")]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [Column("email")]
        public string Email { get; set; } = string.Empty;
        
        // La contraseña puede ser nula para usuarios autenticados con proveedores externos
        [Column("password")]
        public string? Password { get; set; } // Permitir null para usuarios OAuth
        
        [Required]
        [Column("role")]
        public string Role { get; set; } = string.Empty;
        
        [Column("status")]
        public string Status { get; set; } = "pending";
        
        [Column("profile_image")]
        public string? ProfileImage { get; set; } // Permitir null
        
        // Propiedades OAuth según el esquema - permitir valores NULL
        [Column("oauth_provider")]
        public string? OAuthProvider { get; set; } // Permitir null
        
        [Column("oauth_id")]
        public string? OAuthId { get; set; } // Permitir null
        
        [Column("is_oauth_user")]
        public bool IsOAuthUser { get; set; } = false;
        
        [Column("email_verified")]
        public bool EmailVerified { get; set; } = false;
        
        [Column("registration_date")]
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        
        // Resto de propiedades de User...
    }
}
