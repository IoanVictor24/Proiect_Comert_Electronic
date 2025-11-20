using Microsoft.AspNetCore.Mvc;
using ProiectCE.Data;
using ProiectCE.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization; // Necesar pentru [Authorize]
using System.Linq; // Necesar pentru .Any()

namespace ProiectCE.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Ruta de bază: /api/Categories
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        // Injectarea bazei de date (Dependency Injection)
        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // -------------------------------------------------
        // 1. GET: Răsfoire (Toate categoriile) - PUBLIC READ
        // -------------------------------------------------
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            // Accesibil oricui (nu necesită token)
            return await _context.Categories.ToListAsync();
        }

        // -------------------------------------------------
        // 2. GET: Răsfoire (O singură categorie după ID) - PUBLIC READ
        // -------------------------------------------------
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound(); // 404 Not Found
            }

            return category;
        }

        // -------------------------------------------------
        // 3. POST: Adăugare Categorie Nouă - ADMIN CREATE 🔒
        // -------------------------------------------------
        [Authorize(Roles = "admin")] // <-- NOU: Doar utilizatorii cu rolul "admin" pot accesa
        [HttpPost]
        public async Task<ActionResult<Category>> PostCategory(Category category)
        {
            // Punctul de intrare este protejat de atributul [Authorize]
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
        }

        // -------------------------------------------------
        // 4. PUT: Editare Categorie Existentă - ADMIN UPDATE 🔒
        // -------------------------------------------------
        [Authorize(Roles = "admin")] // <-- NOU: Doar utilizatorii cu rolul "admin" pot edita
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(int id, Category category)
        {
            if (id != category.Id)
            {
                return BadRequest();
            }

            _context.Entry(category).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // -------------------------------------------------
        // 5. DELETE: Ștergere Categorie - ADMIN DELETE 🔒
        // -------------------------------------------------
        [Authorize(Roles = "admin")] // <-- NOU: Doar utilizatorii cu rolul "admin" pot șterge
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Metodă ajutătoare pentru verificare
        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}