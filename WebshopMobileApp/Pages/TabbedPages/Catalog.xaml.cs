namespace WebshopMobileApp.Pages.TabbedPages;

public partial class Catalog : ContentPage
{
	public Catalog(ProductsListPageModel model)
	{
		InitializeComponent();
        BindingContext = model;
    }
}