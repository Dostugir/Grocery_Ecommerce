using System.ComponentModel.DataAnnotations;

namespace Grocery_Ecommerce.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; } = "Pending";
        public DateTime? DeliverySlotStart { get; set; }
        public DateTime? DeliverySlotEnd { get; set; }
        public int DeliveryAddressId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<OrderItem> Items { get; set; } = new();
    }
}
