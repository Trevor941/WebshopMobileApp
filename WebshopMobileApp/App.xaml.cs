using WebshopMobileApp.Pages.TabbedPages;

namespace WebshopMobileApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Application.Current.UserAppTheme = AppTheme.Light;
            //MainPage = new NavigationPage(new TabbedParentPage())
            //{
            //    // BarBackgroundColor = trevorGray
            //};

        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}