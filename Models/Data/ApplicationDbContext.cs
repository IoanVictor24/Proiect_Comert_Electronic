using Microsoft.EntityFrameworkCore;
using ProiectCE.Models;

namespace ProiectCE.Data
{
    public class ApplicationDbContext : DbContext
    {
        // Constructor: Configurează baza de date
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet-urile reprezintă tabelele din baza de date
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
    }
}