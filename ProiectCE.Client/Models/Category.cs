namespace ProiectCE.Client.Models; // <--- Asigură-te că ai acest punct și virgulă la final!

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}