using Grocery_Ecommerce.Data;
using Grocery_Ecommerce.Models;
using Grocery_Ecommerce.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Grocery_Ecomerce.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public CheckoutController(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            var items = _context.CartItems.Where(c => c.UserId == userId.Value).ToList();
            foreach (var it in items) it.Product = _context.Products.Find(it.ProductId);

            var total = items.Sum(x => x.Quantity * (x.Product?.Price ?? 0));
            ViewBag.Total = total;

            // Get user's addresses
            var addresses = _context.Addresses.Where(a => a.UserId == userId.Value).ToList();
            ViewBag.Addresses = addresses;

            return View(items);
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(int deliveryAddressId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            var items = _context.CartItems.Where(c => c.UserId == userId.Value).ToList();
            if (!items.Any()) return RedirectToAction("ViewCart", "Cart");

            // Validate delivery address exists
            if (deliveryAddressId <= 0)
            {
                TempData["Error"] = "Please select a valid delivery address.";
                return RedirectToAction("Index");
            }

            decimal total = 0;
            var orderItems = new List<OrderItem>();
            var orderItemsList = new List<string>();

            foreach (var ci in items)
            {
                var p = _context.Products.Find(ci.ProductId);
                if (p == null) continue;
                var oi = new OrderItem { ProductId = p.ProductId, Quantity = ci.Quantity, Price = p.Price };
                total += oi.Quantity * oi.Price;
                orderItems.Add(oi);
                orderItemsList.Add($"{p.Name} x{ci.Quantity} - ${(oi.Quantity * oi.Price):F2}");
            }

            decimal totalWithTax = total * 1.05m; // Add 5% tax

            var order = new Order
            {
                UserId = userId.Value,
                OrderDate = DateTime.Now,
                CreatedAt = DateTime.Now,
                TotalAmount = totalWithTax,
                OrderStatus = "Pending",
                DeliveryAddressId = deliveryAddressId
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            // Add order items after order is created
            foreach (var oi in orderItems)
            {
                oi.OrderId = order.OrderId;
                _context.OrderItems.Add(oi);
            }

            _context.CartItems.RemoveRange(items);
            _context.SaveChanges();

            // Send order confirmation email
            try
            {
                var user = _context.Users.Find(userId.Value);
                if (user != null)
                {
                    var emailSubject = $"Order Confirmation #{order.OrderId} 🎉";
                    var emailBody = EmailTemplates.OrderConfirmationTemplate(order.OrderId, order.TotalAmount, orderItemsList, user.FullName);
                    await _emailService.SendEmailAsync(user.Email, emailSubject, emailBody);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending order confirmation email: {ex.Message}");
            }

            return RedirectToAction("Confirmation", new { id = order.OrderId });
        }

        public IActionResult Confirmation(int id)
        {
            var order = _context.Orders.Find(id);
            if (order == null) return NotFound();
            var items = _context.OrderItems.Where(oi => oi.OrderId == id).ToList();
            foreach (var it in items) it.Product = _context.Products.Find(it.ProductId);
            ViewBag.Items = items;
            return View(order);
        }
    }
}
