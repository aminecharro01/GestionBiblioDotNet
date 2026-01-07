using GestionBiblio.Data;
using GestionBiblio.Models;
using GestionBiblio.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<LibraryContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<LibraryContext>()
    .AddDefaultTokenProviders();

// Add services to the container.
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IEmpruntService, EmpruntService>();
builder.Services.AddScoped<IAmendeService, AmendeService>();
builder.Services.AddScoped<ILivreService, LivreService>();
builder.Services.AddScoped<IMembreService, MembreService>();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});

var app = builder.Build();

// Seed Default Roles and Admin User
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // Seed Roles
    string[] roleNames = { "Admin", "Member" };
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // Seed Admin User
    var adminEmail = "admin@gestionbiblio.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true // Assuming admin email is confirmed
        };
        await userManager.CreateAsync(adminUser, "AdminP@ssw0rd"); // Set a strong default password
    }
    
    if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
    {
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }

    // Ensure Admin has a corresponding Membre record
    var context = scope.ServiceProvider.GetRequiredService<LibraryContext>();
    var adminMembre = await context.Membres.FirstOrDefaultAsync(m => m.Email == adminEmail);
    if (adminMembre == null)
    {
        adminMembre = new Membre
        {
            Email = adminEmail,
            Nom = "Admin",
            Prenom = "System",
            DateAdhesion = DateTime.Now
        };
        context.Membres.Add(adminMembre);
        await context.SaveChangesAsync();
    }

    // Seed Categories (Ensure specific categories exist)
    var categoriesToSeed = new[] { "Roman", "Science-Fiction", "Fantastique", "Biographie", "Histoire", "Science", "Technologie" };
    foreach (var catName in categoriesToSeed)
    {
        if (!await context.Categories.AnyAsync(c => c.Nom == catName))
        {
            context.Categories.Add(new Categorie { Nom = catName });
        }
    }
    await context.SaveChangesAsync();

    // Seed Books - Check and create each book individually if missing
    var booksToSeed = new List<Livre>
    {
        new Livre { Titre = "Les Misérables", Auteur = "Victor Hugo", ISBN = "978-0140444308", AnneePublication = 1862, NombreExemplaires = 5, CategorieId = (await context.Categories.FirstOrDefaultAsync(c => c.Nom == "Roman"))?.Id, ImageUrl = "https://covers.openlibrary.org/b/isbn/9780140444308-L.jpg" },
        new Livre { Titre = "1984", Auteur = "George Orwell", ISBN = "978-0451524935", AnneePublication = 1949, NombreExemplaires = 10, CategorieId = (await context.Categories.FirstOrDefaultAsync(c => c.Nom == "Science-Fiction"))?.Id, ImageUrl = "https://m.media-amazon.com/images/I/71kxa1-0mfL._AC_UF1000,1000_QL80_.jpg" },
        new Livre { Titre = "Le Seigneur des Anneaux", Auteur = "J.R.R. Tolkien", ISBN = "978-0544003415", AnneePublication = 1954, NombreExemplaires = 3, CategorieId = (await context.Categories.FirstOrDefaultAsync(c => c.Nom == "Fantastique"))?.Id, ImageUrl = "https://m.media-amazon.com/images/I/91dSMhdIzTL._AC_UF1000,1000_QL80_.jpg" },
        new Livre { Titre = "Dune", Auteur = "Frank Herbert", ISBN = "978-0441172719", AnneePublication = 1965, NombreExemplaires = 4, CategorieId = (await context.Categories.FirstOrDefaultAsync(c => c.Nom == "Science-Fiction"))?.Id, ImageUrl = "https://covers.openlibrary.org/b/isbn/9780441172719-L.jpg" },
        new Livre { Titre = "Clean Code", Auteur = "Robert C. Martin", ISBN = "978-0132350884", AnneePublication = 2008, NombreExemplaires = 8, CategorieId = (await context.Categories.FirstOrDefaultAsync(c => c.Nom == "Technologie"))?.Id, ImageUrl = "https://m.media-amazon.com/images/I/41xShlnTZTL._AC_UF1000,1000_QL80_.jpg" },
        new Livre { Titre = "L'Étranger", Auteur = "Albert Camus", ISBN = "978-0679720201", AnneePublication = 1942, NombreExemplaires = 6, CategorieId = (await context.Categories.FirstOrDefaultAsync(c => c.Nom == "Roman"))?.Id, ImageUrl = "https://covers.openlibrary.org/b/isbn/9780679720201-L.jpg" }
    };

    foreach (var book in booksToSeed)
    {
        if (!await context.Livres.AnyAsync(l => l.Titre == book.Titre))
        {
            // Note: We need to set Categorie object if CategorieId is null or re-fetch category if we want to be safe, 
            // but setting CategorieId is sufficient if we just fetched it. 
            // However, the above initialization using 'await' inside initializer might need care or separate logic.
            // Simplified approach: match category by name again inside the loop or trust the ID.
            if (book.CategorieId == null) 
            {
                 // Fetch based on logic or skip? Let's skip safely or assign default.
                 continue; 
            }
            context.Livres.Add(book);
        }
    }
    await context.SaveChangesAsync();

    // Seed Test Member and Scenarios
    Console.WriteLine("DEBUGGING SEED: Starting Test Member Seeding...");
    var testEmail = "membre@test.com";

    // 1. Cleanup existing test data (optional, but good for resetting state completely)
    var existingMember = await context.Membres.Include(m => m.Emprunts).ThenInclude(e => e.Amendes).FirstOrDefaultAsync(m => m.Email == testEmail);
    if (existingMember != null)
    {
        Console.WriteLine($"DEBUGGING SEED: Found existing member {existingMember.Id}. Cleaning up loans...");
        context.Amendes.RemoveRange(existingMember.Emprunts.SelectMany(e => e.Amendes));
        context.Emprunts.RemoveRange(existingMember.Emprunts);
        // context.Membres.Remove(existingMember); // Option: Delete member too? Let's keep member but clear loans.
        await context.SaveChangesAsync();
        Console.WriteLine("DEBUGGING SEED: Loans cleaned up.");
    }
    
    // 2. Ensure Identity User exists
    var testUser = await userManager.FindByEmailAsync(testEmail);
    if (testUser == null)
    {
        Console.WriteLine("DEBUGGING SEED: Creating Identity User...");
        testUser = new IdentityUser
        {
            UserName = testEmail,
            Email = testEmail,
            EmailConfirmed = true
        };
        await userManager.CreateAsync(testUser, "Test@1234");
        await userManager.AddToRoleAsync(testUser, "Member");
    }

    // 3. Ensure Membre exists (if not deleted above)
    var testMembre = await context.Membres.FirstOrDefaultAsync(m => m.Email == testEmail);
    if (testMembre == null)
    {
        Console.WriteLine("DEBUGGING SEED: Creating Membre entity...");
        testMembre = new Membre
        {
            Email = testEmail,
            Nom = "Test",
            Prenom = "Membre",
            DateAdhesion = DateTime.Now.AddMonths(-2)
        };
        context.Membres.Add(testMembre);
        await context.SaveChangesAsync();
    }
    
    Console.WriteLine($"DEBUGGING SEED: Target Member ID = {testMembre.Id}");

    // 4. Create Loans
    var livre1984 = await context.Livres.FirstOrDefaultAsync(l => l.Titre == "1984");
    var livreDune = await context.Livres.FirstOrDefaultAsync(l => l.Titre == "Dune");
    var livreCleanCode = await context.Livres.FirstOrDefaultAsync(l => l.Titre == "Clean Code");

    if (livre1984 != null && livreDune != null && livreCleanCode != null)
    {
        Console.WriteLine("DEBUGGING SEED: Creating fresh loans...");
        var loans = new List<Emprunt>
        {
            // Scenario 1: Late Loan (1984)
            new Emprunt
            {
                LivreId = livre1984.Id,
                MembreId = testMembre.Id,
                DateEmprunt = DateTime.Now.AddDays(-40),
                DateRetourPrevue = DateTime.Now.AddDays(-26),
                DateRetourEffective = null
            },
            // Scenario 2: On Time Loan (Dune)
            new Emprunt
            {
                LivreId = livreDune.Id,
                MembreId = testMembre.Id,
                DateEmprunt = DateTime.Now.AddDays(-2),
                DateRetourPrevue = DateTime.Now.AddDays(12),
                DateRetourEffective = null
            },
            // Scenario 3: Returned Loan with Unpaid Fine
             new Emprunt
            {
                LivreId = livreCleanCode.Id,
                MembreId = testMembre.Id,
                DateEmprunt = DateTime.Now.AddDays(-60),
                DateRetourPrevue = DateTime.Now.AddDays(-46),
                DateRetourEffective = DateTime.Now.AddDays(-16) // Returned 30 days late
            }
        };
        context.Emprunts.AddRange(loans);
        await context.SaveChangesAsync();
        
        // Add Fine
        var lateLoan = loans[2];
        var daysLate = (lateLoan.DateRetourEffective.Value - lateLoan.DateRetourPrevue).Days; 
        var amende = new Amende
        {
            EmpruntId = lateLoan.Id,
            Montant = daysLate * 5,
            EstPaye = false,
            DateCreation = lateLoan.DateRetourEffective.Value
        };
        context.Amendes.Add(amende);
        await context.SaveChangesAsync();
        Console.WriteLine("DEBUGGING SEED: Loans and Fines created successfully.");
    }

    // 5. Create Reservations
    var livreMiserables = await context.Livres.FirstOrDefaultAsync(l => l.Titre == "Les Misérables");
    var livreLOTR = await context.Livres.FirstOrDefaultAsync(l => l.Titre == "Le Seigneur des Anneaux");

    if (livreMiserables != null && livreLOTR != null)
    {
        Console.WriteLine("DEBUGGING SEED: Checking/Creating reservations...");
        
        // Scenario 1: Reservation for Les Misérables
        if (!await context.Reservations.AnyAsync(r => r.LivreId == livreMiserables.Id && r.MembreId == testMembre.Id))
        {
             context.Reservations.Add(new Reservation
             {
                 LivreId = livreMiserables.Id,
                 MembreId = testMembre.Id,
                 DateReservation = DateTime.Now.AddDays(-5)
             });
        }

        // Scenario 2: Reservation for Le Seigneur des Anneaux
        if (!await context.Reservations.AnyAsync(r => r.LivreId == livreLOTR.Id && r.MembreId == testMembre.Id))
        {
             context.Reservations.Add(new Reservation
             {
                 LivreId = livreLOTR.Id,
                 MembreId = testMembre.Id,
                 DateReservation = DateTime.Now.AddDays(-1)
             });
        }
        
        await context.SaveChangesAsync();
        Console.WriteLine("DEBUGGING SEED: Reservations created/verified successfully.");
    }

}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Set up Default Culture for MAD Currency
var defaultDateCulture = "fr-MA";
var ci = new System.Globalization.CultureInfo(defaultDateCulture);
ci.NumberFormat.CurrencySymbol = "MAD"; // Explicitly set symbol if needed, though fr-MA usually uses 'MAD' or 'dh'

// Configure Localization
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(ci),
    SupportedCultures = new List<System.Globalization.CultureInfo> { ci },
    SupportedUICultures = new List<System.Globalization.CultureInfo> { ci }
};
app.UseRequestLocalization(localizationOptions);

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
