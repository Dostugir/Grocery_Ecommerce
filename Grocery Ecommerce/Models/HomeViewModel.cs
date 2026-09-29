using System.Collections.Generic;

namespace Grocery_Ecommerce.Models
{
    public class HomeViewModel
    {
        public List<Category> Categories { get; set; } = new();
        public List<Product> Products { get; set; } = new();
        // Filtering and sorting
        public int? SelectedCategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string SortBy { get; set; } // e.g. "price_asc", "price_desc", "name_asc", "name_desc"
        public string SearchQuery { get; set; }
    }
}
