using Microsoft.AspNetCore.Mvc;
using ProiectCE.Data; // ATENȚIE: Aici folosim noul namespace ProiectCE
using ProiectCE.Models;
using ProiectCE.Dtos;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using ProiectCE.Data;
using ProiectCE.Models;

namespace ProiectCE.Controllers // ATENȚIE: Noul Namespace
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

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
            // Folosim HMACSHA512 pentru hashing securizat
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            // Folosește același Salt pentru a recrea Hash-ul
            using (var hmac = new HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

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
                return Unauthorized("Username sau parolă incorectă.");
            }

            // 3. Verifică parola
            if (!VerifyPasswordHash(userDto.Password, userFromRepo.PasswordHash, userFromRepo.PasswordSalt))
            {
                return Unauthorized("Username sau parolă incorectă.");
            }

            // 4. Autentificare reușită
            return Ok(new
            {
                message = $"Autentificare reușită.",
                username = userFromRepo.Username,
                role = userFromRepo.Role
            });
        }
    }
}