using GestionBiblio.Data;
using GestionBiblio.Models;
using GestionBiblio.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;
using System.Globalization;
using Microsoft.AspNetCore.Identity;

namespace GestionBiblio.Controllers
{
    [Authorize]
    public class AmendesController : Controller
    {
        private readonly IAmendeService _amendeService;
        private readonly LibraryContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IMembreService _membreService;
        private readonly IEmpruntService _empruntService;

        public AmendesController(IAmendeService amendeService, LibraryContext context, UserManager<IdentityUser> userManager, IMembreService membreService, IEmpruntService empruntService)
        {
            _amendeService = amendeService;
            _context = context;
            _userManager = userManager;
            _membreService = membreService;
            _empruntService = empruntService;
        }

        // GET: Amendes
        [Authorize]
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
            {
                return View(await _amendeService.GetAllAsync());
            }
            else
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null || currentUser.Email == null)
                {
                    return Forbid();
                }
                var membre = await _membreService.GetOrCreateMemberByEmailAsync(currentUser.Email);
                return View(await _amendeService.GetFinesByMemberIdAsync(membre.Id));
            }
        }

        // GET: Amendes/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var amende = await _amendeService.GetByIdAsync(id.Value);
            if (amende == null)
            {
                return NotFound();
            }

            // Check if the current user is an admin or if the fine belongs to the logged-in member
            if (!User.IsInRole("Admin"))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null || currentUser.Email == null)
                {
                    return Forbid();
                }
                var membre = await _membreService.GetByEmailAsync(currentUser.Email);
                if (membre == null || amende.Emprunt == null || membre.Id != amende.Emprunt.MembreId)
                {
                    return Forbid();
                }
            }

            return View(amende);
        }

        // GET: Amendes/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["EmpruntId"] = new SelectList(_context.Emprunts.Include(l => l.Livre).Include(l => l.Membre), "Id", "DescriptionEmprunt");
            return View();
        }

        // POST: Amendes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,EmpruntId,Montant,EstPaye,DateCreation,DatePaiement")] Amende amende)
        {
            if (ModelState.IsValid)
            {
                await _amendeService.CreateAsync(amende);
                TempData["success"] = "Amende créée avec succès.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmpruntId"] = new SelectList(_context.Emprunts.Include(l => l.Livre).Include(l => l.Membre), "Id", "DescriptionEmprunt", amende.EmpruntId);
            return View(amende);
        }

        // GET: Amendes/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var amende = await _amendeService.GetByIdAsync(id.Value);
            if (amende == null)
            {
                return NotFound();
            }
            ViewData["EmpruntId"] = new SelectList(_context.Emprunts.Include(l => l.Livre).Include(l => l.Membre), "Id", "DescriptionEmprunt", amende.EmpruntId);
            return View(amende);
        }

        // POST: Amendes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,EmpruntId,Montant,EstPaye,DateCreation,DatePaiement")] Amende amende)
        {
            if (id != amende.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _amendeService.UpdateAsync(amende);
                    TempData["success"] = "Amende mise à jour avec succès.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AmendeExists(amende.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmpruntId"] = new SelectList(_context.Emprunts.Include(l => l.Livre).Include(l => l.Membre), "Id", "DescriptionEmprunt", amende.EmpruntId);
            return View(amende);
        }

        // GET: Amendes/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var amende = await _amendeService.GetByIdAsync(id.Value);
            if (amende == null)
            {
                return NotFound();
            }

            return View(amende);
        }

        // POST: Amendes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _amendeService.DeleteAsync(id);
            TempData["success"] = "Amende supprimée avec succès.";
            return RedirectToAction(nameof(Index));
        }

        private bool AmendeExists(int id)
        {
            return _amendeService.AmendeExists(id);
        }

        // GET: Amendes/UnpaidFines
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UnpaidFines()
        {
            return View(await _amendeService.GetUnpaidFinesAsync());
        }

        // GET: Amendes/ExportToCsv
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportToCsv()
        {
            var amendes = await _amendeService.GetAllAsync();
            using (var memoryStream = new MemoryStream())
            using (var writer = new StreamWriter(memoryStream))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(amendes);
                writer.Flush();
                return File(memoryStream.ToArray(), "text/csv", "amendes.csv");
            }
        }

        // POST: Amendes/Pay/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Pay(int id)
        {
            // Check if the current user is an admin or if the fine belongs to the logged-in member
            if (!User.IsInRole("Admin"))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null || currentUser.Email == null)
                {
                    return Forbid();
                }
                var membre = await _membreService.GetByEmailAsync(currentUser.Email);
                if (membre == null)
                {
                    return Forbid(); // Member not found for the current user.
                }

                var amende = await _amendeService.GetByIdAsync(id); // Get fine with loan included to check member id
                if (amende == null || amende.Emprunt == null || membre.Id != amende.Emprunt.MembreId)
                {
                    return Forbid(); // Not authorized to pay this fine
                }
            }

            await _amendeService.MarkAsPaidAsync(id);
            TempData["success"] = "Amende payée avec succès.";
            return RedirectToAction(nameof(Index));
        }
    }
}
