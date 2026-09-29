using BCrypt.Net;
using Grocery_Ecommerce.Data;
using Grocery_Ecommerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Grocery_Ecomerce.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        private int? GetUserId()
        {
            return HttpContext.Session.GetInt32("UserId");
        }

        // Profile Page
        [HttpGet]
        public IActionResult Profile()
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            var user = _context.Users.Find(userId.Value);
            if (user == null)
                return NotFound();

            var addresses = _context.Addresses
                .Where(a => a.UserId == userId.Value)
                .ToList();

            var model = new UserProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                CreatedAt = user.CreatedAt,
                Addresses = addresses
            };

            return View(model);
        }

        // Update Profile
        [HttpPost]
        public IActionResult UpdateProfile(UserProfileViewModel model)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            var user = _context.Users.Find(userId.Value);
            if (user == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                model.Addresses = _context.Addresses
                    .Where(a => a.UserId == userId.Value)
                    .ToList();
                return View("Profile", model);
            }

            // Check if email is already in use by another user
            if (user.Email != model.Email && _context.Users.Any(u => u.Email == model.Email && u.UserId != userId.Value))
            {
                ModelState.AddModelError("Email", "This email is already in use");
                model.Addresses = _context.Addresses.Where(a => a.UserId == userId.Value).ToList();
                return View("Profile", model);
            }

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            _context.Users.Update(user);
            _context.SaveChanges();

            HttpContext.Session.SetString("UserName", user.FullName);

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }

        // Change Password
        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            var user = _context.Users.Find(userId.Value);
            if (user == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                var profileModel = new UserProfileViewModel
                {
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    CreatedAt = user.CreatedAt,
                    Addresses = _context.Addresses.Where(a => a.UserId == userId.Value).ToList()
                };
                return View("Profile", profileModel);
            }

            // Verify current password
            bool isValidPassword = user.Password.StartsWith("$2a$")
                ? BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.Password)
                : user.Password == model.CurrentPassword;

            if (!isValidPassword)
            {
                ModelState.AddModelError("CurrentPassword", "Current password is incorrect");
                var profileModel = new UserProfileViewModel
                {
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    CreatedAt = user.CreatedAt,
                    Addresses = _context.Addresses.Where(a => a.UserId == userId.Value).ToList()
                };
                return View("Profile", profileModel);
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            _context.Users.Update(user);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Password changed successfully!";
            return RedirectToAction("Profile");
        }

        // Add/Edit Address
        [HttpGet]
        public IActionResult AddressForm(int? id)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            if (id.HasValue)
            {
                var address = _context.Addresses
                    .FirstOrDefault(a => a.AddressId == id.Value && a.UserId == userId.Value);
                
                if (address == null)
                    return NotFound();

                var model = new AddressViewModel
                {
                    AddressId = address.AddressId,
                    Street = address.Street,
                    City = address.City,
                    State = address.State,
                    PostalCode = address.PostalCode,
                    Country = address.Country,
                    IsDefault = address.IsDefault
                };

                return PartialView("_AddressForm", model);
            }

            return PartialView("_AddressForm", new AddressViewModel());
        }

        [HttpPost]
        public IActionResult SaveAddress(AddressViewModel model)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            if (model.AddressId.HasValue)
            {
                // Edit existing address
                var address = _context.Addresses
                    .FirstOrDefault(a => a.AddressId == model.AddressId.Value && a.UserId == userId.Value);
                
                if (address == null)
                    return NotFound();

                address.Street = model.Street;
                address.City = model.City;
                address.State = model.State;
                address.PostalCode = model.PostalCode;
                address.Country = model.Country;
                address.UpdatedAt = DateTime.Now;

                if (model.IsDefault)
                {
                    // Remove default from other addresses
                    var otherAddresses = _context.Addresses
                        .Where(a => a.UserId == userId.Value && a.AddressId != address.AddressId)
                        .ToList();
                    foreach (var addr in otherAddresses)
                        addr.IsDefault = false;
                }

                address.IsDefault = model.IsDefault;
                _context.Addresses.Update(address);
            }
            else
            {
                // Add new address
                var newAddress = new Address
                {
                    UserId = userId.Value,
                    Street = model.Street,
                    City = model.City,
                    State = model.State,
                    PostalCode = model.PostalCode,
                    Country = model.Country,
                    IsDefault = model.IsDefault,
                    CreatedAt = DateTime.Now
                };

                if (model.IsDefault)
                {
                    var otherAddresses = _context.Addresses
                        .Where(a => a.UserId == userId.Value)
                        .ToList();
                    foreach (var addr in otherAddresses)
                        addr.IsDefault = false;
                }

                _context.Addresses.Add(newAddress);
            }

            _context.SaveChanges();

            return Json(new { success = true, message = model.AddressId.HasValue ? "Address updated successfully!" : "Address added successfully!" });
        }

        [HttpPost]
        public IActionResult DeleteAddress(int id)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var address = _context.Addresses
                .FirstOrDefault(a => a.AddressId == id && a.UserId == userId.Value);
            
            if (address == null)
                return NotFound();

            _context.Addresses.Remove(address);
            _context.SaveChanges();

            return Json(new { success = true, message = "Address deleted successfully!" });
        }

        [HttpPost]
        public IActionResult SetDefaultAddress(int id)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var address = _context.Addresses
                .FirstOrDefault(a => a.AddressId == id && a.UserId == userId.Value);
            
            if (address == null)
                return NotFound();

            try
            {
                // Remove default from other addresses
                var otherAddresses = _context.Addresses
                    .Where(a => a.UserId == userId.Value && a.AddressId != id)
                    .ToList();
                
                foreach (var addr in otherAddresses)
                {
                    addr.IsDefault = false;
                    _context.Addresses.Update(addr);
                }

                // Set this address as default
                address.IsDefault = true;
                address.UpdatedAt = DateTime.Now;
                _context.Addresses.Update(address);
                
                // Save all changes
                _context.SaveChanges();

                return Json(new { success = true, message = "Default address updated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating default address: " + ex.Message });
            }
        }
    }
}
