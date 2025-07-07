using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rent2Go.API.IAM.Domain.Model
{
    public class Provider
    {
        [Key]
        [Column("id")]
        public string Id { get; set; } = string.Empty;
        
        [Required]
        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;
        
        [Column("business_name")]
        public string BusinessName { get; set; } = string.Empty;
        
        [Column("tax_id")]
        public string TaxId { get; set; } = string.Empty;
        
        [Column("phone")]
        public string Phone { get; set; } = string.Empty;
        
        [Column("status")]
        public string Status { get; set; } = "pending";
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Relación con User
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
