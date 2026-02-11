namespace WebshopMobileApp.Pages.TabbedPages;

public partial class Catalog : ContentPage
{
	public Catalog(ProductsListPageModel model)
	{
		InitializeComponent();
        BindingContext = model;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (BindingContext is ProductsListPageModel vm && query.ContainsKey("categoryId"))
        {
            vm.LoadProductsByCategory(query["categoryId"]?.ToString());
        }
    }
}