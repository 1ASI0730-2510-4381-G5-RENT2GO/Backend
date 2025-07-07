using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rent2Go.API.Provider.Domain.Model
{
    [Table("vehicles")]
    public class Vehicle
    {
        [Key]
        [Column("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Column("provider_id")]
        public string OwnerId { get; set; }

        [Required]
        [Column("brand")]
        public string Brand { get; set; }

        [Required]
        [Column("model")]
        public string Model { get; set; }

        [Required]
        [Column("year")]
        public int Year { get; set; }

        [Required]
        [Column("type")]
        public string Type { get; set; }

        [Required]
        [Column("daily_rate")]
        public decimal DailyRate { get; set; }

        [Required]
        [Column("description")]
        public string Description { get; set; }

        [Required]
        [Column("location")]
        public string Location { get; set; }

        [Required]
        [Column("status")]
        public string Status { get; set; } = "available";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Propiedades de navegación para relacionar con las especificaciones
        [NotMapped]
        public VehicleSpecification Specifications { get; set; }
        
        // Propiedades para imágenes del vehículo
        [NotMapped]
        public List<string> Images { get; set; } = new List<string>();
        
        // Propiedades mapeadas temporalmente para compatibilidad con el frontend
        [NotMapped]
        public int Doors { get; set; }
        
        [NotMapped]
        public int Seats { get; set; }
        
        [NotMapped]
        public string Transmission { get; set; }
        
        [NotMapped]
        public string FuelType { get; set; }
        
        [NotMapped]
        public bool AirConditioner { get; set; }
    }
    
    [Table("vehicle_specifications")]
    public class VehicleSpecification
    {
        [Key]
        [Column("vehicle_id")]
        public string VehicleId { get; set; }
        
        [Column("doors")]
        public int Doors { get; set; } = 4;
        
        [Column("seats")]
        public int Seats { get; set; } = 5;
        
        [Column("transmission")]
        public string Transmission { get; set; } = "manual";
        
        [Column("fuel_type")]
        public string FuelType { get; set; } = "gasoline";
        
        [Column("air_conditioner")]
        public bool AirConditioner { get; set; } = true;
        
        // Propiedad de navegación para relacionar con el vehículo
        [ForeignKey("VehicleId")]
        public Vehicle Vehicle { get; set; }
    }
}
