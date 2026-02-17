using WebshopMobileApp.Models;

namespace WebshopMobileApp.Pages.Controls;

public partial class RecommendedProductsView 
{
	public RecommendedProductsView()
	{
		InitializeComponent();
    }
    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        this.ForceLayout();  // forces MAUI to recalc layout immediately
    }
}