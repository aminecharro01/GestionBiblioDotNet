using GestionBiblio.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GestionBiblio.Services
{
    public interface IReservationService
    {
        Task<List<Reservation>> GetAllAsync();
        Task<Reservation> GetByIdAsync(int id);
        Task<List<Reservation>> GetReservationsByMemberIdAsync(int memberId);
        Task CreateAsync(Reservation reservation);
        Task UpdateAsync(Reservation reservation);
        Task DeleteAsync(int id);
        bool ReservationExists(int id);
    }
}
