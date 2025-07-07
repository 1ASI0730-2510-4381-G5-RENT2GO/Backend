using BackendRent2Go.Client.Domain.Model.Entities;

namespace BackendRent2Go.Provider.Domain.Repositories;

public interface IProviderReservationRepository
{
    Task<IEnumerable<Reservation>> GetReservationsByProviderIdAsync(string providerId);
    Task<Reservation?> FindByIdAsync(string reservationId);
    Task<Reservation> UpdateReservationAsync(Reservation reservation);
    Task<IEnumerable<Reservation>> GetReservationsByStatusAsync(string providerId, string status);
    Task<IEnumerable<Reservation>> GetReservationsByVehicleIdAsync(string vehicleId);
    Task<IEnumerable<Reservation>> GetReservationsByDateRangeAsync(string providerId, DateTime startDate, DateTime endDate);
}
