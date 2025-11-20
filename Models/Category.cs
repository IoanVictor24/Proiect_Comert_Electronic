using System.Collections.Generic; // Nevoie pentru ICollection

namespace ProiectCE.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        // Proprietate de navigare: EF Core va ști că o Categorie conține mai multe Produse
        public ICollection<Product> Products { get; set; }
    }
}