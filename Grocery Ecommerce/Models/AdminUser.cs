using System.ComponentModel.DataAnnotations;

namespace Grocery_Ecommerce.Models
{
    public class AdminUser
    {
        [Key]
        public int AdminId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } = "Admin";
    }
}
