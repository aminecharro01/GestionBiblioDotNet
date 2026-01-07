using System;
using System.ComponentModel.DataAnnotations;

namespace GestionBiblio.Models
{
    public class Amende
    {
        public int Id { get; set; }
        public int EmpruntId { get; set; }
        public decimal Montant { get; set; }
        [Display(Name = "Est Payé")]
        public bool EstPaye { get; set; }
        [Display(Name = "Date de Création")]
        public DateTime DateCreation { get; set; }
        [Display(Name = "Date de Paiement")]
        public DateTime? DatePaiement { get; set; }

        public Emprunt Emprunt { get; set; } = null!;
    }
}
