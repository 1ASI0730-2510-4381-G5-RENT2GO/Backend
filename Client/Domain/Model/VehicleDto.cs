using System.ComponentModel.DataAnnotations;

namespace BackendRent2Go.Client.Domain.Model;

public class VehicleDto
{
    public string Id { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; } // Precio diario
    public int Seats { get; set; }
    public int Luggage { get; set; }
    public string Transmission { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    public string Status { get; set; } = "available";
    public List<string> Images { get; set; } = new List<string>();
}

