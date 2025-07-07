using BackendRent2Go.Client.Domain.Model.Entities;

namespace BackendRent2Go.Client.Domain.Repositories;

public interface IReservationRepository
{
    Task<IEnumerable<Reservation>> GetReservationsByUserIdAsync(string userId);
    Task<Reservation?> FindByIdAsync(string reservationId);
    Task<Reservation> CreateReservationAsync(Reservation reservation);
    Task<Reservation> UpdateReservationAsync(Reservation reservation);
    Task<bool> DeleteReservationAsync(string reservationId);
    Task<IEnumerable<Reservation>> GetReservationsByStatusAsync(string userId, string status);
}
