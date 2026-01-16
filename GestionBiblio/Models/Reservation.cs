using System;
using System.ComponentModel.DataAnnotations;

namespace GestionBiblio.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public int LivreId { get; set; }
        public int MembreId { get; set; }
        [System.ComponentModel.DataAnnotations.Display(Name = "Date de Réservation")]
        public DateTime DateReservation { get; set; }

        public Livre? Livre { get; set; }
        public Membre? Membre { get; set; }
    }
}