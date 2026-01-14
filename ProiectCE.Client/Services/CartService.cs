using ProiectCE.Client.Models;

namespace ProiectCE.Client.Services
{
    public class CartService
    {
        // Lista care stochează produsele adăugate
        public List<CartItem> SelectedItems { get; set; } = new List<CartItem>();

        // Metodă pentru a adăuga un produs
        public void AddProductToCart(Product product)
        {
            var item = SelectedItems.FirstOrDefault(x => x.Product.Id == product.Id);
            if (item == null)
            {
                SelectedItems.Add(new CartItem { Product = product, Quantity = 1 });
            }
            else
            {
                item.Quantity++;
            }
        }

        // Calculează prețul total
        public decimal GetTotal() => SelectedItems.Sum(x => x.Product.Price * x.Quantity);
    }
}