using System.ComponentModel.DataAnnotations;
using Grocery_Ecommerce.Models;

namespace Grocery_Ecommerce.Models
{
    public class CartItem
    {
        [Key]
        public int CartItemId { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }

        public Product? Product { get; set; }
    }
}
