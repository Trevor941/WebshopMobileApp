using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using WebshopMobileApp.Pages.TabbedPages;

namespace WebshopMobileApp.Data
{
    public class AppNavigationService : IAppNavigationService
    {
        private readonly IServiceProvider _serviceProvider;

        public AppNavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public void GoToMainApp()
        {
            var mainPage = _serviceProvider.GetRequiredService<TabbedParentPage>();
            Application.Current.MainPage = mainPage;
        }

        public void GoToLogin()
        {
            var shell = _serviceProvider.GetRequiredService<AppShell>();
            Application.Current.MainPage = shell;
        }
    }
}
