using GestionBiblio.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace GestionBiblio.Controllers
{
    public class TestController : Controller
    {
        private readonly LibraryContext _context;

        public TestController(LibraryContext context)
        {
            _context = context;
        }

        [HttpGet("/test/data")]
        public async Task<IActionResult> Index()
        {
            var membres = await _context.Membres.ToListAsync();
            var emprunts = await _context.Emprunts.Include(e => e.Livre).Include(e => e.Membre).ToListAsync();

            var result = "<h1>Debug Data</h1>";
            result += "<h2>Membres</h2><ul>";
            foreach (var m in membres)
            {
                result += $"<li>ID: {m.Id}, Email: {m.Email}, Name: {m.FullName}</li>";
            }
            result += "</ul>";

            result += "<h2>Emprunts</h2><ul>";
            foreach (var e in emprunts)
            {
                result += $"<li>ID: {e.Id}, Membre: {e.Membre?.Email} (ID: {e.MembreId}), Livre: {e.Livre?.Titre}, DateRetourPrevue: {e.DateRetourPrevue}, IsReturned: {e.DateRetourEffective.HasValue}</li>";
            }
            result += "</ul>";

            return Content(result, "text/html");
        }
    }
}
