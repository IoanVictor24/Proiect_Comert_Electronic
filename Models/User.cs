namespace CatalogRestApi.Models
{
    public class User
    {
        // Cheia primară (Id-ul)
        public int Id { get; set; }

        // Numele de utilizator/Email (pentru login)
        public string Username { get; set; }

        // Parola stocată ca hash (NU stocăm parola în text simplu!)
        public byte[] PasswordHash { get; set; }

        // Salt-ul (pentru securitatea hash-ului)
        public byte[] PasswordSalt { get; set; }

        // Rolul: "admin" sau "user" (pentru autorizare)
        public string Role { get; set; } = "user"; // Implicit, toți sunt utilizatori
    }
}