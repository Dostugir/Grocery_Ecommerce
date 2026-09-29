using BCrypt.Net;
using Grocery_Ecommerce.Models;
using Grocery_Ecommerce.Data;
using Grocery_Ecommerce.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Grocery_Ecomerce.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public AccountController(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 1. Check Admin
            var admin = _context.AdminUsers.FirstOrDefault(a => a.Email == model.Email);
            if (admin != null && !string.IsNullOrEmpty(admin.Password))
            {
                // Check password (hash or plain)
                bool isValid = admin.Password.StartsWith("$2a$")
                    ? BCrypt.Net.BCrypt.Verify(model.Password, admin.Password)
                    : admin.Password == model.Password;

                if (isValid)
                {
                    HttpContext.Session.SetInt32("AdminId", admin.AdminId);
                    HttpContext.Session.SetString("UserName", admin.FullName);
                    HttpContext.Session.SetString("Role", "Admin");
                    return RedirectToAction("Index", "Home");
                }
            }

            // 2. Check Customer
            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
            if (user != null)
            {
                bool isValid = user.Password.StartsWith("$2a$")
                    ? BCrypt.Net.BCrypt.Verify(model.Password, user.Password)
                    : user.Password == model.Password;

                if (isValid)
                {
                    HttpContext.Session.SetInt32("UserId", user.UserId);
                    HttpContext.Session.SetString("UserName", user.FullName);
                    HttpContext.Session.SetString("Role", "Customer");
                    return RedirectToAction("Index", "Home");
                }
            }

            // If we got here, login failed
            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // prevent duplicates
            if (_context.Users.Any(u => u.Email == model.Email) || _context.AdminUsers.Any(a => a.Email == model.Email))
            {
                ModelState.AddModelError("", "Email already in use");
                return View(model);
            }

            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password)
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            // Send welcome email
            try
            {
                var emailSubject = "Welcome to Grocery Store! 🎉";
                var emailBody = EmailTemplates.WelcomeEmailTemplate(model.FullName, model.Email);
                await _emailService.SendEmailAsync(model.Email, emailSubject, emailBody);
            }
            catch (Exception ex)
            {
                // Log error but don't block user registration
                Console.WriteLine($"Error sending welcome email: {ex.Message}");
            }

            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("Role", "Customer");

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AdminLogin()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AdminLogin(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var admin = _context.AdminUsers.FirstOrDefault(a => a.Email == model.Email);
            if (admin != null && !string.IsNullOrEmpty(admin.Password))
            {
                bool isValid = admin.Password.StartsWith("$2a$")
                    ? BCrypt.Net.BCrypt.Verify(model.Password, admin.Password)
                    : admin.Password == model.Password;

                if (isValid)
                {
                    HttpContext.Session.SetInt32("AdminId", admin.AdminId);
                    HttpContext.Session.SetString("UserName", admin.FullName);
                    HttpContext.Session.SetString("Role", "Admin");
                    return RedirectToAction("Index", "Admin");
                }
            }
            ModelState.AddModelError("", "Invalid admin login attempt.");
            return View(model);
        }

        [HttpGet]
        public IActionResult AdminRegister()
        {
            // Allow access to admin registration without requiring login
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AdminRegister(AdminRegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (_context.AdminUsers.Any(a => a.Email == model.Email))
            {
                ModelState.AddModelError("", "Email already in use");
                return View(model);
            }

            var admin = new AdminUser
            {
                FullName = model.FullName,
                Email = model.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password)
            };
            _context.AdminUsers.Add(admin);
            _context.SaveChanges();

            // Send welcome email to admin
            try
            {
                var emailSubject = "Admin Account Created Successfully 🎉";
                var emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; }}
        .container {{ max-width: 600px; margin: 0 auto; }}
        .header {{ background: linear-gradient(135deg, #27ae60 0%, #229954 100%); color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: white; padding: 20px; border-radius: 0 0 8px 8px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'><h1>Admin Account Created</h1></div>
        <div class='content'>
            <p>Hello {model.FullName},</p>
            <p>Your admin account has been successfully created!</p>
            <p><strong>Email:</strong> {model.Email}</p>
            <p>You can now log in to the admin dashboard and start managing the grocery store.</p>
        </div>
    </div>
</body>
</html>";
                await _emailService.SendEmailAsync(model.Email, emailSubject, emailBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending admin welcome email: {ex.Message}");
            }

            TempData["AdminRegisterSuccess"] = "Admin registered successfully.";
            return RedirectToAction("AdminLogin");
        }
    }
}