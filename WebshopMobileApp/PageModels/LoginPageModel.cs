using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
namespace WebshopMobileApp.PageModels
{
    public partial class LoginPageModel : ObservableObject
    {
        private readonly LoginRepository _loginRepository;

        [ObservableProperty]
        private string _username = string.Empty;
        [ObservableProperty]
        private string _password = string.Empty;
        [ObservableProperty]
        private bool _termsandconditions = false;
        
        public LoginPageModel(LoginRepository loginRepository)
        {
           _loginRepository = loginRepository;
        }

        [RelayCommand]
        private async Task LoginMethod()
        {
            if(_termsandconditions != true)
            {
                await AppShell.DisplayToastAsync("Agree with our terms and conditions.");
                return;
            }
           var response = await _loginRepository.LoginAPICall(_username, _password);
            if (response != null)
            {
                if(response.Token != null)
                {
                    // var mainPage = new MainPageModel();
                    try
                    {
                        Preferences.Default.Set("token", response.Token);
                        Preferences.Default.Set("name", response.Name);
                        Preferences.Default.Set("customerId", response.CustomerId);
                        await Shell.Current.GoToAsync($"//main");
                       
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());       
                    }
                    
                }
                else
                {
                    await AppShell.DisplayToastAsync("Username and password did not match!");
                }
                //_username = "";
                //_password = "";
            }
        }

    }
}
