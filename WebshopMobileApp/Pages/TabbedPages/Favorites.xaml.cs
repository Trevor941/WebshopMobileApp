
namespace WebshopMobileApp.Pages.TabbedPages;

public partial class Favorites : ContentPage
{
	public Favorites(FavouriteListPageModel model)
	{
		InitializeComponent();
        BindingContext = model;
    }
}