using GestionBiblio.Data;
using GestionBiblio.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GestionBiblio.Services
{
    public class AmendeService : IAmendeService
    {
        private readonly LibraryContext _context;

        public AmendeService(LibraryContext context)
        {
            _context = context;
        }

        public async Task<List<Amende>> GetAllAsync()
        {
            return await _context.Amendes
                .Include(f => f.Emprunt)
                .ThenInclude(l => l.Membre)
                .Include(f => f.Emprunt)
                .ThenInclude(l => l.Livre)
                .ToListAsync();
        }

        public async Task<Amende> GetByIdAsync(int id)
        {
            return await _context.Amendes
                .Include(f => f.Emprunt)
                .ThenInclude(l => l.Membre)
                .Include(f => f.Emprunt)
                .ThenInclude(l => l.Livre)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task CreateAsync(Amende amende)
        {
            _context.Add(amende);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Amende amende)
        {
            var originalFine = await _context.Amendes.AsNoTracking().FirstOrDefaultAsync(f => f.Id == amende.Id);
            if (originalFine != null && !originalFine.EstPaye && amende.EstPaye)
            {
                amende.DatePaiement = DateTime.UtcNow;
            }
            
            amende.DateCreation = originalFine.DateCreation;

            _context.Update(amende);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var amende = await _context.Amendes.FindAsync(id);
            if (amende != null)
            {
                _context.Amendes.Remove(amende);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Amende>> GetUnpaidFinesAsync()
        {
            return await _context.Amendes
                .Include(f => f.Emprunt)
                .ThenInclude(l => l.Membre)
                .Include(f => f.Emprunt)
                .ThenInclude(l => l.Livre)
                .Where(f => !f.EstPaye)
                .ToListAsync();
        }

        public async Task<List<Amende>> GetFinesByMemberIdAsync(int memberId)
        {
            return await _context.Amendes
                .Include(f => f.Emprunt)
                    .ThenInclude(l => l.Membre)
                .Include(f => f.Emprunt)
                    .ThenInclude(l => l.Livre)
                .Where(f => f.Emprunt.MembreId == memberId)
                .ToListAsync();
        }

        public async Task MarkAsPaidAsync(int id)
        {
            var amende = await _context.Amendes.FindAsync(id);
            if (amende != null)
            {
                amende.EstPaye = true;
                amende.DatePaiement = DateTime.UtcNow;
                _context.Update(amende);
                await _context.SaveChangesAsync();
            }
        }

        public bool AmendeExists(int id)
        {
            return _context.Amendes.Any(e => e.Id == id);
        }
    }
}
