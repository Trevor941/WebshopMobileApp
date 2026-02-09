using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebshopMobileApp.Models
{
    public class ProductsWithQuantity : ProductModel, INotifyPropertyChanged
    {
       // public int Quantity { get; set; } = 1;
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
