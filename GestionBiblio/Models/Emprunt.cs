using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GestionBiblio.Models
{
    public class Emprunt
    {
        public Emprunt()
        {
            Amendes = new HashSet<Amende>();
        }

        public int Id { get; set; }
        public int LivreId { get; set; }
        public int MembreId { get; set; }
        [Display(Name = "Date d'Emprunt")]
        public DateTime DateEmprunt { get; set; }
        [Display(Name = "Date de Retour Prévue")]
        public DateTime DateRetourPrevue { get; set; }
        [Display(Name = "Date de Retour Effective")]
        public DateTime? DateRetourEffective { get; set; }

        public Livre? Livre { get; set; }
        public Membre? Membre { get; set; }
        public ICollection<Amende> Amendes { get; set; }

        public bool IsReturned => DateRetourEffective.HasValue;
        public bool IsLate => !IsReturned && DateTime.Now > DateRetourPrevue;

        public string DescriptionEmprunt
        {
            get
            {
                string titreLivre = Livre?.Titre ?? "Livre inconnu";
                string nomMembre = Membre?.FullName ?? "Membre inconnu";
                return $"{titreLivre} - {nomMembre} ({DateEmprunt.ToShortDateString()})";
            }
        }
    }
}
