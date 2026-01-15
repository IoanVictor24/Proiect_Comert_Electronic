using Microsoft.AspNetCore.Mvc;
using ProiectCE.Data;
using ProiectCE.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace ProiectCE.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Ruta de bază: /api/Products
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        // Injectarea bazei de date
        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Metodă ajutătoare pentru a include datele Categoriei în răspuns
        private IQueryable<Product> GetProductsWithCategory()
        {
            return _context.Products.Include(p => p.Category);
        }

        // -------------------------------------------------
        // 1. GET: Răsfoire (Toate produsele) - READ
        // Ruta: GET /api/Products
        // -------------------------------------------------
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            // Returnează produsele, incluzând detaliile categoriei
            return await GetProductsWithCategory().ToListAsync();
        }

        // -------------------------------------------------
        // 2. GET: Răsfoire (Un singur produs după ID) - READ
        // Ruta: GET /api/Products/5
        // -------------------------------------------------
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            // Caută produsul și încarcă și detaliile categoriei
            var product = await GetProductsWithCategory().FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound(); // 404 Not Found
            }

            return product;
        }

        // -------------------------------------------------
        // 3. GET: Căutare după Nume, Furnizor, sau Preț - SEARCH
        // Ruta: GET /api/Products/search?q=termen
        // -------------------------------------------------
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Product>>> SearchProducts([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest("Termenul de căutare nu poate fi gol.");
            }

            var query = GetProductsWithCategory()
                .Where(p =>
                    p.Name.ToLower().Contains(q.ToLower()) ||
                    p.Supplier.ToLower().Contains(q.ToLower()) ||
                    p.Description.ToLower().Contains(q.ToLower())
                );

            return await query.ToListAsync();
        }

        // -------------------------------------------------
        // 3.5. GET: Filtrare Avansată - FILTER
        // Ruta: GET /api/Products/filter?minPrice=100&maxPrice=500&supplier=Samsung
        // -------------------------------------------------
        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<Product>>> FilterProducts(
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] bool? inStock,
            [FromQuery] string? model,
            [FromQuery] string? supplier,
            [FromQuery] string? deliveryMethod)
        {
            // Pornim cu toate produsele
            var query = GetProductsWithCategory().AsQueryable();

            // 1. Filtru Preț Minim
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            // 2. Filtru Preț Maxim
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            // 3. Filtru Stoc (dacă inStock e true, căutăm produse cu stoc > 0)
            if (inStock.HasValue)
            {
                if (inStock.Value)
                {
                    query = query.Where(p => p.Stock > 0);
                }
                else
                {
                    // Dacă utilizatorul vrea produse epuizate
                    query = query.Where(p => p.Stock == 0);
                }
            }

            // 4. Filtru Model (căutare în Nume)
            if (!string.IsNullOrWhiteSpace(model))
            {
                query = query.Where(p => p.Name.ToLower().Contains(model.ToLower()));
            }

            // 5. Filtru Furnizor
            if (!string.IsNullOrWhiteSpace(supplier))
            {
                query = query.Where(p => p.Supplier.ToLower().Contains(supplier.ToLower()));
            }

            // 6. Filtru Metodă Livrare
            if (!string.IsNullOrWhiteSpace(deliveryMethod))
            {
                query = query.Where(p => p.DeliveryMethod.ToLower().Contains(deliveryMethod.ToLower()));
            }

            return await query.ToListAsync();
        }

        // -------------------------------------------------
        // 4. POST: Adăugare Produs Nou - CREATE
        // Ruta: POST /api/Products
        // -------------------------------------------------
        [Authorize(Roles = "admin")]
        [HttpPost]

        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            // 1. Verifică dacă ID-ul categoriei specificate există
            if (!await _context.Categories.AnyAsync(c => c.Id == product.CategoryId))
            {
                return BadRequest("ID-ul categoriei specificate nu există.");
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // 2. Reîncarcă produsul pentru a include datele Categoriei în răspunsul 201 Created
            var createdProduct = await GetProductsWithCategory().FirstOrDefaultAsync(p => p.Id == product.Id);

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, createdProduct);
        }

        // -------------------------------------------------
        // 5. PUT: Editare Produs Existent - UPDATE
        // Ruta: PUT /api/Products/5
        // -------------------------------------------------
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }

            // 1. Verifică dacă ID-ul categoriei specificate există
            if (!await _context.Categories.AnyAsync(c => c.Id == product.CategoryId))
            {
                return BadRequest("ID-ul categoriei specificate nu există.");
            }

            // Marchează entitatea ca modificată
            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent(); // 204 No Content
        }

        // -------------------------------------------------
        // 6. DELETE: Ștergere Produs - DELETE
        // Ruta: DELETE /api/Products/5
        // -------------------------------------------------
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent(); // 204 No Content
        }

        // Metodă ajutătoare pentru verificare
        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}