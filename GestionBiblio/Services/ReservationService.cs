using GestionBiblio.Data;
using GestionBiblio.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GestionBiblio.Services
{
    public class ReservationService : IReservationService
    {
        private readonly LibraryContext _context;

        public ReservationService(LibraryContext context)
        {
            _context = context;
        }

        public async Task<List<Reservation>> GetAllAsync()
        {
            return await _context.Reservations
                .Include(r => r.Livre)
                .Include(r => r.Membre)
                .ToListAsync();
        }

        public async Task<Reservation> GetByIdAsync(int id)
        {
            return await _context.Reservations
                .Include(r => r.Livre)
                .Include(r => r.Membre)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task CreateAsync(Reservation reservation)
        {
            var livre = await _context.Livres.FindAsync(reservation.LivreId);
            if (livre != null && livre.NombreExemplaires > 0)
            {
                throw new System.Exception("Le livre est disponible et ne peut pas être réservé.");
            }
            _context.Add(reservation);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Reservation reservation)
        {
            _context.Update(reservation);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation != null)
            {
                _context.Reservations.Remove(reservation);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Reservation>> GetReservationsByMemberIdAsync(int memberId)
        {
            return await _context.Reservations
                .Include(r => r.Livre)
                .Include(r => r.Membre)
                .Where(r => r.MembreId == memberId)
                .ToListAsync();
        }

        public bool ReservationExists(int id)
        {
            return _context.Reservations.Any(e => e.Id == id);
        }
    }
}
