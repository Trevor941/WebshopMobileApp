using WebshopMobileApp.Models;
using WebshopMobileApp.PageModels;

namespace WebshopMobileApp.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}