using GestionBiblio.Data;
using GestionBiblio.Models;
using GestionBiblio.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;
using System.Globalization;
using Microsoft.AspNetCore.Identity;

namespace GestionBiblio.Controllers
{
    [Authorize]
    public class EmpruntsController : Controller
    {
        private readonly IEmpruntService _empruntService;
        private readonly LibraryContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IMembreService _membreService;
        private readonly ILogger<EmpruntsController> _logger;

        public EmpruntsController(IEmpruntService empruntService, LibraryContext context, UserManager<IdentityUser> userManager, IMembreService membreService, ILogger<EmpruntsController> logger)
        {
            _empruntService = empruntService;
            _context = context;
            _userManager = userManager;
            _membreService = membreService;
            _logger = logger;
        }

        // GET: Emprunts
        [Authorize]
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
            {
                return View(await _empruntService.GetAllAsync());
            }
            else
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null || currentUser.Email == null)
                {
                    return Forbid();
                }
                var membre = await _membreService.GetOrCreateMemberByEmailAsync(currentUser.Email);
                return View(await _empruntService.GetLoansByMemberIdAsync(membre.Id));
            }
        }

        // GET: Emprunts/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var emprunt = await _empruntService.GetByIdAsync(id.Value);
            if (emprunt == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin"))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null || currentUser.Email == null)
                {
                    return Forbid();
                }
                var membre = await _membreService.GetOrCreateMemberByEmailAsync(currentUser.Email);
                if (membre.Id != emprunt.MembreId)
                {
                    return Forbid();
                }
            }

            return View(emprunt);
        }

        // GET: Emprunts/Create
        [Authorize]
        public async Task<IActionResult> Create(int? livreId)
        {
            if (User.IsInRole("Admin"))
            {
                ViewData["LivreId"] = new SelectList(_context.Livres, "Id", "Titre");
                ViewData["MembreId"] = new SelectList(_context.Membres, "Id", "FullName");
            }
            else
            {
                var currentUser = await _userManager.GetUserAsync(User);
                Console.WriteLine($"DEBUG: IsAuthenticated: {User.Identity?.IsAuthenticated}");
                Console.WriteLine($"DEBUG: AuthType: {User.Identity?.AuthenticationType}");
                foreach (var claim in User.Claims)
                {
                    Console.WriteLine($"DEBUG: Claim: {claim.Type} = {claim.Value}");
                }
                
                Console.WriteLine($"DEBUG: CurrentUser is null? {currentUser == null}");
                 if (currentUser == null)
                {
                     _logger.LogWarning("Create (GET): User is null");
                     Console.WriteLine("DEBUG: Forbid because currentUser is null");
                     // FORCE RE-LOGIN if user is null but claims exist (desync)
                     // return Challenge(); 
                    return Forbid();
                }
                if (currentUser.Email == null)
                {
                     _logger.LogWarning("Create (GET): Email is null");
                     Console.WriteLine("DEBUG: Forbid because Email is null");
                    return Forbid();
                }
                _logger.LogInformation($"Create (GET): User {currentUser.Email} accessing create loan page.");
                var membre = await _membreService.GetOrCreateMemberByEmailAsync(currentUser.Email);

                // For members, pre-select their own MemberId and make it read-only
                ViewData["LivreId"] = new SelectList(_context.Livres, "Id", "Titre", livreId);
                ViewData["MembreId"] = new SelectList(new List<Membre> { membre }, "Id", "FullName", membre.Id);
                ViewData["MembreIdReadOnly"] = true;
            }

            var emprunt = new Emprunt
            {
                DateEmprunt = DateTime.Now,
                DateRetourPrevue = DateTime.Now.AddDays(14) // Standard 2 weeks
                // DateRetourEffective must be null for new loans
            };

            if (!User.IsInRole("Admin"))
            {
                // Ensure model has the correct MembreId for the hidden field
                var currentUser = await _userManager.GetUserAsync(User);
                 if (currentUser != null && currentUser.Email != null)
                 {
                    var membre = await _membreService.GetOrCreateMemberByEmailAsync(currentUser.Email);
                    emprunt.MembreId = membre.Id;
                 }
            }

            return View(emprunt);
        }

        // POST: Emprunts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Id,LivreId,MembreId,DateEmprunt,DateRetourPrevue,DateRetourEffective")] Emprunt emprunt)
        {
            Console.WriteLine($"DEBUG: POST Create reached. MembreId: {emprunt.MembreId}, LivreId: {emprunt.LivreId}");
            
            if (!User.IsInRole("Admin"))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                 Console.WriteLine($"DEBUG: POST Create User: {currentUser?.Email}");
                if (currentUser == null || currentUser.Email == null)
                {
                    Console.WriteLine("DEBUG: POST Create Forbid (User or Email null)");
                    return Forbid();
                }
                var membre = await _membreService.GetOrCreateMemberByEmailAsync(currentUser.Email);
                Console.WriteLine($"DEBUG: POST Create Member ID found: {membre.Id}");
                
                // If the form sends 0 (because disabled field), we must fix it here too? 
                // Wait, if input matches MemberId, good. If not...
                // If the field was disabled, the value MIGHT NOT BE SENT.
                // So emprunt.MembreId might be 0.
                if (emprunt.MembreId == 0) 
                {
                     Console.WriteLine("DEBUG: POST Create MembreId is 0. Setting from logged-in user.");
                     emprunt.MembreId = membre.Id;
                     // Remove ModelState error if any regarding MembreId
                     ModelState.Remove("MembreId"); 
                }

                if (membre.Id != emprunt.MembreId)
                {
                    Console.WriteLine($"DEBUG: POST Create Forbid (Mismatch). MembreId: {membre.Id} vs Emprunt: {emprunt.MembreId}");
                    return Forbid();
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _empruntService.CreateAsync(emprunt);
                    TempData["success"] = "Emprunt créé avec succès.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DEBUG: POST Create Exception: {ex.Message}");
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            else
            {
                 Console.WriteLine("DEBUG: POST Create ModelState Invalid.");
                 foreach (var state in ModelState)
                 {
                     foreach (var error in state.Value.Errors)
                     {
                         Console.WriteLine($"DEBUG: Error in {state.Key}: {error.ErrorMessage}");
                     }
                 }
            }

            if (User.IsInRole("Admin"))
            {
                ViewData["LivreId"] = new SelectList(_context.Livres, "Id", "Titre", emprunt.LivreId);
                ViewData["MembreId"] = new SelectList(_context.Membres, "Id", "FullName", emprunt.MembreId);
            }
            else
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null || currentUser.Email == null)
                {
                    return Forbid();
                }
                var membre = await _membreService.GetOrCreateMemberByEmailAsync(currentUser.Email);
                ViewData["LivreId"] = new SelectList(_context.Livres, "Id", "Titre", emprunt.LivreId);
                ViewData["MembreId"] = new SelectList(new List<Membre> { membre }, "Id", "FullName", membre.Id);
                ViewData["MembreIdReadOnly"] = true;
            }

            return View(emprunt);
        }

        // GET: Emprunts/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var emprunt = await _empruntService.GetByIdAsync(id.Value);
            if (emprunt == null)
            {
                return NotFound();
            }
            ViewData["LivreId"] = new SelectList(_context.Livres, "Id", "Titre", emprunt.LivreId);
            ViewData["MembreId"] = new SelectList(_context.Membres, "Id", "FullName", emprunt.MembreId);
            return View(emprunt);
        }

        // POST: Emprunts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,LivreId,MembreId,DateEmprunt,DateRetourPrevue,DateRetourEffective")] Emprunt emprunt)
        {
            if (id != emprunt.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _empruntService.UpdateAsync(emprunt);
                    TempData["success"] = "Emprunt mis à jour avec succès.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmpruntExists(emprunt.Id))
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
            ViewData["LivreId"] = new SelectList(_context.Livres, "Id", "Titre", emprunt.LivreId);
            ViewData["MembreId"] = new SelectList(_context.Membres, "Id", "FullName", emprunt.MembreId);
            return View(emprunt);
        }

        // GET: Emprunts/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var emprunt = await _empruntService.GetByIdAsync(id.Value);
            if (emprunt == null)
            {
                return NotFound();
            }

            return View(emprunt);
        }

        // POST: Emprunts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _empruntService.DeleteAsync(id);
            TempData["success"] = "Emprunt supprimé avec succès.";
            return RedirectToAction(nameof(Index));
        }

        private bool EmpruntExists(int id)
        {
            return _empruntService.EmpruntExists(id);
        }

        // GET: Emprunts/LateLoans
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> LateLoans()
        {
            return View(await _empruntService.GetLateLoansAsync());
        }

        // POST: Emprunts/Return/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Return(int id)
        {
            await _empruntService.ReturnAsync(id);
            TempData["success"] = "Livre retourné avec succès.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Emprunts/ExportToCsv
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportToCsv()
        {
            var emprunts = await _empruntService.GetAllAsync();
            using (var memoryStream = new MemoryStream())
            using (var writer = new StreamWriter(memoryStream))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(emprunts);
                writer.Flush();
                return File(memoryStream.ToArray(), "text/csv", "emprunts.csv");
            }
        }

        // POST: Emprunts/CreateAutomated
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> CreateAutomated(int livreId)
        {
            var emprunt = new Emprunt
            {
                LivreId = livreId,
                DateEmprunt = DateTime.Now,
                DateRetourPrevue = DateTime.Now.AddDays(10),
                DateRetourEffective = DateTime.Now.AddDays(15)
            };

            if (!User.IsInRole("Admin"))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null || currentUser.Email == null)
                {
                    return Forbid();
                }
                var membre = await _membreService.GetOrCreateMemberByEmailAsync(currentUser.Email);
                emprunt.MembreId = membre.Id;
            }
            else
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var membre = await _membreService.GetOrCreateMemberByEmailAsync(currentUser.Email);
                emprunt.MembreId = membre.Id;
            }

            try
            {
                await _empruntService.CreateAsync(emprunt);
                TempData["success"] = "Emprunt créé automatiquement avec succès.";
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Erreur lors de l'emprunt : {ex.Message}";
                return RedirectToAction("Index", "Livres");
            }

            return RedirectToAction("Index", "Emprunts");
        }
    }
}
