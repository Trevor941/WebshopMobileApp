using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WebshopMobileApp.Models;

namespace WebshopMobileApp.PageModels
{
    public partial class ProductsListPageModel : ObservableObject
    {
        private readonly ProductRepository _productRepository;
        [ObservableProperty]
        private List<ProductsWithQuantity> _products = [];
        [ObservableProperty]
        private List<TblPromoPicturesSet> _slots = [];
        [ObservableProperty]
        private List<Category> _categories = [];
        [ObservableProperty]
        private bool _iSVisibleSpinner = true;
        [ObservableProperty]
        private string _deliveryDate = "";
        //public List<(int CategoryId, string Category)> _categories { get; set; } = new();
        public ProductsListPageModel(ProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        [RelayCommand]
        private async Task LoadItems()
        {
            await GetProducts();
            //await GetSlots();
            ISVisibleSpinner = false;
            DeliveryDate = "Delivery Date: " + DateTime.Now.AddDays(1).ToString("dd MMM yyyy");
        }

        private async Task GetProducts()
        {
            try
            {
                await _productRepository.CreateTableProductsLocally();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            var localproducts = await _productRepository.GetProductsLocally();
            if (localproducts.Count > 0)
            {
                Products = localproducts.Take(10).ToList();
                 await ProductFileUrls();
            }
            else
            {
                Products = await _productRepository.GetProductsFromAPICall();
                if (Products.Count > 0)
                {
                    foreach (var product in Products)
                    {
                        await _productRepository.InsertProduct(product);
                    }
                    var localproducts2 = await _productRepository.GetProductsLocally();
                    if (localproducts2.Count > 0)
                    {
                        Products = localproducts2.Take(10).ToList();
                        await ProductFileUrls();
                    }
                }
            }

        }

        private async Task GetSlots()
        {
            Slots = await _productRepository.GetSlotsFromAPICall();
        }

        private async Task ProductFileUrls()
        {
            foreach (var x in Products)
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
