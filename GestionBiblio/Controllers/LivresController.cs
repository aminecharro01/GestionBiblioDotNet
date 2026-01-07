using GestionBiblio.Models;
using GestionBiblio.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;
using System.Globalization;
using System;
using GestionBiblio.Data;

namespace GestionBiblio.Controllers
{
    [Authorize]
    public class LivresController : Controller
    {
        private readonly ILivreService _livreService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly LibraryContext _context; // Added for category dropdown
        private const string DefaultImageUrl = "https://img.freepik.com/free-psd/3d-rendering-back-school-icon_23-2149589337.jpg?semt=ais_hybrid&w=740&q=80";

        public LivresController(ILivreService livreService, IWebHostEnvironment webHostEnvironment, LibraryContext context)
        {
            _livreService = livreService;
            _webHostEnvironment = webHostEnvironment;
            _context = context;
        }

        // GET: Livres
        [Authorize] // All authenticated users can view books
        public async Task<IActionResult> Index(string searchString)
        {
            if (!string.IsNullOrEmpty(searchString))
            {
                return View(await _livreService.SearchAsync(searchString));
            }
            return View(await _livreService.GetAllAsync());
        }

        // GET: Livres/Details/5
        [Authorize] // All authenticated users can view book details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var livre = await _livreService.GetByIdAsync(id.Value);
            if (livre == null)
            {
                return NotFound();
            }

            return View(livre);
        }

        // GET: Livres/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["CategorieId"] = new SelectList(_context.Categories, "Id", "Nom");
            return View();
        }

        // POST: Livres/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Titre,Auteur,ISBN,AnneePublication,NombreExemplaires,CategorieId")] Livre livre, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                livre.ImageUrl = await SaveImageAsync(imageFile);
                await _livreService.CreateAsync(livre);
                TempData["success"] = "Livre créé avec succès.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategorieId"] = new SelectList(_context.Categories, "Id", "Nom", livre.CategorieId);
            return View(livre);
        }

        // GET: Livres/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var livre = await _livreService.GetByIdAsync(id.Value);
            if (livre == null)
            {
                return NotFound();
            }
            ViewData["CategorieId"] = new SelectList(_context.Categories, "Id", "Nom", livre.CategorieId);
            return View(livre);
        }

        // POST: Livres/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Titre,Auteur,ISBN,AnneePublication,NombreExemplaires,ImageUrl,CategorieId")] Livre livre, IFormFile? imageFile)
        {
            if (id != livre.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Si une nouvelle image est uploadée, la sauvegarder
                    if (imageFile != null)
                    {
                        // Supprimer l'ancienne image si elle existe et n'est pas l'image par défaut
                        if (!string.IsNullOrEmpty(livre.ImageUrl) && !livre.ImageUrl.StartsWith("http"))
                        {
                            DeleteImage(livre.ImageUrl);
                        }
                        livre.ImageUrl = await SaveImageAsync(imageFile);
                    }
                    // Si aucune image n'est fournie et qu'il n'y a pas d'image existante, utiliser l'image par défaut
                    else if (string.IsNullOrEmpty(livre.ImageUrl))
                    {
                        livre.ImageUrl = null; // GetImageUrl() utilisera l'image par défaut
                    }
                    
                    await _livreService.UpdateAsync(livre);
                    TempData["success"] = "Livre mis à jour avec succès.";
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
                {
                    if (!LivreExists(livre.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategorieId"] = new SelectList(_context.Categories, "Id", "Nom", livre.CategorieId);
            return View(livre);
        }

        // GET: Livres/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var livre = await _livreService.GetByIdAsync(id.Value);
            if (livre == null)
            {
                return NotFound();
            }

            return View(livre);
        }

        // POST: Livres/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _livreService.DeleteAsync(id);
            TempData["success"] = "Livre supprimé avec succès.";
            return RedirectToAction(nameof(Index));
        }

        private bool LivreExists(int id)
        {
            return _livreService.LivreExists(id);
        }

        private async Task<string?> SaveImageAsync(IFormFile? imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return null; // Retourner null pour utiliser l'image par défaut
            }

            // Créer le dossier images/books s'il n'existe pas
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "books");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Générer un nom de fichier unique
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Sauvegarder le fichier
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            // Retourner le chemin relatif
            return $"/images/books/{uniqueFileName}";
        }

        private void DeleteImage(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl) || imageUrl.StartsWith("http"))
            {
                return; // Ne pas supprimer les URLs externes ou les images par défaut
            }

            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, imageUrl.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        // GET: Livres/ExportToCsv
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportToCsv()
        {
            var livres = await _livreService.GetAllAsync();
            using (var memoryStream = new MemoryStream())
            using (var writer = new StreamWriter(memoryStream))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(livres);
                writer.Flush();
                return File(memoryStream.ToArray(), "text/csv", "livres.csv");
            }
        }
    }
}
