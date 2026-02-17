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
        public int ProductServerId { get; set; } 
    }

    public class Category 
    {
        [Key]
        public int Id { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? FileUrl { get; set; }
    }

    public class TblTypicalOrdersSet
    {
        public int Id { get; set; }

        public byte[] RowVersion { get; set; } = null!;

        public decimal? Qty { get; set; }
        public decimal? qtyOnHand { get; set; }

        public int? PushType { get; set; }

        public string? ProductCode { get; set; }

        public string? ProductDesc { get; set; }

        public string? CustomerCode { get; set; }

        public string? CustomerDesc { get; set; }

        public decimal? CommissionPayable { get; set; }

        public int TblTypicalOrdersTblProducts { get; set; }

        public int TblTypicalOrdersTblCustomer { get; set; }

        //public virtual TblCustomer TblTypicalOrdersTblCustomerNavigation { get; set; } = null!;

        //public virtual TblProductsSet TblTypicalOrdersTblProductsNavigation { get; set; } = null!;
    }
}
