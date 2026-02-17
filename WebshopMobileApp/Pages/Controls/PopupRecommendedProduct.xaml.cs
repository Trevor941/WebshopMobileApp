using CommunityToolkit.Maui.Views;
using WebshopMobileApp.Models;

namespace WebshopMobileApp.Pages.Controls;

public partial class PopupRecommendedProduct : Popup
{
    public ProductsWithQuantity Product { get; }
    public PopupRecommendedProduct(ProductsWithQuantity model)
    {
        InitializeComponent();
        Product = model;
        this.BindingContext = Product;
    }
}