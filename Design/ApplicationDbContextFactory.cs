using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ProiectCE.Data;

namespace ProiectCE.Design
{
    // Clasa moștenește din IDesignTimeDbContextFactory
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // 1. Obținerea string-ului de conexiune din appsettings.json
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // Setează directorul de bază
                .AddJsonFile("appsettings.json") // Adaugă fișierul de configurare
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // 2. Crearea opțiunilor DbContext
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseSqlite(connectionString);

            // 3. Returnează o instanță a DbContext-ului
            return new ApplicationDbContext(builder.Options);
        }
    }
}