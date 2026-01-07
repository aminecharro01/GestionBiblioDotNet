using GestionBiblio.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GestionBiblio.Services
{
    public interface IMembreService
    {
        Task<List<Membre>> GetAllAsync();
        Task<List<Membre>> SearchAsync(string searchString);
        Task<Membre> GetByIdAsync(int id);
        Task<Membre> GetByIdWithRelatedDataAsync(int id);
        Task<Membre> GetByEmailAsync(string email);
        Task<Membre> GetOrCreateMemberByEmailAsync(string email);
        Task CreateAsync(Membre membre);
        Task UpdateAsync(Membre membre);
        Task DeleteAsync(int id);
        bool MembreExists(int id);
    }
}
