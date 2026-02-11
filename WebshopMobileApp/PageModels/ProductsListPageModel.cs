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
    //[QueryProperty(nameof(ParameterToPassBack), "categoryId")]
    public partial class ProductsListPageModel : ObservableObject, IQueryAttributable
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
        public string CategoryId { get; set; }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("categoryId", out var value))
            {
                CategoryId = value?.ToString();
                // do your loading logic here
                await LoadItems();
            }
        }
        //public string ParameterToPassBack
        //{
        //    get => _parameterToPassBack;
        //    set
        //    {
        //        _parameterToPassBack = Uri.UnescapeDataString(value);
        //        OnPropertyChanged();
        //        // handle the value here
        //    }
        //}
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText == value) return;
                _searchText = value;
                OnPropertyChanged();
                Task.Delay(2000);
                FilterProducts(_searchText); // call your filter logic here
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


       // [RelayCommand]
        private async Task LoadItems()
        {
            SearchText = "";
            ISVisibleSpinner = false;
           // DeliveryDate = "Delivery Date: " + DateTime.Now.AddDays(1).ToString("dd MMM yyyy");
            if(CategoryId != null && CategoryId != "")
            {
                if(CategoryId == "0")
                {
                    return;
                }
                await LoadProductsByCategory(CategoryId);
                return;
            }
            await GetProducts();
        }

        public async Task LoadProductsByCategory(string categoryId)
        {
            // Do whatever you need with the parameter
            //var SearchedProducts = UnFilteredProducts.Where(x => x.CategoryId.ToString() == categoryId).ToList();
            //if(SearchedProducts != null && SearchedProducts.Count > 0)
            //{
                UnFilteredProducts = await _productRepository.GetProductsLocallyByCategory(Int32.Parse(categoryId));
                Products = UnFilteredProducts;
                await ProductFileUrls();
            //  }
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
            ISVisibleSpinner = true;
            var productsvar = UnFilteredProducts;
            Products = UnFilteredProducts;
            if (string.IsNullOrWhiteSpace(searchText))
            {
                Products = UnFilteredProducts;
                ISVisibleSpinner = false;
                return;
            }

            try
            {
                if (searchText.Length >= 3)
                {
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
           ISVisibleSpinner = false;

        }
    }
}
