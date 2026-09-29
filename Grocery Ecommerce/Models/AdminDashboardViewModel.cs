namespace Grocery_Ecommerce.Models
{
    public class AdminDashboardViewModel
    {
        public int UsersCount { get; set; }
        public int ProductsCount { get; set; }
        public int CategoriesCount { get; set; }
        public List<string> RecentOrders { get; set; } = new();
    }
}
