using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace ProiectCE.Client.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly HttpClient _httpClient;

        public CustomAuthStateProvider(IJSRuntime jsRuntime, HttpClient httpClient)
        {
            _jsRuntime = jsRuntime;
            _httpClient = httpClient;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");

            if (string.IsNullOrEmpty(token))
            {
                // Utilizator neautentificat (Anonim)
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            // Atașăm token-ul la cererile HTTP
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Parsăm token-ul și extragem cererile (claims)
            var claims = ParseClaimsFromJwt(token);

            // AICI ESTE SECRETUL: Îi spunem lui Blazor explicit că rolul se găsește sub cheia "role"
            // și numele utilizatorului sub cheia "unique_name" (sau "name")
            var identity = new ClaimsIdentity(claims, "jwt", "unique_name", "role");

            // Alternativă pentru siguranță maximă (verificăm dacă backend-ul a trimis rolul ca URL lung)
            if (!claims.Any(c => c.Type == "role") && claims.Any(c => c.Type.Contains("/role")))
            {
                // Dacă e URL lung, Blazor îl va găsi automat, dar e bine să fim siguri
            }

            var user = new ClaimsPrincipal(identity);

            return new AuthenticationState(user);
        }

        public void NotifyUserLogout()
        {
            var authState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            NotifyAuthenticationStateChanged(authState);
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            if (keyValuePairs != null)
            {
                foreach (var kvp in keyValuePairs)
                {
                    // TRUCUL MAGIC: Normalizăm cheia pentru Rol
                    var key = kvp.Key;

                    // Dacă cheia conține "role" (chiar și cea lungă de la Microsoft), o redenumim simplu "role"
                    if (key.Contains("role", StringComparison.InvariantCultureIgnoreCase))
                    {
                        key = "role";
                    }

                    if (kvp.Value is JsonElement element && element.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in element.EnumerateArray())
                        {
                            claims.Add(new Claim(key, item.ToString()));
                        }
                    }
                    else
                    {
                        claims.Add(new Claim(key, kvp.Value.ToString()));
                    }
                }
            }

            return claims;
        }

        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}