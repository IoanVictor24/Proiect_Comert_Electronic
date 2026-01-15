using ProiectCE.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProiectCE.Client.Services
{
    public class CartService
    {
        // Evenimentul care anunță componentele când se modifică coșul
        public event Action OnChange;

        // Am redenumit SelectedItems în CartItems pentru a fi compatibil cu pagina Razor
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();

        // Metodă pentru a adăuga un produs
        public void AddProductToCart(Product product)
        {
            var item = CartItems.FirstOrDefault(x => x.Product.Id == product.Id);
            if (item == null)
            {
                CartItems.Add(new CartItem { Product = product, Quantity = 1 });
            }
            else
            {
                item.Quantity++;
            }

            // Anunțăm că s-a modificat ceva
            OnChange?.Invoke();
        }

        // Metodă nouă pentru ȘTERGERE (pentru butonul cerut)
        public void StergeProdus(CartItem itemDeSters)
        {
            var item = CartItems.FirstOrDefault(p => p.Product.Id == itemDeSters.Product.Id);
            if (item != null)
            {
                CartItems.Remove(item);
                // Anunțăm că s-a modificat ceva
                OnChange?.Invoke();
            }
        }

        // Calculează prețul total
        public decimal GetTotal() => CartItems.Sum(x => x.Product.Price * x.Quantity);
    }
}