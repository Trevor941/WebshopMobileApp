//using static Android.Graphics.ColorSpace;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
namespace WebshopMobileApp.Pages;

public partial class LoginPage : ContentPage
{

    public LoginPage(LoginPageModel model)
    {
		InitializeComponent();
        BindingContext = model;
    }
    //private async void btnLogin_Clicked(object sender, EventArgs e)
    //{
    //    var mainPage = new MainPageModel();
    //    await Navigation.PushAsync(new MainPage(mainPage));
    //}

}