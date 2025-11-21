using Microsoft.AspNetCore.Mvc;
using ProiectCE.Data;
using ProiectCE.Models;
using ProiectCE.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration; // Necesar pentru IConfiguration
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography; // Pentru HMACSHA512
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic; // Pentru List<Claim>

namespace ProiectCE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config; // Injectarea Configurației

        public AuthController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config; // Salvează configurația pentru a accesa cheia JWT
        }

        // ----------------------------------------------------------------------------------
        // METODE HELPER PENTRU SECURITATE (HASHING ȘI VERIFICARE)
        // ----------------------------------------------------------------------------------

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            using (var hmac = new HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }
            return true;
        }

        // ----------------------------------------------------------------------------------
        // METODĂ HELPER: GENERARE TOKEN JWT (Pasul 10.2.4)
        // ----------------------------------------------------------------------------------

        private string CreateToken(User user)
        {
            // 1. Definirea revendicărilor (Claims: ID, Nume, Rol)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role) // Rolul este cel mai important pentru Autorizare
            };

            // 2. Extragerea cheii de securitate din appsettings.json
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));

            // 3. Crearea credențialelor (Cheia + Algoritmul de semnare)
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            // 4. Descrierea token-ului
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1), // Expiră în 1 zi
                SigningCredentials = creds
            };

            // 5. Generarea token-ului
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token); // Returnează string-ul final
        }

        // ----------------------------------------------------------------------------------
        // ENDPOINT: ÎNREGISTRARE (POST /api/Auth/register)
        // ----------------------------------------------------------------------------------

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto userDto)
        {
            if (await _context.Users.AnyAsync(u => u.Username.ToLower() == userDto.Username.ToLower()))
            {
                return BadRequest("Username-ul este deja folosit.");
            }

            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(userDto.Password, out passwordHash, out passwordSalt);

            var userToCreate = new User
            {
                Username = userDto.Username,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Role = userDto.Role.ToLower() == "admin" ? "admin" : "user"
            };

            _context.Users.Add(userToCreate);
            await _context.SaveChangesAsync();

            return StatusCode(201);
        }

        // ----------------------------------------------------------------------------------
        // ENDPOINT: LOGIN (POST /api/Auth/login) - Retur Token (Pasul 10.2.5)
        // ----------------------------------------------------------------------------------

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto userDto)
        {
            var userFromRepo = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == userDto.Username.ToLower());

            if (userFromRepo == null)
            {
                return Unauthorized("Username sau parolă incorectă.");
            }

            if (!VerifyPasswordHash(userDto.Password, userFromRepo.PasswordHash, userFromRepo.PasswordSalt))
            {
                return Unauthorized("Username sau parolă incorectă.");
            }

            // 4. Autentificare reușită: Generează și returnează Token-ul JWT
            var token = CreateToken(userFromRepo);

            return Ok(new { token = token }); // Returnează Token-ul JWT în răspunsul JSON
        }
    }
}