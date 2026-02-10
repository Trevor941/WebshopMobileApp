using WebshopMobileApp.Pages.TabbedPages;

namespace WebshopMobileApp
{
    public partial class App : Application
    {
        private readonly ProductRepository _productRepository;
        private readonly CartRepository _cartRepository;
        private readonly ProductsListPageModel _model;
        public App(ProductRepository productRepository, ProductsListPageModel model, CartRepository cartRepository)
        {
            InitializeComponent();
            Application.Current.UserAppTheme = AppTheme.Light;
            _productRepository = productRepository;
            _model = model;
            _cartRepository = cartRepository;
            //MainPage = new NavigationPage(new TabbedParentPage())
            //{
            //    // BarBackgroundColor = trevorGray
            //};

        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell(_productRepository, _model, _cartRepository));
        }
    }
}