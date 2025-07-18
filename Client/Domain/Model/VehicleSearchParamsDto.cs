using System;

namespace BackendRent2Go.Client.Domain.Model
{
    /// <summary>
    /// Par metros de b squeda para veh culos
    /// </summary>
    public class VehicleSearchParamsDto
    {
        public string? Search { get; set; }
        public string? Category { get; set; }
        public string? Brand { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Transmission { get; set; }
        public DateTime? PickupDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public int? MinSeats { get; set; }
        public int? MinLuggage { get; set; }
        public int? MinYear { get; set; }
        public decimal? MinRating { get; set; }
    }
}
