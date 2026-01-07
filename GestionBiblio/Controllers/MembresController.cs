using GestionBiblio.Models;
using GestionBiblio.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;
using System.Globalization;
using Microsoft.AspNetCore.Identity;

namespace GestionBiblio.Controllers
{
    [Authorize]
    public class MembresController : Controller
    {
        private readonly IMembreService _membreService;
        private readonly UserManager<IdentityUser> _userManager;

        public MembresController(IMembreService membreService, UserManager<IdentityUser> userManager)
        {
            _membreService = membreService;
            _userManager = userManager;
        }

        // GET: Membres
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string searchString)
        {
            if (!string.IsNullOrEmpty(searchString))
            {
                return View(await _membreService.SearchAsync(searchString));
            }
            return View(await _membreService.GetAllAsync());
        }

        // GET: Membres/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var membre = await _membreService.GetByIdAsync(id.Value);
            if (membre == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin"))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null || currentUser.Email == null || currentUser.Email != membre.Email)
                {
                    return Forbid();
                }
            }

            return View(membre);
        }

        // GET: Membres/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Membres/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Nom,Prenom,Email,DateAdhesion")] Membre membre)
        {
            if (ModelState.IsValid)
            {
                await _membreService.CreateAsync(membre);
                TempData["success"] = "Membre créé avec succès.";
                return RedirectToAction(nameof(Index));
            }
            return View(membre);
        }

        // GET: Membres/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var membre = await _membreService.GetByIdAsync(id.Value);
            if (membre == null)
            {
                return NotFound();
            }
            return View(membre);
        }

        // POST: Membres/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nom,Prenom,Email,DateAdhesion")] Membre membre)
        {
            if (id != membre.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _membreService.UpdateAsync(membre);
                    TempData["success"] = "Membre mis à jour avec succès.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MembreExists(membre.Id))
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
            return View(membre);
        }

        // GET: Membres/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var membre = await _membreService.GetByIdAsync(id.Value);
            if (membre == null)
            {
                return NotFound();
            }

            return View(membre);
        }

        // POST: Membres/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _membreService.DeleteAsync(id);
            TempData["success"] = "Membre supprimé avec succès.";
            return RedirectToAction(nameof(Index));
        }

        private bool MembreExists(int id)
        {
            return _membreService.MembreExists(id);
        }

        // GET: Membres/MemberProfile/5
        [Authorize]
        [Route("Membres/Profil/{id?}")] // Route personnalisée pour Profil
        public async Task<IActionResult> Profil(int? id)
        {
            Membre membre;

            if (id == null)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null || currentUser.Email == null)
                {
                    return Forbid();
                }
                membre = await _membreService.GetOrCreateMemberByEmailAsync(currentUser.Email);
            }
            else
            {
                membre = await _membreService.GetByIdWithRelatedDataAsync(id.Value);
                if (membre == null)
                {
                    return NotFound();
                }

                if (!User.IsInRole("Admin"))
                {
                    var currentUser = await _userManager.GetUserAsync(User);
                    if (currentUser == null || currentUser.Email == null || currentUser.Email != membre.Email)
                    {
                        return Forbid();
                    }
                }
            }
            
            return View("Profil", membre); // Use "Profil" view to match filename
        }

        // GET: Membres/ExportToCsv
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportToCsv()
        {
            var membres = await _membreService.GetAllAsync();
            using (var memoryStream = new MemoryStream())
            using (var writer = new StreamWriter(memoryStream))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(membres);
                writer.Flush();
                return File(memoryStream.ToArray(), "text/csv", "membres.csv");
            }
        }
    }
}
