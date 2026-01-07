using GestionBiblio.Data;
using GestionBiblio.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GestionBiblio.Services
{
    public class EmpruntService : IEmpruntService
    {
        private readonly LibraryContext _context;

        public EmpruntService(LibraryContext context)
        {
            _context = context;
        }

        public async Task<List<Emprunt>> GetAllAsync()
        {
            return await _context.Emprunts
                .Include(l => l.Livre)
                .Include(l => l.Membre)
                .ToListAsync();
        }

        public async Task<Emprunt> GetByIdAsync(int id)
        {
            return await _context.Emprunts
                .Include(l => l.Livre)
                .Include(l => l.Membre)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task CreateAsync(Emprunt emprunt)
        {
            // 1. Validate the member's borrowing capacity
            var activeLoanCount = await _context.Emprunts
                .CountAsync(l => l.MembreId == emprunt.MembreId && l.DateRetourEffective == null);

            if (activeLoanCount >= 3)
            {
                throw new InvalidOperationException("Un membre ne peut pas emprunter plus de 3 livres à la fois.");
            }

            // Check if member already has this specific book
            var alreadyHasBook = await _context.Emprunts
                .AnyAsync(l => l.MembreId == emprunt.MembreId && l.LivreId == emprunt.LivreId && l.DateRetourEffective == null);

            if (alreadyHasBook)
            {
                throw new InvalidOperationException("Vous avez déjà un emprunt actif pour ce livre.");
            }

            // 2. Validate the book's existence
            var livre = await _context.Livres.FindAsync(emprunt.LivreId);
            if (livre == null)
            {
                throw new KeyNotFoundException("Le livre demandé n'existe pas.");
            }

            // 3. Check book availability
            if (livre.NombreExemplaires > 0)
            {
                // Book is available, proceed with loan
                livre.NombreExemplaires--;
                _context.Update(livre);
                _context.Add(emprunt);
                await _context.SaveChangesAsync();
                return;
            }

            // 4. Book is not in stock, check for reservations
            var firstReservation = await _context.Reservations
                .Where(r => r.LivreId == emprunt.LivreId)
                .OrderBy(r => r.DateReservation)
                .FirstOrDefaultAsync();

            if (firstReservation != null)
            {
                // A reservation exists, check if it belongs to the current member
                if (firstReservation.MembreId == emprunt.MembreId)
                {
                    // The loan is for the reserving member, proceed and remove the reservation
                    _context.Reservations.Remove(firstReservation);
                    _context.Add(emprunt);
                    await _context.SaveChangesAsync();
                    return;
                }
                else
                {
                    // The book is reserved by someone else
                    throw new InvalidOperationException("Ce livre est actuellement réservé par un autre membre.");
                }
            }
            else
            {
                // No copies and no reservations
                throw new InvalidOperationException("Ce livre n'est pas disponible en stock et aucune réservation n'existe.");
            }
        }

        public async Task UpdateAsync(Emprunt emprunt)
        {
            var originalLoan = await _context.Emprunts.AsNoTracking().FirstOrDefaultAsync(l => l.Id == emprunt.Id);

            if (originalLoan == null)
            {
                throw new KeyNotFoundException("L'emprunt à mettre à jour n'a pas été trouvé.");
            }

            _context.Update(emprunt);

            // Check if the book is being returned in this update
            bool wasJustReturned = originalLoan.DateRetourEffective == null && emprunt.DateRetourEffective != null;

            if (wasJustReturned)
            {
                // 1. Increment book stock
                var livre = await _context.Livres.FindAsync(emprunt.LivreId);
                if (livre != null)
                {
                    livre.NombreExemplaires++;
                    _context.Update(livre);

                    // 2. Notify about the next reservation (logic to be implemented, e.g., send email)
                    var nextReservation = await _context.Reservations
                        .Include(r => r.Membre)
                        .Where(r => r.LivreId == livre.Id)
                        .OrderBy(r => r.DateReservation)
                        .FirstOrDefaultAsync();

                    if (nextReservation != null)
                    {
                        // In a real app, you would trigger a notification here.
                        // For now, we'll keep the console log for demonstration.
                        System.Diagnostics.Debug.WriteLine($"Livre '{livre.Titre}' disponible pour {nextReservation.Membre.FullName}.");
                    }
                }

                // 3. Check for and create a fine if the book is late
                if (emprunt.DateRetourEffective > emprunt.DateRetourPrevue)
                {
                    var daysLate = (emprunt.DateRetourEffective.Value - emprunt.DateRetourPrevue).Days;
                    if (daysLate > 0)
                    {
                        var amende = new Amende
                        {
                            EmpruntId = emprunt.Id,
                            Montant = daysLate * 5, // Assuming a fine of 5 per day
                            EstPaye = false,
                            DateCreation = DateTime.UtcNow
                        };
                        _context.Add(amende);
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var emprunt = await _context.Emprunts.FindAsync(id);
            if (emprunt != null)
            {
                _context.Emprunts.Remove(emprunt);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ReturnAsync(int id)
        {
            var emprunt = await GetByIdAsync(id);
            if (emprunt != null)
            {
                emprunt.DateRetourEffective = DateTime.UtcNow;
                await UpdateAsync(emprunt);
            }
        }

        public async Task<List<Emprunt>> GetLateLoansAsync()
        {
            return await _context.Emprunts
                .Include(l => l.Livre)
                .Include(l => l.Membre)
                .Where(l => l.DateRetourEffective == null && l.DateRetourPrevue < DateTime.Now)
                .ToListAsync();
        }

        public async Task<List<Emprunt>> GetLoansByMemberIdAsync(int memberId)
        {
            return await _context.Emprunts
                .Include(l => l.Livre)
                .Include(l => l.Membre)
                .Where(l => l.MembreId == memberId)
                .ToListAsync();
        }

        public bool EmpruntExists(int id)
        {
            return _context.Emprunts.Any(e => e.Id == id);
        }
    }
}
