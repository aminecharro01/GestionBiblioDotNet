using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GestionBiblio.Models
{
    public class Categorie
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom de la catégorie est requis.")]
        [Display(Name = "Nom de la catégorie")]
        public string Nom { get; set; }

        public ICollection<Livre> Livres { get; set; }
    }
}
