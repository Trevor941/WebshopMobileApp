using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebshopMobileApp.Models
{
    public class ProductsWithQuantity : ProductModel
    {
        public int Quantity { get; set; } = 1;
        public bool IsPromoted { get; set; } = false;
        public string? FileUrl { get; set; }
        public int? ProductServerId { get; set; } 
    }

    public class Category 
    {
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? FileUrl { get; set; }
    }
}
