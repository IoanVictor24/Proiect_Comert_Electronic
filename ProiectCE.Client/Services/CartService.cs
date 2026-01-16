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

        // Logica pentru coduri bonus
        public string AppliedBonusCode { get; private set; } = "";
        public decimal DiscountPercentage { get; private set; } = 0;

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

        public decimal GetSubTotal() => CartItems.Sum(x => x.Product.Price * x.Quantity);

        // Aplicare cod bonus (Exemplu: REDUCERE10 oferă 10% reducere)
        public bool ApplyBonusCode(string code)
        {
            if (code.ToUpper() == "REDUCERE10")
            {
                AppliedBonusCode = code.ToUpper();
                DiscountPercentage = 10;
                OnChange?.Invoke();
                return true;
            }
            return false;
        }

        public decimal GetDiscount() => GetSubTotal() * (DiscountPercentage / 100);

        public decimal GetDeliveryCost()
        {
            var subTotal = GetSubTotal();
            if (subTotal == 0 || subTotal >= 500) return 0; // Gratuit peste 500 RON
            return 20; // Cost fix livrare
        }

        public decimal GetTotal() => GetSubTotal() + GetDeliveryCost() - GetDiscount();

        public void ClearCart()
        {
            CartItems.Clear();
            AppliedBonusCode = "";
            DiscountPercentage = 0;
            OnChange?.Invoke();
        }
    }
}