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
    [QueryProperty(nameof(ParameterToPassBack), "categoryId")]
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
        private string _searchText;
        private List<ProductsWithQuantity> FilteredProducts = new List<ProductsWithQuantity>();
        private List<ProductsWithQuantity> UnFilteredProducts = new List<ProductsWithQuantity>();
        private string _parameterToPassBack;
        public string ParameterToPassBack
        {
            get => _parameterToPassBack;
            set
            {
                _parameterToPassBack = Uri.UnescapeDataString(value);
                // handle the value here
            }
        }
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText == value) return;
                _searchText = value;
                OnPropertyChanged();

                FilterProducts(_searchText); // call your filter logic here
            }
        }

        public ProductsListPageModel(ProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        [RelayCommand]
        private async Task LoadItems()
        {
            SearchText = "";
            await GetProducts();
            //await GetSlots();
            ISVisibleSpinner = false;
            DeliveryDate = "Delivery Date: " + DateTime.Now.AddDays(1).ToString("dd MMM yyyy");
            if(ParameterToPassBack != null && ParameterToPassBack != "")
            {
                if(ParameterToPassBack == "0")
                {
                    return;
                }
                LoadProductsData(ParameterToPassBack);
            }
        }

        public void LoadProductsData(string categoryId)
        {
            // Do whatever you need with the parameter
            var SearchedProducts = UnFilteredProducts.Where(x => x.CategoryId.ToString() == categoryId).ToList();
            if(SearchedProducts != null && SearchedProducts.Count > 0)
            {
                Products = SearchedProducts;
            }
            // Maybe load catalog items from API based on this ID
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
            UnFilteredProducts = await _productRepository.GetProductsLocally();
            if (UnFilteredProducts.Count > 0)
            {
                Products = UnFilteredProducts;//.Take(10).ToList();
                 await ProductFileUrls();
            }
            else
            {
                UnFilteredProducts = await _productRepository.GetProductsFromAPICall();
                if (UnFilteredProducts.Count > 0)
                {
                    foreach (var product in Products)
                    {
                        await _productRepository.InsertProduct(product);
                    }
                    UnFilteredProducts = await _productRepository.GetProductsLocally();
                    if (UnFilteredProducts.Count > 0)
                    {
                        Products = UnFilteredProducts;//.Take(10).ToList();
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

        private void FilterProducts(string searchText)
        {
            var productsvar = UnFilteredProducts;
            if (string.IsNullOrWhiteSpace(searchText))
            {
                //FilteredProducts.Clear();
                //foreach (var p in Products)
                //    FilteredProducts.Add(p);
                //return;
                Products = UnFilteredProducts;
                return;
            }

            if(searchText.Length >= 4)
            {
                var results = Products.Where(p =>
                p.Description.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                ||
               p.Code.Contains(searchText, StringComparison.OrdinalIgnoreCase));
                    Products = results.ToList();
            }
        
            //FilteredProducts.Clear();
            //foreach (var item in results)
            //    FilteredProducts.Add(item);
        }
    }
}
