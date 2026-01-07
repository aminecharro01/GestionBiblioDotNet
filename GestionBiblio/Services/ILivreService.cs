using GestionBiblio.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GestionBiblio.Services
{
    public interface ILivreService
    {
        Task<List<Livre>> GetAllAsync();
        Task<List<Livre>> GetAvailableBooksAsync();
        Task<List<Livre>> SearchAsync(string searchString);
        Task<Livre> GetByIdAsync(int id);
        Task CreateAsync(Livre livre);
        Task UpdateAsync(Livre livre);
        Task DeleteAsync(int id);
        bool LivreExists(int id);
    }
}
