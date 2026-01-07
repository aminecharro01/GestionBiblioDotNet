using GestionBiblio.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GestionBiblio.Services
{
    public interface IAmendeService
    {
        Task<List<Amende>> GetAllAsync();
        Task<Amende> GetByIdAsync(int id);
        Task CreateAsync(Amende amende);
        Task UpdateAsync(Amende amende);
        Task DeleteAsync(int id);
        Task<List<Amende>> GetUnpaidFinesAsync();
        Task<List<Amende>> GetFinesByMemberIdAsync(int memberId);
        Task MarkAsPaidAsync(int id);
        bool AmendeExists(int id);
    }
}
