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
        private readonly CartRepository _cartRepository;
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
        private CancellationTokenSource _cts;

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
        private int _quantity;

        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged();
            }
        }
        public ProductsListPageModel(ProductRepository productRepository, CartRepository cartRepository)
        {
            _productRepository = productRepository;
            _cartRepository = cartRepository;
        }
        
        [RelayCommand]
        public async void AddToCart(ProductsWithQuantity product)
        {
            try
            {
                await _cartRepository.CreateTableCart();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            if (product != null)
            {
                var cart = new CartModel();
                
                    cart.ProductServerId = product.ProductServerId;
                    cart.ProductCode = product.Code.Trim();
                    cart.Quantity = product.Quantity;
                    cart.Price = product.Price;
                    cart.PriceIncl = product.PriceIncl;
                    cart.HasImage = product.HasImage;
                    cart.Description = product.Description.Trim();
                    cart.UnitOfSale = product.UOM.Trim();
                    cart.TaxPercentage = product.TaxPercentage;
                    cart.TotalInc = 1;
                    cart.lineTotal = 1;
                    cart.NettPrice = 1;
                    cart.VatTotal = 1;

                    await _cartRepository.InsertCart(cart);
                    product = new();
                   var xyz = await _cartRepository.GetCartData();
                 ((AppShell)Shell.Current).UpdateCartCount(xyz.Count);
            }
        }

        [RelayCommand]
        private void IncrementQuantity(ProductsWithQuantity product)
        {
            product.Quantity++;
            Console.WriteLine($"clicked increment button, {product.Description}, quntity is {product.Quantity}");
        }

        [RelayCommand]
        private void DecrementQuantity(ProductsWithQuantity product)
        {
            Console.WriteLine($"clicked decrement button, , {product.Description}, quntity is {product.Quantity}");
            if (product.Quantity > 1)
                product.Quantity--;
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
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            var productsvar = UnFilteredProducts;
            if (string.IsNullOrWhiteSpace(searchText))
            {
                Products = UnFilteredProducts;
                return;
            }

            try
            {
                if (searchText.Length >= 4)
                {
                    Task.Delay(300, token);
                    var results = Products.Where(p =>
                    p.Description.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                    ||
                   p.Code.Contains(searchText, StringComparison.OrdinalIgnoreCase));
                    Products = results.ToList();
                }
            }
            catch (TaskCanceledException)
            {
                // Ignore cancellation
            }
           
        
        }
    }
}
