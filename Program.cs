using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ProiectCE.Data;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------------------------
// PASUL 1: TOATE SERVICIILE SE ADJUGĂ AICI
// ----------------------------------------------------------------------

// 1. Configurarea Bazei de Date (DbContext)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// 2. Adaugă serviciul pentru Controller-e (inclusiv AuthController-ul nostru)
builder.Services.AddControllers();

// 3. Adaugă serviciul Swagger (pentru testare)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Adaugă opțiunea de a introduce token-ul Bearer direct în Swagger UI
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Te rog introdu Token-ul JWT (Bearer token) în câmpul de mai jos.",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// 4. Configurare Autentificare (JWT Bearer) - PAS NOU!
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true, // Validare Cheie Secretă
            // Extrage cheia secretă din AppSettings:Token (din appsettings.json)
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value)),
            ValidateIssuer = false, // Dezactivat pentru mediu local
            ValidateAudience = false // Dezactivat pentru mediu local
        };
    });


// ----------------------------------------------------------------------
// PASUL 2: CONSTRUIREA APLICAȚIEI
// ----------------------------------------------------------------------
var app = builder.Build();


// ----------------------------------------------------------------------
// PASUL 3: CONFIGURAREA MIDDLEWARE-ULUI (Ordinea este importantă aici)
// ----------------------------------------------------------------------

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    // Configurează ruta Swagger (cu opțiunile adăugate anterior)
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProiectCE API v1");
    });
}

app.UseHttpsRedirection();

// ADĂUGARE MIDDLEWARE DE AUTENTIFICARE ȘI AUTORIZARE (Ordinea este crucială: Auth înainte de Map)
app.UseAuthentication(); // <-- Adaugă/Decomentează
app.UseAuthorization();  // <-- Adaugă/Decomentează

app.MapControllers(); // Harta rutele Controller-elor

app.Run();