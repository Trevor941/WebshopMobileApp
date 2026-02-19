using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using WebshopMobileApp.Pages.TabbedPages;
using Font = Microsoft.Maui.Font;

namespace WebshopMobileApp
{
    public partial class AppShell : Shell
    {
        private int _cartItemCount = 0;
        private readonly ProductRepository _productRepository;
        private readonly CartRepository _cardRepository;
        private readonly ProductsListPageModel _model;
        public AppShell(ProductRepository productRepository, ProductsListPageModel model, CartRepository cardRepository)
        {
            InitializeComponent();
            UpdateCartCount(0);
            var currentTheme = Application.Current!.RequestedTheme;
            ThemeSegmentedControl.SelectedIndex = currentTheme == AppTheme.Light ? 0 : 1;
            _productRepository = productRepository;
            _model = model;
            _cardRepository = cardRepository;
            GetCartItems();
            LoadCategories();
        }
        public static async Task DisplaySnackbarAsync(string message)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            var snackbarOptions = new SnackbarOptions
            {
                BackgroundColor = Color.FromArgb("#FF3300"),
                TextColor = Colors.White,
                ActionButtonTextColor = Colors.Yellow,
                CornerRadius = new CornerRadius(0),
                Font = Font.SystemFontOfSize(18),
                ActionButtonFont = Font.SystemFontOfSize(14)
            };

            var snackbar = Snackbar.Make(message, visualOptions: snackbarOptions);

            await snackbar.Show(cancellationTokenSource.Token);
        }
        private async Task GetCartItems()
        {
            try
            {
                var Cart = await _cardRepository.GetCartData();
                if (Cart.Count > 0)
                {
                     UpdateCartCount(Cart.Sum(p => p.Quantity));
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public void UpdateCartCount(int count)
        {
            _cartItemCount = count;

            CartCountLabel.Text = count.ToString();
            CartBadge.IsVisible = count > 0;
        }
        public static async Task DisplayToastAsync(string message)
        {
            // Toast is currently not working in MCT on Windows
            if (OperatingSystem.IsWindows())
                return;

            var toast = Toast.Make(message, textSize: 18);

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await toast.Show(cts.Token);
        }

        private void SfSegmentedControl_SelectionChanged(object sender, Syncfusion.Maui.Toolkit.SegmentedControl.SelectionChangedEventArgs e)
        {
            Application.Current!.UserAppTheme = e.NewIndex == 0 ? AppTheme.Light : AppTheme.Dark;
        }

        private async void OnShellNavigating(object sender, ShellNavigatingEventArgs e)
        {
            if (e.Target.Location.OriginalString.Equals("//main/catalog"))
            {
                e.Cancel(); // stop normal navigation
                await Shell.Current.GoToAsync("//catalog?categoryId=0");
            }
        }

        private async void OnCartClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//mycart");
        }

        public async Task LoadCategories()
        {
            try
            {
                var categories = await _productRepository.GetCategoriesLocally();

                if (categories.Count > 0)
                {
                    foreach (var category in categories)
                    {
                        // Add the category ID as a query parameter in the route
                        // string routeWithQuery = $"{nameof(CatalogPage)}?categoryId={category.Id}";

                        var shellContent = new ShellContent
                        {
                            Title = category.CategoryName,
                            Icon = ImageSource.FromFile("catalog.png"),
                            ContentTemplate = new DataTemplate(() => new Catalog(_model)),
                            // Route = category.Route // unique Shell route
                            Route = $"catalog_{category.CategoryId}"
                        };

                        // Handle click/navigation using Shell
                        shellContent.Appearing += async (s, e) =>
                        {
                            // await Shell.Current.GoToAsync($"//catalog? {category.CategoryId}" );
                            if (category.CategoryId != null)
                            {
                                if (category.CategoryId >= 0)
                                {
                                    await Shell.Current.GoToAsync($"//catalog?categoryId={category.CategoryId}");
                                }

                            }
                        };

                        this.Items.Add(shellContent);
                    }
                }
            }
            catch (Exception ex) 
            { 
                Console.WriteLine(ex.Message.ToString());   
            }
          
        }


    }
}
