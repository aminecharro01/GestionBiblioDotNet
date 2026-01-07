using GestionBiblio.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace GestionBiblio.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILivreService _livreService;
        private readonly IMembreService _membreService;
        private readonly IEmpruntService _empruntService;
        private readonly IReservationService _reservationService;
        private readonly IAmendeService _amendeService;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(
            ILivreService livreService, 
            IMembreService membreService, 
            IEmpruntService empruntService, 
            IReservationService reservationService,
            IAmendeService amendeService,
            UserManager<IdentityUser> userManager)
        {
            _livreService = livreService;
            _membreService = membreService;
            _empruntService = empruntService;
            _reservationService = reservationService;
            _amendeService = amendeService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
            {
                // Admin Dashboard
                var livres = await _livreService.GetAllAsync();
                var membres = await _membreService.GetAllAsync();
                var emprunts = await _empruntService.GetAllAsync();
                var reservations = await _reservationService.GetAllAsync();

                ViewData["TotalBooks"] = livres.Count;
                ViewData["TotalMembers"] = membres.Count;
                ViewData["ActiveLoans"] = emprunts.Count(l => !l.IsReturned);
                ViewData["LateLoans"] = emprunts.Count(l => l.IsLate);
                ViewData["TotalReservations"] = reservations.Count;

                var booksByAuthor = livres.GroupBy(b => b.Auteur)
                                         .Select(g => new { Author = g.Key, Count = g.Count() })
                                         .ToList();
                ViewData["Authors"] = booksByAuthor.Select(x => x.Author).ToList();
                ViewData["BooksByAuthor"] = booksByAuthor.Select(x => x.Count).ToList();

                var loansPerMonth = emprunts.GroupBy(l => l.DateEmprunt.Month)
                                         .Select(g => new { Month = g.Key, Count = g.Count() })
                                         .OrderBy(x => x.Month)
                                         .ToList();
                ViewData["LoanMonths"] = loansPerMonth.Select(x => new System.Globalization.DateTimeFormatInfo().GetMonthName(x.Month)).ToList();
                ViewData["LoansPerMonth"] = loansPerMonth.Select(x => x.Count).ToList();

                return View();
            }
            else
            {
                // Member Dashboard
                var currentUser = await _userManager.GetUserAsync(User);
                Console.WriteLine($"DEBUG DASHBOARD: User Email = {currentUser?.Email}");

                if (currentUser == null || currentUser.Email == null)
                {
                    return Forbid();
                }

                var membre = await _membreService.GetOrCreateMemberByEmailAsync(currentUser.Email);
                Console.WriteLine($"DEBUG DASHBOARD: Member ID = {membre?.Id}, Name = {membre?.FullName}");
                
                // Get available books
                var availableBooks = await _livreService.GetAvailableBooksAsync();
                
                // Get member's loans
                var memberLoans = await _empruntService.GetLoansByMemberIdAsync(membre.Id);
                Console.WriteLine($"DEBUG DASHBOARD: Loans found = {memberLoans.Count}");
                foreach(var l in memberLoans) Console.WriteLine($"DEBUG LOAN: Id={l.Id}, Book={l.Livre?.Titre}, IsLate={l.IsLate}, Due={l.DateRetourPrevue}");
                
                // Get member's reservations
                var memberReservations = await _reservationService.GetReservationsByMemberIdAsync(membre.Id);
                
                // Get late loans for this member
                var lateLoans = memberLoans.Where(l => l.IsLate).ToList();
                Console.WriteLine($"DEBUG DASHBOARD: Late Loans = {lateLoans.Count}");
                
                // Get unpaid fines for this member
                var unpaidFines = await _amendeService.GetFinesByMemberIdAsync(membre.Id);
                unpaidFines = unpaidFines.Where(f => !f.EstPaye).ToList();
                Console.WriteLine($"DEBUG DASHBOARD: Unpaid Fines = {unpaidFines.Count}");

                var model = new GestionBiblio.ViewModels.MemberDashboardViewModel
                {
                    AvailableBooks = availableBooks,
                    MemberLoans = memberLoans,
                    MemberReservations = memberReservations.Where(r => r != null).ToList(), // Extra safety
                    LateLoans = lateLoans,
                    UnpaidFines = unpaidFines,
                    Member = membre,
                    
                    ActiveLoansCount = memberLoans.Count(l => !l.IsReturned),
                    LateLoansCount = lateLoans.Count,
                    ReservationsCount = memberReservations.Count,
                    UnpaidFinesCount = unpaidFines.Count
                };

                return View("MemberDashboard", model);
            }
        }
    }
}