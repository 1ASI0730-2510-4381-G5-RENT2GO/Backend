using System;

namespace Rent2Go.API.Provider.Domain.Model
{
    public class VehicleDto
    {
        public string Id { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string Type { get; set; }
        public decimal DailyRate { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
        public int Doors { get; set; }
        public int Seats { get; set; }
        public string Transmission { get; set; }
        public string FuelType { get; set; }
        public bool AirConditioner { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateVehicleRequest
    {
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string Type { get; set; }
        public decimal DailyRate { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public int Doors { get; set; }
        public int Seats { get; set; }
        public string Transmission { get; set; }
        public string FuelType { get; set; }
        public bool AirConditioner { get; set; }
    }

    public class UpdateVehicleRequest
    {
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string Type { get; set; }
        public decimal DailyRate { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public int Doors { get; set; }
        public int Seats { get; set; }
        public string Transmission { get; set; }
        public string FuelType { get; set; }
        public bool AirConditioner { get; set; }
    }
}
