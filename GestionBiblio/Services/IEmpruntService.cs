using GestionBiblio.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GestionBiblio.Services
{
    public interface IEmpruntService
    {
        Task<List<Emprunt>> GetAllAsync();
        Task<Emprunt> GetByIdAsync(int id);
        Task CreateAsync(Emprunt emprunt);
        Task UpdateAsync(Emprunt emprunt);
        Task DeleteAsync(int id);
        Task ReturnAsync(int id);
        Task<List<Emprunt>> GetLateLoansAsync(); // Je garde le nom de méthode pour éviter trop de cassures dans les controlleurs
        Task<List<Emprunt>> GetLoansByMemberIdAsync(int memberId);
        bool EmpruntExists(int id);
    }
}
