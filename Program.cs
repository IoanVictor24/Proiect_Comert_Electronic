using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
// ----------------------------------------------------------------------
// PASUL 2: CONSTRUIREA APLICAȚIEI
// ----------------------------------------------------------------------
var app = builder.Build();


// ----------------------------------------------------------------------
// PASUL 3: CONFIGURAREA MIDDLEWARE-ULUI (Ordinea este importantă aici)
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    // ADAUGĂ AICI CONFIGURAREA RUTĂRII SWAGGER:
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProiectCE API v1");
    });
}
// ----------------------------------------------------------------------

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
// app.UseAuthorization(); // Îl vom adăuga când implementăm JWT

app.MapControllers(); // Harta rutele Controller-elor (inclusiv AuthController)

app.Run();