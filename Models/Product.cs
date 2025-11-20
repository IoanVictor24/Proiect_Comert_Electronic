namespace ProiectCE.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Specifications { get; set; } // Adăugat conform cerințelor
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Supplier { get; set; }
        public string DeliveryMethod { get; set; }

        // Cheia străină: leagă produsul de o anumită categorie
        public int CategoryId { get; set; }

        // Proprietate de navigare: îți permite să accesezi detaliile Categoriei
        public Category Category { get; set; } 
    }
}