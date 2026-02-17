using CommunityToolkit.Maui.Views;
using WebshopMobileApp.Models;

namespace WebshopMobileApp.Pages.Controls;

public partial class PopupSingleProduct : Popup
{
    public CartModel Cart { get; }
    public PopupSingleProduct(CartModel model)
	{
		InitializeComponent();
        Cart = model;
        this.BindingContext = Cart;
    }
   
}