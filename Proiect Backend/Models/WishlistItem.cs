using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProiectCE.Models
{
    public class WishlistItem
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; } // ID-ul utilizatorului care a salvat produsul

        public int ProductId { get; set; } // ID-ul produsului salvat

        [ForeignKey("ProductId")]
        public Product? Product { get; set; } // Legătura cu tabela de produse
    }
}