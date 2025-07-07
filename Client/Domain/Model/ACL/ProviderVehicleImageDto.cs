namespace BackendRent2Go.Client.Domain.Model.ACL
{
    /// <summary>
    /// DTO para representar una imagen de vehículo del Provider en el contexto de Client
    /// </summary>
    public class ProviderVehicleImageDto
    {
        public string Id { get; set; }
        public string VehicleId { get; set; }
        public string ImageUrl { get; set; }
    }
}

