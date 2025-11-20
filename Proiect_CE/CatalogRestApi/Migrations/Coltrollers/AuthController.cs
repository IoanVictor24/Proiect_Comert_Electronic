using Microsoft.AspNetCore.Mvc;
using CatalogRestApi.Data;
using CatalogRestApi.Models; // Modelul User
using CatalogRestApi.Dtos; // DTO-urile noastre
using Microsoft.AspNetCore.Identity; // Pachetul pentru a ne asigura că este disponibil, deși folosim o metodă mai simplă
using System.Threading.Tasks; // Pentru lucrul cu operații asincrone
using Microsoft.EntityFrameworkCore; // Pentru FirstOrDefaultAsync

namespace CatalogRestApi.Controllers
{
    [ApiController] 
    [Route("api/[controller]")] // Ruta: /api/Auth
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        
        // Nu mai avem nevoie de IPasswordHasher<User> dacă folosim metoda HMACSHA512
        // private readonly IPasswordHasher<User> _passwordHasher; 
        
        // Constructorul: Primește contextul bazei de date (DB Context)
        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ----------------------------------------------------------------------------------
        // METODE HELPER PENTRU SECURITATE (HASHING)
        // ----------------------------------------------------------------------------------

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key; // Key-ul este Salt-ul (șirul aleatoriu)
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            // Folosește același Salt pentru a recrea Hash-ul
            using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                
                // Compară Hash-ul introdus cu cel stocat (byte cu byte)
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }
            return true;
        }


        // ----------------------------------------------------------------------------------
        // ENDPOINT: ÎNREGISTRARE (POST /api/Auth/register)
        // ----------------------------------------------------------------------------------
        [HttpPost("register")] 
        public async Task<IActionResult> Register(UserRegisterDto userDto)
        {
            // 1. Verifică dacă utilizatorul există deja (Case-insensitive)
            if (await _context.Users.AnyAsync(u => u.Username.ToLower() == userDto.Username.ToLower()))
            {
                return BadRequest("Username-ul este deja folosit.");
            }

            // 2. Criptează parola
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(userDto.Password, out passwordHash, out passwordSalt);

            // 3. Creează noul obiect User (Model)
            var userToCreate = new User
            {
                Username = userDto.Username,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                // Setează rolul ("admin" sau "user")
                Role = userDto.Role.ToLower() == "admin" ? "admin" : "user" 
            };

            // 4. Salvează în baza de date
            _context.Users.Add(userToCreate);
            await _context.SaveChangesAsync(); 

            // 5. Răspuns de succes
            return StatusCode(201); // 201 Created
        }

        // ----------------------------------------------------------------------------------
        // ENDPOINT: LOGIN (POST /api/Auth/login)
        // ----------------------------------------------------------------------------------
        [HttpPost("login")] 
        public async Task<IActionResult> Login(UserLoginDto userDto)
        {
            // 1. Caută utilizatorul după nume
            var userFromRepo = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == userDto.Username.ToLower());

            // 2. Verifică dacă utilizatorul există
            if (userFromRepo == null)
            {
                // Este o practică de securitate să nu spunem dacă lipsește userul sau e greșită parola
                return Unauthorized("Username sau parolă incorectă."); 
            }

            // 3. Verifică parola
            if (!VerifyPasswordHash(userDto.Password, userFromRepo.PasswordHash, userFromRepo.PasswordSalt))
            {
                return Unauthorized("Username sau parolă incorectă.");
            }

            // 4. Autentificare reușită: În etapa următoare, vom returna un Token JWT aici
            return Ok(new 
            { 
                message = $"Autentificare reușită.", 
                username = userFromRepo.Username,
                role = userFromRepo.Role 
            });
        }
    }
}