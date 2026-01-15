using Microsoft.AspNetCore.Mvc;
using ProiectCE.Models;
using System.Collections.Generic;

namespace Proiect_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        // Deocamdată folosim o listă statică pentru testare în Swagger
        // Într-o aplicație reală, aici ai injecta ApplicationDbContext
        private static List<CartItem> _cartItems = new List<CartItem>();

        // GET: api/cart
        [HttpGet]
        public ActionResult<List<CartItem>> GetCart()
        {
            return Ok(_cartItems);
        }

        // POST: api/cart
        [HttpPost]
        public ActionResult AddToCart(CartItem item)
        {
            _cartItems.Add(item);
            return Ok("Produs adăugat");
        }

        // DELETE: api/cart/{id} -> ASTA ESTE CEEA CE AI CERUT
        [HttpDelete("{productId}")]
        public ActionResult DeleteFromCart(int productId)
        {
            // Căutăm produsul în lista de pe server
            var itemToDelete = _cartItems.Find(x => x.Product.Id == productId);

            if (itemToDelete == null)
            {
                return NotFound("Produsul nu este în coș.");
            }

            _cartItems.Remove(itemToDelete);
            return Ok("Produs șters din coș cu succes.");
        }
    }
}