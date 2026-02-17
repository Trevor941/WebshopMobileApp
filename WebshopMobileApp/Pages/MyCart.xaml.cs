using CommunityToolkit.Maui.Views;
using WebshopMobileApp.Pages.TabbedPages;

namespace WebshopMobileApp.Pages;

public partial class MyCart : ContentPage
{
	public MyCart(MyCartPageModel model)
	{
		InitializeComponent();
        BindingContext = model;
    }


    //async void OnOpenModalButtonClicked(object sender, EventArgs e)
    //{
    //    await Navigation.PushModalAsync(new Favorites());
    //}

    //private async void Button_Clicked(object sender, EventArgs e)
    //{
    //   // await Navigation.PushModalAsync(new Favorites());
    //    await this.ShowPopupAsync(new Favorites());
    //}
}