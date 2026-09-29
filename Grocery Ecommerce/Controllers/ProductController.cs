using Grocery_Ecommerce.Data;
using Grocery_Ecommerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Grocery_Ecomerce.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Details(int id)
        {
            var product = _context.Products.Include(p => p.Category).FirstOrDefault(p => p.ProductId == id);
            if (product == null) return NotFound();
            var reviews = _context.ProductReviews.Include(r => r.User).Where(r => r.ProductId == id).OrderByDescending(r => r.CreatedAt).ToList();
            ViewBag.Reviews = reviews;
            ViewBag.AvgRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;
            return View(product);
        }

        [HttpPost]
        public IActionResult AddReview(int productId, int rating, string comment)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["ReviewMessage"] = "You must be logged in to leave a review.";
                return RedirectToAction("Details", new { id = productId });
            }
            if (rating < 1 || rating > 5 || string.IsNullOrWhiteSpace(comment))
            {
                TempData["ReviewMessage"] = "Invalid review data.";
                return RedirectToAction("Details", new { id = productId });
            }
            var review = new ProductReview
            {
                ProductId = productId,
                UserId = userId.Value,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.UtcNow
            };
            _context.ProductReviews.Add(review);
            _context.SaveChanges();
            TempData["ReviewMessage"] = "Thank you for your review!";
            return RedirectToAction("Details", new { id = productId });
        }

        public IActionResult List(int? categoryId, string search = null)
        {
            var products = _context.Products.Include(p => p.Category).AsQueryable();
            if (categoryId.HasValue)
                products = products.Where(p => p.CategoryId == categoryId.Value);
            if (!string.IsNullOrWhiteSpace(search))
                products = products.Where(p => p.Name.Contains(search) || (p.Description != null && p.Description.Contains(search)));
            return View(products.ToList());
        }
    }
}
