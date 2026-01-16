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
    public class ReservationsController : Controller
    {
        private readonly IReservationService _reservationService;
        private readonly LibraryContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IMembreService _membreService;

        public ReservationsController(IReservationService reservationService, LibraryContext context, UserManager<IdentityUser> userManager, IMembreService membreService)
        {
            _reservationService = reservationService;
            _context = context;
            _userManager = userManager;
            _membreService = membreService;
        }

        // GET: Reservations
        [Authorize]
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
            {
                return View(await _reservationService.GetAllAsync());
            }
            else
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null || currentUser.Email == null)
                {
                    return Challenge(); // Force re-login if user session is stale
                }
                var membre = await _membreService.GetOrCreateMemberByEmailAsync(currentUser.Email);
                return View(await _reservationService.GetReservationsByMemberIdAsync(membre.Id));
            }
        }

        // GET: Reservations/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _reservationService.GetByIdAsync(id.Value);
            if (reservation == null)
            {
                return NotFound();
            }

            // Check if the current user is an admin or if the reservation belongs to the logged-in member
            if (!User.IsInRole("Admin"))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null || currentUser.Email == null)
                {
                    return Challenge(); // Force re-login if user session is stale
                }
                var membre = await _membreService.GetByEmailAsync(currentUser.Email);
                if (membre == null || membre.Id != reservation.MembreId)
                {
                    return Forbid();
                }
            }

            return View(reservation);
        }

        // GET: Reservations/Create
        [Authorize]
        public async Task<IActionResult> Create(int? livreId)
        {
            if (User.IsInRole("Admin"))
            {
                ViewData["LivreId"] = new SelectList(_context.Livres, "Id", "Titre", livreId);
                ViewData["MembreId"] = new SelectList(_context.Membres, "Id", "FullName");
            }
            else
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null || currentUser.Email == null)
                {
                    return Challenge(); // Force re-login if user session is stale
                }
                var membre = await _membreService.GetOrCreateMemberByEmailAsync(currentUser.Email);

                ViewData["LivreId"] = new SelectList(_context.Livres, "Id", "Titre", livreId);
                ViewData["MembreId"] = new SelectList(new List<Membre> { membre }, "Id", "FullName", membre.Id);
                ViewData["MembreIdReadOnly"] = true;
            }
            return View(new Reservation { DateReservation = DateTime.Now });
        }

        // POST: Reservations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Id,LivreId,MembreId,DateReservation")] Reservation reservation)
        {
            if (!User.IsInRole("Admin"))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null || currentUser.Email == null)
                {
                    return Challenge();
                }
                var membre = await _membreService.GetByEmailAsync(currentUser.Email);
                if (membre == null)
                {
                    return Forbid(); 
                }
                // SECURITY FIX: Force the reservation to be for the logged-in user, ignoring form data
                reservation.MembreId = membre.Id;
                
                // Remove the old check "if (membre.Id != reservation.MembreId)" since we just overwrote it.
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _reservationService.CreateAsync(reservation);
                    TempData["success"] = "Réservation créée avec succès.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            else 
            {
               // DEBUGGING: Log validation errors
               var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
               foreach (var error in errors)
               {
                   Console.WriteLine($"DEBUG RESERVATION ERROR: {error}");
               }
            }

            // Repopulate ViewDatas if ModelState is invalid
            if (User.IsInRole("Admin"))
            {
                ViewData["LivreId"] = new SelectList(_context.Livres, "Id", "Titre", reservation.LivreId);
                ViewData["MembreId"] = new SelectList(_context.Membres, "Id", "FullName", reservation.MembreId);
            }
            else
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null || currentUser.Email == null)
                {
                    return Challenge(); // Force re-login if user session is stale
                }
                var membre = await _membreService.GetByEmailAsync(currentUser.Email);
                ViewData["LivreId"] = new SelectList(_context.Livres, "Id", "Titre", reservation.LivreId);
                ViewData["MembreId"] = new SelectList(new List<Membre> { membre }, "Id", "FullName", membre.Id);
                ViewData["MembreIdReadOnly"] = true;
            }

            return View(reservation);
        }

        // GET: Reservations/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _reservationService.GetByIdAsync(id.Value);
            if (reservation == null)
            {
                return NotFound();
            }
            ViewData["LivreId"] = new SelectList(_context.Livres, "Id", "Titre", reservation.LivreId);
            ViewData["MembreId"] = new SelectList(_context.Membres, "Id", "FullName", reservation.MembreId);
            return View(reservation);
        }

        // POST: Reservations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,LivreId,MembreId,DateReservation")] Reservation reservation)
        {
            if (id != reservation.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _reservationService.UpdateAsync(reservation);
                    TempData["success"] = "Réservation mise à jour avec succès.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservationExists(reservation.Id))
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
            ViewData["LivreId"] = new SelectList(_context.Livres, "Id", "Titre", reservation.LivreId);
            ViewData["MembreId"] = new SelectList(_context.Membres, "Id", "FullName", reservation.MembreId);
            return View(reservation);
        }

        // GET: Reservations/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _reservationService.GetByIdAsync(id.Value);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // POST: Reservations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _reservationService.DeleteAsync(id);
            TempData["success"] = "Réservation supprimée avec succès.";
            return RedirectToAction(nameof(Index));
        }

        private bool ReservationExists(int id)
        {
            return _reservationService.ReservationExists(id);
        }

        // GET: Reservations/ExportToCsv
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportToCsv()
        {
            var reservations = await _reservationService.GetAllAsync();
            using (var memoryStream = new MemoryStream())
            using (var writer = new StreamWriter(memoryStream))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(reservations);
                writer.Flush();
                return File(memoryStream.ToArray(), "text/csv", "reservations.csv");
            }
        }
    }
}