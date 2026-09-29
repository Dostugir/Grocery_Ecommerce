using Grocery_Ecommerce.Models;
using BCrypt.Net;

namespace Grocery_Ecommerce.Data
{
    public static class DbInitializer
    {
        public static void Seed(AppDbContext context)
        {
            // Ensure database is created
            context.Database.EnsureCreated();

            // Seed Admin user
            if (!context.AdminUsers.Any())
            {
                var admin = new AdminUser
                {
                    FullName = "Super Admin",
                    Email = "admin@grocery.local",
                    Password = BCrypt.Net.BCrypt.HashPassword("Admin@123")
                };
                context.AdminUsers.Add(admin);
            }

            // Seed Categories
            if (!context.Categories.Any())
            {
                var cats = new List<Category>
                {
                    new Category { Name = "Fruits", Description = "Fresh fruits" },
                    new Category { Name = "Vegetables", Description = "Fresh vegetables" },
                    new Category { Name = "Dairy", Description = "Milk, cheese and more" }
                };
                context.Categories.AddRange(cats);
            }

            context.SaveChanges();

            // Seed Products
            if (!context.Products.Any())
            {
                var fruits = context.Categories.FirstOrDefault(c => c.Name == "Fruits");
                var veg = context.Categories.FirstOrDefault(c => c.Name == "Vegetables");
                var dairy = context.Categories.FirstOrDefault(c => c.Name == "Dairy");

                var products = new List<Product>
                {
                    new Product { Name = "Banana", Price = 30m, UnitType = "kg", CategoryId = fruits?.CategoryId ?? 0, ImageUrl = "https://via.placeholder.com/200" },
                    new Product { Name = "Apple", Price = 120m, UnitType = "kg", CategoryId = fruits?.CategoryId ?? 0, ImageUrl = "https://via.placeholder.com/200" },
                    new Product { Name = "Carrot", Price = 40m, UnitType = "kg", CategoryId = veg?.CategoryId ?? 0, ImageUrl = "https://via.placeholder.com/200" },
                    new Product { Name = "Milk", Price = 80m, UnitType = "liter", CategoryId = dairy?.CategoryId ?? 0, ImageUrl = "https://via.placeholder.com/200" }
                };

                // Only add products if their category ids are valid
                foreach (var p in products)
                {
                    if (p.CategoryId > 0) context.Products.Add(p);
                }
                context.SaveChanges();
            }

            // Seed Product Reviews (optional demo)
            if (!context.ProductReviews.Any() && context.Products.Any() && context.Users.Any())
            {
                var user = context.Users.FirstOrDefault();
                var product = context.Products.FirstOrDefault();
                if (user != null && product != null)
                {
                    context.ProductReviews.Add(new ProductReview
                    {
                        ProductId = product.ProductId,
                        UserId = user.UserId,
                        Rating = 5,
                        Comment = "Great product! Highly recommended.",
                        CreatedAt = DateTime.UtcNow
                    });
                    context.SaveChanges();
                }
            }
        }
    }
}
