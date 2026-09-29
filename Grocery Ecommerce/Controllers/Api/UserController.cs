using Grocery_Ecommerce.Data;
using Grocery_Ecommerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Grocery_Ecomerce.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _context.Users.Select(u => new { u.UserId, u.FullName, u.Email, u.CreatedAt }).ToList();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var u = _context.Users.Find(id);
            if (u == null) return NotFound();
            return Ok(new { u.UserId, u.FullName, u.Email, u.CreatedAt });
        }

        [HttpPost]
        public IActionResult Create(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (_context.Users.Any(x => x.Email == model.Email) || _context.AdminUsers.Any(a => a.Email == model.Email))
                return Conflict("Email already taken");

            var user = new User { FullName = model.FullName, Email = model.Email, Password = BCrypt.Net.BCrypt.HashPassword(model.Password) };
            _context.Users.Add(user);
            _context.SaveChanges();
            return CreatedAtAction(nameof(Get), new { id = user.UserId }, new { user.UserId, user.FullName, user.Email });
        }
    }
}
