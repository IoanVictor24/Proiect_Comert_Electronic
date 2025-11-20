using Microsoft.EntityFrameworkCore;
using ProiectCE.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    // Folosește SQLite, citind stringul de conexiune "DefaultConnection" din appsettings.json
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
