using Grocery_Ecommerce.Data;
using Grocery_Ecommerce.Models;
using Microsoft.AspNetCore.Mvc;

namespace Grocery_Ecomerce.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Add(int productId, decimal quantity = 1, string redirect = null)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            var existing = _context.CartItems.FirstOrDefault(c => c.UserId == userId.Value && c.ProductId == productId);
            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                _context.CartItems.Add(new CartItem { UserId = userId.Value, ProductId = productId, Quantity = quantity });
            }

            _context.SaveChanges();

            // Redirect based on redirect parameter
            if (redirect == "cart")
            {
                return RedirectToAction("ViewCart", "Cart");
            }
            else
            {
                // Default: redirect to checkout
                return RedirectToAction("Index", "Checkout");
            }
        }

        public IActionResult ViewCart()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            var items = _context.CartItems.Where(c => c.UserId == userId.Value).ToList();
            foreach (var it in items) { it.Product = _context.Products.Find(it.ProductId); }
            return View(items);
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int cartItemId, int quantity)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return Json(new { success = false, message = "Not authenticated" });

            var cartItem = _context.CartItems
                .FirstOrDefault(c => c.CartItemId == cartItemId && c.UserId == userId.Value);

            if (cartItem == null)
                return Json(new { success = false, message = "Item not found" });

            if (quantity <= 0)
            {
                _context.CartItems.Remove(cartItem);
            }
            else
            {
                cartItem.Quantity = quantity;
                _context.CartItems.Update(cartItem);
            }

            _context.SaveChanges();

            // Return updated subtotal
            var items = _context.CartItems.Where(c => c.UserId == userId.Value).ToList();
            foreach (var it in items) { it.Product = _context.Products.Find(it.ProductId); }
            
            var subtotal = items.Sum(x => x.Quantity * (x.Product?.Price ?? 0));
            var tax = subtotal * 0.05m;
            var total = subtotal + tax;

            return Json(new 
            { 
                success = true, 
                message = "Quantity updated",
                subtotal = subtotal.ToString("0.00"),
                tax = tax.ToString("0.00"),
                total = total.ToString("0.00"),
                itemSubtotal = (cartItem.Quantity * (cartItem.Product?.Price ?? 0)).ToString("0.00")
            });
        }

        [HttpPost]
        public IActionResult RemoveItem(int cartItemId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return Json(new { success = false, message = "Not authenticated" });

            var cartItem = _context.CartItems
                .FirstOrDefault(c => c.CartItemId == cartItemId && c.UserId == userId.Value);

            if (cartItem == null)
                return Json(new { success = false, message = "Item not found" });

            _context.CartItems.Remove(cartItem);
            _context.SaveChanges();

            // Return updated cart
            var items = _context.CartItems.Where(c => c.UserId == userId.Value).ToList();
            foreach (var it in items) { it.Product = _context.Products.Find(it.ProductId); }
            
            var subtotal = items.Sum(x => x.Quantity * (x.Product?.Price ?? 0));
            var tax = subtotal * 0.05m;
            var total = subtotal + tax;

            return Json(new 
            { 
                success = true, 
                message = "Item removed from cart",
                itemCount = items.Count,
                subtotal = subtotal.ToString("0.00"),
                tax = tax.ToString("0.00"),
                total = total.ToString("0.00")
            });
        }
    }
}
