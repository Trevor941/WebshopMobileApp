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
    public partial class FavouriteListPageModel : ObservableObject
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
        [ObservableProperty]
        private List<CartModel> _cart = [];
        private string _searchText;
        private List<ProductsWithQuantity> FilteredProducts = new List<ProductsWithQuantity>();
        private List<ProductsWithQuantity> UnFilteredProducts = new List<ProductsWithQuantity>();
        private CancellationTokenSource _searchCts;
        public string CategoryId { get; set; }
        [ObservableProperty]
        private string _totalInc = string.Empty;
        //public async void ApplyQueryAttributes(IDictionary<string, object> query)
        //{
        //    if (query.TryGetValue("categoryId", out var value))
        //    {
        //        CategoryId = value?.ToString();
        //        // do your loading logic here
        //       // await LoadItems();
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
                DebounceSearch(_searchText); // call your filter logic here
            }
        }

        public FavouriteListPageModel(ProductRepository productRepository, CartRepository cartRepository)
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

                cart.NettPrice = Math.Round(cart.Price * cart.Quantity, 2);
                cart.VatTotal = Math.Round(cart.Price * (cart.TaxPercentage / 100), 2, MidpointRounding.AwayFromZero);
                var Vat = Math.Round(cart.NettPrice * (cart.TaxPercentage / 100), 2);
                cart.lineTotal = Math.Round(cart.NettPrice + Vat, 2);
                cart.TotalInc = 0;
                cart.FileUrl = product.FileUrl;
                await _cartRepository.InsertUpdateCart(cart);
                product = new();
                var Cart = await _cartRepository.GetCartData();
                ((AppShell)Shell.Current).UpdateCartCount(Cart.Sum(p => p.Quantity));
            }
        }

        [RelayCommand]
        private void IncrementQuantity(ProductsWithQuantity product)
        {
            product.Quantity++;
            Console.WriteLine($"clicked increment button, {product.Description}, quntity is {product.Quantity}");
        }

        [RelayCommand]
        private async Task AppearingLoad()
        {
            await LoadItems();
        }

        [RelayCommand]
        private async Task DecrementQuantity(ProductsWithQuantity product)
        {
            Console.WriteLine($"clicked decrement button , {product.Description}, quntity is {product.Quantity}");
            if (product.Quantity > 1)
            {
                product.Quantity--;

            }
        }


        [RelayCommand]
        private async Task LoadItems()
        {
            await GetCartItems();
            await GetProducts();
        }

        private async Task GetProducts()
        {
            try
            {
                UnFilteredProducts = await _productRepository.GetProductsByFavorites();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                UnFilteredProducts = new List<ProductsWithQuantity>();
            }
            Products = UnFilteredProducts;

        }

        private async void FilterProducts(string searchText)
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
                Products = await _productRepository.GetProductsBySearchWord(searchText);
            }
            catch (TaskCanceledException)
            {
                // Ignore cancellation
            }
            ISVisibleSpinner = false;

        }

        private async Task GetCartItems()
        {
            try
            {
                Cart = await _cartRepository.GetCartData();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private async void DebounceSearch(string text)
        {
            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();
            var token = _searchCts.Token;

            try
            {
                await Task.Delay(500, token); // wait until user stops typing

                if (token.IsCancellationRequested)
                    return;

                FilterProducts(text);
            }
            catch (TaskCanceledException)
            {
                // expected when user keeps typing
            }
        }

    }
}
