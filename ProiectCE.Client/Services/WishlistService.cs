using Blazored.LocalStorage;
using ProiectCE.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProiectCE.Client.Services
{
    public class WishlistService
    {
        private readonly ILocalStorageService _localStorage;
        public event Action OnChange;

        // Lista locală
        public List<Product> Wishlist { get; set; } = new List<Product>();

        public WishlistService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        // Citim lista din memoria browserului
        public async Task GetWishlist()
        {
            // Încercăm să luăm lista salvată. Dacă nu există, creăm una goală.
            Wishlist = await _localStorage.GetItemAsync<List<Product>>("wishlistLocal") ?? new List<Product>();
            OnChange?.Invoke();
        }

        // Adăugăm un produs
        public async Task AddToWishlist(Product product)
        {
            // Ne asigurăm că avem lista încărcată
            if (Wishlist.Count == 0) await GetWishlist();

            // Verificăm dacă există deja ca să nu îl dublăm
            if (!Wishlist.Any(p => p.Id == product.Id))
            {
                Wishlist.Add(product);
                // Salvăm noua listă în browser
                await _localStorage.SetItemAsync("wishlistLocal", Wishlist);
                OnChange?.Invoke();
            }
        }

        // Ștergem un produs
        public async Task RemoveFromWishlist(Product product)
        {
            var item = Wishlist.FirstOrDefault(p => p.Id == product.Id);
            if (item != null)
            {
                Wishlist.Remove(item);
                await _localStorage.SetItemAsync("wishlistLocal", Wishlist);
                OnChange?.Invoke();
            }
        }
    }
}