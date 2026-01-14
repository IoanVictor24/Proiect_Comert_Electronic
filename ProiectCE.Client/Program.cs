using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ProiectCE.Client;
using Microsoft.AspNetCore.Components.Authorization; // <--- Asta trebuie să fie sus
using ProiectCE.Client.Services;                     // <--- Asta trebuie să fie sus

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Configurare Autentificare
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

// Configurare Servicii (Cart)
builder.Services.AddSingleton<ProiectCE.Client.Services.CartService>();

await builder.Build().RunAsync();
