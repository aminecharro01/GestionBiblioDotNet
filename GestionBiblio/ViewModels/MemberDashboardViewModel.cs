using GestionBiblio.Models;
using System.Collections.Generic;

namespace GestionBiblio.ViewModels
{
    public class MemberDashboardViewModel
    {
        public Membre Member { get; set; }
        
        // Collections
        public List<Livre> AvailableBooks { get; set; } = new List<Livre>();
        public List<Emprunt> MemberLoans { get; set; } = new List<Emprunt>();
        public List<Reservation> MemberReservations { get; set; } = new List<Reservation>();
        public List<Amende> UnpaidFines { get; set; } = new List<Amende>();
        public List<Emprunt> LateLoans { get; set; } = new List<Emprunt>();

        // Pre-calculated Counts
        public int ActiveLoansCount { get; set; }
        public int LateLoansCount { get; set; }
        public int ReservationsCount { get; set; }
        public int UnpaidFinesCount { get; set; }
    }
}
