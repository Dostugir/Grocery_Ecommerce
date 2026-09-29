using System.ComponentModel.DataAnnotations;

namespace Grocery_Ecommerce.Models
{
    public class OrderItem
    {
        [Key]
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }

        public Product? Product { get; set; }
    }
}
