using System.ComponentModel.DataAnnotations;

namespace GestionBiblio.Models
{
    public class Livre
    {
        public int Id { get; set; }
        [Required]
        public string Titre { get; set; }
        [Required]
        public string Auteur { get; set; }
        [Required]
        public string ISBN { get; set; }
        [Display(Name = "Année de Publication")]
        public int AnneePublication { get; set; }
        [Display(Name = "Nombre d'Exemplaires")]
        [Range(0, int.MaxValue)]
        public int NombreExemplaires { get; set; }
        
        [Display(Name = "Image")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Catégorie")]
        public int? CategorieId { get; set; }
        public Categorie? Categorie { get; set; }

        public ICollection<Emprunt> Emprunts { get; set; }
        
        public string GetImageUrl()
        {
            if (!string.IsNullOrEmpty(ImageUrl))
            {
                return ImageUrl;
            }
            return "https://img.freepik.com/free-psd/3d-rendering-back-school-icon_23-2149589337.jpg?semt=ais_hybrid&w=740&q=80";
        }

        public Livre()
        {
            Emprunts = new HashSet<Emprunt>();
        }
    }
}
