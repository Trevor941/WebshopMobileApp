namespace WebshopMobileApp.Pages;

public partial class MyCart : ContentPage
{
	public MyCart(MyCartPageModel model)
	{
		InitializeComponent();
        BindingContext = model;
    }
}