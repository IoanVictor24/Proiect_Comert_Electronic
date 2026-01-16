using Microsoft.EntityFrameworkCore;
using ProiectCE.Models;

// ATENȚIE: Am schimbat namespace-ul în ProiectCE.Data ca să nu mai dea eroare în Controllers
namespace ProiectCE.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
     
    }
}