using Grocery_Ecommerce.Data;
using Grocery_Ecommerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Grocery_Ecommerce.Filters;

namespace Grocery_Ecomerce.Controllers
{
    [AdminAuthorize]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var model = new AdminDashboardViewModel
            {
                UsersCount = _context.Users.Count(),
                ProductsCount = _context.Products.Count(),
                CategoriesCount = _context.Categories.Count(),
                RecentOrders = new List<string>() // placeholder
            };

            return View(model);
        }

        // Category CRUD
        public IActionResult Categories()
        {
            return View(_context.Categories.ToList());
        }

        [HttpGet]
        public IActionResult CreateCategory() { return View(); }

        [HttpPost]
        public IActionResult CreateCategory(Category model)
        {
            if (!ModelState.IsValid) return View(model);
            _context.Categories.Add(model);
            _context.SaveChanges();
            return RedirectToAction("Categories");
        }

        [HttpGet]
        public IActionResult EditCategory(int id)
        {
            var c = _context.Categories.Find(id);
            if (c == null) return NotFound();
            return View(c);
        }

        [HttpPost]
        public IActionResult EditCategory(Category model)
        {
            if (!ModelState.IsValid) return View(model);
            _context.Categories.Update(model);
            _context.SaveChanges();
            return RedirectToAction("Categories");
        }

        [HttpGet]
        public IActionResult DeleteCategory(int id)
        {
            var c = _context.Categories.Find(id);
            if (c == null) return NotFound();
            return View(c);
        }

        [HttpPost]
        public IActionResult DeleteCategoryConfirmed(int id)
        {
            var c = _context.Categories.Find(id);
            if (c != null)
            {
                _context.Categories.Remove(c);
                _context.SaveChanges();
            }
            return RedirectToAction("Categories");
        }

        // Product CRUD
        public IActionResult Products() { return View(_context.Products.Include(p => p.Category).ToList()); }

        [HttpGet]
        public IActionResult CreateProduct() { ViewBag.Categories = _context.Categories.ToList(); return View(); }

        [HttpPost]
        public IActionResult CreateProduct(Product model)
        {
            if (!ModelState.IsValid) { ViewBag.Categories = _context.Categories.ToList(); return View(model); }
            _context.Products.Add(model);
            _context.SaveChanges();
            return RedirectToAction("Products");
        }

        [HttpGet]
        public IActionResult EditProduct(int id)
        {
            var p = _context.Products.Find(id);
            if (p == null) return NotFound();
            ViewBag.Categories = _context.Categories.ToList();
            return View(p);
        }

        [HttpPost]
        public IActionResult EditProduct(Product model)
        {
            if (!ModelState.IsValid) { ViewBag.Categories = _context.Categories.ToList(); return View(model); }
            _context.Products.Update(model);
            _context.SaveChanges();
            return RedirectToAction("Products");
        }

        [HttpGet]
        public IActionResult DeleteProduct(int id)
        {
            var p = _context.Products.Include(x => x.Category).FirstOrDefault(x => x.ProductId == id);
            if (p == null) return NotFound();
            return View(p);
        }

        [HttpPost]
        public IActionResult DeleteProductConfirmed(int id)
        {
            var p = _context.Products.Find(id);
            if (p != null)
            {
                _context.Products.Remove(p);
                _context.SaveChanges();
            }
            return RedirectToAction("Products");
        }

        // User Management
        public IActionResult Users()
        {
            return View(_context.Users.ToList());
        }

        // Settings
        public IActionResult Settings()
        {
            return View();
        }

        // Order Management
        public IActionResult Orders()
        {
            var orders = _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .ToList();
            return View(orders);
        }

        public IActionResult OrderDetails(int id)
        {
            var order = _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefault(o => o.OrderId == id);
            if (order == null) return NotFound();
            return View(order);
        }

        [HttpPost]
        public IActionResult UpdateOrderStatus(int orderId, string status)
        {
            var order = _context.Orders.Find(orderId);
            if (order == null)
                return Json(new { success = false, message = "Order not found" });

            var validStatuses = new[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };
            if (!validStatuses.Contains(status))
                return Json(new { success = false, message = "Invalid status" });

            order.OrderStatus = status;
            _context.Orders.Update(order);
            _context.SaveChanges();

            return Json(new { success = true, message = $"Order status updated to {status}" });
        }
    }
}
