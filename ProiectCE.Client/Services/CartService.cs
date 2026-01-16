using ProiectCE.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProiectCE.Client.Services
{
    public class CartService
    {
        public event Action? OnChange;

        public List<CartItem> CartItems { get; set; } = new List<CartItem>();

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
            OnChange?.Invoke();
        }

        public void StergeProdus(CartItem itemDeSters)
        {
            var item = CartItems.FirstOrDefault(p => p.Product.Id == itemDeSters.Product.Id);
            if (item != null)
            {
                CartItems.Remove(item);
                OnChange?.Invoke();
            }
        }

        // --- MODIFICĂRI PENTRU LIVRARE ---

        // 1. Calculăm subtotalul (doar produsele)
        public decimal GetSubTotal() => CartItems.Sum(x => x.Product.Price * x.Quantity);

        // 2. Calculăm costul de livrare (Exemplu: 20 RON, sau Gratuit dacă ai peste 500 RON în coș)
        public decimal GetDeliveryCost()
        {
            var subTotal = GetSubTotal();
            if (subTotal == 0) return 0; // Dacă e gol coșul, 0 lei
            if (subTotal >= 500) return 0; // Gratuit peste 500 RON
            return 20; // Altfel 20 RON
        }

        // 3. Totalul final (Produse + Livrare)
        public decimal GetGrandTotal()
        {
            return GetSubTotal() + GetDeliveryCost();
        }
    }
}