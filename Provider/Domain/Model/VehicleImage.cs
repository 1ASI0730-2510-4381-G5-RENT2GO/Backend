using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rent2Go.API.Provider.Domain.Model
{
    [Table("vehicle_images")]
    public class VehicleImage
    {
        [Key]
        [Column("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        [Column("vehicle_id")]
        public required string VehicleId { get; set; }
        
        [Required]
        [Column("image_url")]
        public required string ImageUrl { get; set; }
        
        [Column("is_primary")]
        public bool IsPrimary { get; set; }
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Propiedad de navegación para relacionar con el vehículo
        [ForeignKey("VehicleId")]
        public Vehicle? Vehicle { get; set; }
    }
}
