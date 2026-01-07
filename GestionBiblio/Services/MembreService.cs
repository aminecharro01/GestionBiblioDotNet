using GestionBiblio.Data;
using GestionBiblio.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace GestionBiblio.Services
{
    public class MembreService : IMembreService
    {
        private readonly LibraryContext _context;

        public MembreService(LibraryContext context)
        {
            _context = context;
        }

        public async Task<List<Membre>> GetAllAsync()
        {
            return await _context.Membres.ToListAsync();
        }

        public async Task<List<Membre>> SearchAsync(string searchString)
        {
            var membres = from m in _context.Membres
                          select m;

            if (!string.IsNullOrEmpty(searchString))
            {
                membres = membres.Where(s => s.Nom.Contains(searchString)
                                       || s.Prenom.Contains(searchString)
                                       || s.Email.Contains(searchString));
            }

            return await membres.ToListAsync();
        }

        public async Task<Membre> GetByIdAsync(int id)
        {
            return await _context.Membres.FindAsync(id);
        }

        public async Task<Membre> GetByIdWithRelatedDataAsync(int id)
        {
            return await _context.Membres
                .Include(m => m.Emprunts)
                    .ThenInclude(l => l.Livre)
                .Include(m => m.Emprunts)
                    .ThenInclude(l => l.Amendes)
                .Include(m => m.Reservations)
                    .ThenInclude(r => r.Livre) // Corrected
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Membre> GetByEmailAsync(string email)
        {
            return await _context.Membres
                .Include(m => m.Emprunts)
                    .ThenInclude(l => l.Livre)
                .Include(m => m.Emprunts)
                    .ThenInclude(l => l.Amendes)
                .Include(m => m.Reservations)
                    .ThenInclude(r => r.Livre) // Corrected
                .FirstOrDefaultAsync(m => m.Email == email);
        }

        public async Task CreateAsync(Membre membre)
        {
            _context.Add(membre);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Membre membre)
        {
            _context.Update(membre);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var membre = await _context.Membres.FindAsync(id);
            if (membre != null)
            {
                _context.Membres.Remove(membre);
                await _context.SaveChangesAsync();
            }
        }

        public bool MembreExists(int id)
        {
            return _context.Membres.Any(e => e.Id == id);
        }

        public async Task<Membre> GetOrCreateMemberByEmailAsync(string email)
        {
            var membre = await GetByEmailAsync(email);
            if (membre == null)
            {
                // Create a new Member if it doesn't exist
                var emailParts = email.Split('@');
                membre = new Membre
                {
                    Email = email,
                    Nom = emailParts.Length > 0 ? emailParts[0] : "User",
                    Prenom = "Member",
                    DateAdhesion = DateTime.Now
                };
                _context.Membres.Add(membre);
                await _context.SaveChangesAsync();
            }
            return membre;
        }
    }
}
