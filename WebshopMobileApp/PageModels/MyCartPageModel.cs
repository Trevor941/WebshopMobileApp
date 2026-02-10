using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebshopMobileApp.Models;

namespace WebshopMobileApp.PageModels
{
    public partial class MyCartPageModel : ObservableObject
    {
        private readonly CartRepository _cartRepository;
        [ObservableProperty]
        private List<CartModel> _cart = [];
        public MyCartPageModel(CartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }


        [RelayCommand]
        private async Task LoadCartItems()
        {
            await GetCartItems();
        }

        [RelayCommand]
        private async Task DeleteCartItem(CartModel model)
        {
             await _cartRepository.DeleteCartItem(model.ProductServerId);
            GetCartItems();
            ((AppShell)Shell.Current).UpdateCartCount(Cart.Count);
        }


        private async Task GetCartItems()
        {
            try
            {
              Cart = await _cartRepository.GetCartData();
                if(Cart.Count > 0)
                {
                    await ProductFileUrls();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task ProductFileUrls()
        {
            foreach (var x in Cart)
            {
                if (x.HasImage)
                {
                    x.FileUrl = "https://orders.lumarfoods.co.za:20603/images/" + x.ProductServerId + ".png";
                }
                else
                {
                    x.FileUrl = "https://orders.lumarfoods.co.za:20603/images/300px-no_image_available.svg.png";
                }
            }
        }

    }
}
