using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using WebshopMobileApp.Pages.TabbedPages;
using Microsoft.Maui.Controls;
namespace WebshopMobileApp.PageModels
{
    public partial class LoginPageModel : ObservableObject
    {
        private readonly LoginRepository _loginRepository;
        private readonly IAppNavigationService _navigation;
        [ObservableProperty]
        private string _username = "";//string.Empty;
        [ObservableProperty]
        private string _password = ""; //string.Empty;
        [ObservableProperty]
        private bool _termsandconditions = false;
        
        public LoginPageModel(LoginRepository loginRepository, IAppNavigationService navigation)
        {
            _loginRepository = loginRepository;
            _navigation = navigation;
        }

        [RelayCommand]
        private async Task LoginMethod()
        {
          
            if (_username == "" || _password == "")
            {
                await AppShell.DisplayToastAsync("Fill in username and password fields.");
                return;
            }
            if (_termsandconditions != true)
            {
                await AppShell.DisplayToastAsync("Agree with our terms and conditions.");
                return;
            }

            NetworkAccess accessType = Connectivity.Current.NetworkAccess;

            if (accessType == NetworkAccess.Internet)
            {
                var response = await _loginRepository.LoginAPICall(_username, _password);
                if (response != null)
                {
                    if (response.Token != null)
                    {
                        // var mainPage = new MainPageModel();
                        try
                        {
                            Preferences.Default.Set("token", response.Token);
                            Preferences.Default.Set("userEmail", Username);
                            Preferences.Default.Set("name", response.Name);
                            Preferences.Default.Set("customerId", response.CustomerId);
                            //   _navigation.GoToMainApp();
                            await Shell.Current.GoToAsync($"//home");

                            // Application.Current.MainPage = new NavigationPage(new WebshopMobileApp.Pages.MainPage());
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }

                    }
                    else
                    {
                        if(response.Name != null)
                        {
                            await AppShell.DisplayToastAsync(response.Name);
                        }
                        else
                        {
                            await AppShell.DisplayToastAsync("Internal server error. Contact admin!");
                        }
                    }
                }
           
            }
            else
            {
                await AppShell.DisplayToastAsync("You need an internet connection to use this app!");
                return;
            }
        }

    }
}
