using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Microsoft.Maui.Controls.PlatformConfiguration;
namespace WebshopMobileApp.Pages.TabbedPages;

public partial class TabbedMainPage : Microsoft.Maui.Controls.TabbedPage
{
    private readonly ProductRepository _productRepository;
    public TabbedMainPage()
	{
		InitializeComponent();
        
        // Put tabs at the bottom on Android
        On<Microsoft.Maui.Controls.PlatformConfiguration.Android>()
            .SetToolbarPlacement(ToolbarPlacement.Bottom);

        // Optional: fixed tabs (don’t scroll)
        On<Microsoft.Maui.Controls.PlatformConfiguration.Android>()
            .SetIsSwipePagingEnabled(false);
    }
}