using GestionBiblio.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GestionBiblio.Data
{
    public class LibraryContext : IdentityDbContext<IdentityUser, IdentityRole, string>
    {
        public LibraryContext(DbContextOptions<LibraryContext> options) : base(options) { }

        public DbSet<Livre> Livres { get; set; }
        public DbSet<Membre> Membres { get; set; }
        public DbSet<Emprunt> Emprunts { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Amende> Amendes { get; set; }
        public DbSet<Categorie> Categories { get; set; }
    }
}
