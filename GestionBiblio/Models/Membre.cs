using System.ComponentModel.DataAnnotations;

namespace GestionBiblio.Models
{
    public class Membre
    {
        public int Id { get; set; }
        [Required]
        public string Nom { get; set; }
        [Required]
        public string Prenom { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Display(Name = "Date d'Adhésion")]
        public DateTime DateAdhesion { get; set; }

        public string FullName
        {
            get
            {
                return $"{Prenom} {Nom}";
            }
        }

        public ICollection<Emprunt> Emprunts { get; set; }
        public ICollection<Reservation> Reservations { get; set; }

        public Membre()
        {
            Emprunts = new HashSet<Emprunt>();
            Reservations = new HashSet<Reservation>();
        }
    }
}
