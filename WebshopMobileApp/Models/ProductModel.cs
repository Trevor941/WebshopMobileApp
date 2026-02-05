using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebshopMobileApp.Models
{
    public class ProductModel 
    {
        [Key]
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal QuantityOnHand { get; set; } = 0;
        public bool HasImage { get; set; } = false;
        public byte[]? Image { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Price { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal PriceIncl { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public bool OnSpecial { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? SpecialPrice { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? TypicalOrderQuantity { get; set; }
        public decimal TaxPercentage { get; set; }

        public string? UOM { get; set; }

        public string? Category1 { get; set; }
        public string? Category2 { get; set; }
        public string? Category3 { get; set; }
        public string? Category4 { get; set; }
        public string? Category5 { get; set; }
        public string? Category6 { get; set; }
        public string? Category7 { get; set; }
        public string? Category8 { get; set; }
        public bool isFavoured { get; set; }
        public bool InfoApproved { get; set; }
        public int CategoryId { get; set; }
        public string Category { get; set; } = string.Empty;
    }
}
