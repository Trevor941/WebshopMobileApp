using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebshopMobileApp.Models
{
    public class CartModel : INotifyPropertyChanged
    {
        [Key]
        public int Id { get; set; }
        public int ProductServerId { get; set; }
        public string? ProductCode { get; set; } = "";
        private int quantity;
        public int Quantity
        {
            get => quantity;
            set
            {
                if (quantity != value)
                {
                    quantity = value;
                    OnPropertyChanged(nameof(Quantity));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        //[Column(TypeName = "decimal(18,2)")]
        public decimal? Price { get; set; }
        //[Column(TypeName = "decimal(18,2)")]
        public decimal? PriceIncl { get; set; }

        public bool HasImage { get; set; } = false;
        public string FileUrl { get; set; } = "";
        public string Description { get; set; } = "";
        public string UnitOfSale { get; set; } = "";
        public decimal TaxPercentage { get; set; }
        public decimal TotalInc { get; set; } = 0;
        public decimal lineTotal { get; set; } = 0;
        public decimal NettPrice { get; set; } = 0;
        public decimal VatTotal { get; set; } = 0;

    }

    public class CartModelToPost 
    {
        [Key]
        public int Id { get; set; }
        public int ProductID { get; set; }
        public string? ProductCode { get; set; } = "";
        public int Quantity { get; set; } = 1;
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal PriceIncl { get; set; }

        public bool HasImage { get; set; } = false;
        public string Description { get; set; } = "";
        public string UnitOfSale { get; set; } = "";
        public decimal TaxPercentage { get; set; }
        public decimal TotalInc { get; set; } = 0;
        public decimal lineTotal { get; set; } = 0;
        public decimal NettPrice { get; set; } = 0;
        public decimal VatTotal { get; set; } = 0;
    }
}
