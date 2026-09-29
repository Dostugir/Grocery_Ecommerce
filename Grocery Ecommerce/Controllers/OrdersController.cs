using Grocery_Ecommerce.Data;
using Microsoft.AspNetCore.Mvc;

namespace Grocery_Ecommerce.Controllers
{
    public class OrdersController : Controller
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            var orders = _context.Orders
                .Where(o => o.UserId == userId.Value)
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

            return View(orders);
        }

        public IActionResult Details(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            var order = _context.Orders.Find(id);
            if (order == null || order.UserId != userId.Value)
                return NotFound();

            var items = _context.OrderItems
                .Where(oi => oi.OrderId == id)
                .ToList();

            foreach (var it in items)
                it.Product = _context.Products.Find(it.ProductId);

            ViewBag.Items = items;
            return View(order);
        }

        [HttpPost]
        public IActionResult ReorderItems(int orderId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return Json(new { success = false, message = "Please log in first" });

            var order = _context.Orders.Find(orderId);
            if (order == null || order.UserId != userId.Value)
                return Json(new { success = false, message = "Order not found" });

            var items = _context.OrderItems
                .Where(oi => oi.OrderId == orderId)
                .ToList();

            if (!items.Any())
                return Json(new { success = false, message = "No items in this order" });

            try
            {
                // Add items to cart
                foreach (var item in items)
                {
                    var existingCartItem = _context.CartItems
                        .FirstOrDefault(c => c.UserId == userId.Value && c.ProductId == item.ProductId);

                    if (existingCartItem != null)
                    {
                        existingCartItem.Quantity += item.Quantity;
                        _context.CartItems.Update(existingCartItem);
                    }
                    else
                    {
                        var cartItem = new Grocery_Ecommerce.Models.CartItem
                        {
                            UserId = userId.Value,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity
                        };
                        _context.CartItems.Add(cartItem);
                    }
                }

                _context.SaveChanges();

                return Json(new { 
                    success = true, 
                    message = $"{items.Count} item(s) added to cart",
                    redirectUrl = "/Cart/ViewCart"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error adding items to cart: " + ex.Message });
            }
        }
    }
}
