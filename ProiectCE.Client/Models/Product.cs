namespace ProiectCE.Client.Models; // <--- Trebuie să fie IDENTIC cu cel din Category.cs

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Specifications { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Supplier { get; set; } = string.Empty;
    public string DeliveryMethod { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    // Acum nu ar trebui să mai aibă eroare, pentru că sunt în același namespace
    public Category? Category { get; set; }
}