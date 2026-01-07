using GestionBiblio.Data;
using GestionBiblio.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GestionBiblio.Services
{
    public class LivreService : ILivreService
    {
        private readonly LibraryContext _context;

        public LivreService(LibraryContext context)
        {
            _context = context;
        }

        public async Task<List<Livre>> GetAllAsync()
        {
            return await _context.Livres.Include(b => b.Categorie).ToListAsync();
        }

        public async Task<List<Livre>> GetAvailableBooksAsync()
        {
            return await _context.Livres
                .Include(b => b.Categorie)
                .Where(b => b.NombreExemplaires > 0)
                .ToListAsync();
        }

        public async Task<List<Livre>> SearchAsync(string searchString)
        {
            var livres = from b in _context.Livres
                        select b;

            if (!string.IsNullOrEmpty(searchString))
            {
                livres = livres.Where(s => s.Titre.Contains(searchString)
                                       || s.Auteur.Contains(searchString)
                                       || s.ISBN.Contains(searchString));
            }

            return await livres.Include(b => b.Categorie).ToListAsync();
        }

        public async Task<Livre> GetByIdAsync(int id)
        {
            return await _context.Livres
                .Include(b => b.Categorie)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task CreateAsync(Livre livre)
        {
            _context.Add(livre);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Livre livre)
        {
            _context.Update(livre);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var livre = await _context.Livres.FindAsync(id);
            if (livre != null)
            {
                _context.Livres.Remove(livre);
                await _context.SaveChangesAsync();
            }
        }

        public bool LivreExists(int id)
        {
            return _context.Livres.Any(e => e.Id == id);
        }
    }
}
