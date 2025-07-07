namespace BackendRent2Go.Client.Domain.Model.ACL
{
    /// <summary>
    /// DTO para representar un vehículo del Provider en el contexto de Client
    /// </summary>
    public class ProviderVehicleDto
    {
        public string Id { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string Type { get; set; }
        public decimal DailyRate { get; set; }
        public int Seats { get; set; }
        public int? LuggageCapacity { get; set; }
        public string Transmission { get; set; }
        public decimal? Rating { get; set; } // Cambiado de double? a decimal? para coincidir con VehicleDto
        public int? ReviewCount { get; set; }
        public string Status { get; set; }
    }
}

