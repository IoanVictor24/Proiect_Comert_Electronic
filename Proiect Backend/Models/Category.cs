using System.Collections.Generic; // Nevoie pentru ICollection
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProiectCE.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        // Proprietate de navigare: EF Core va ști că o Categorie conține mai multe Produse

        [JsonIgnore]
        public ICollection<Product>? Products { get; set; }
    }
}