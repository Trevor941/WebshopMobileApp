using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebshopMobileApp.Models;
using WebshopMobileApp.Pages.Controls;

namespace WebshopMobileApp.PageModels
{
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
        [ObservableProperty]
        private List<CartModel> _cart = [];
        private string _searchText;
        private List<ProductsWithQuantity> FilteredProducts = new List<ProductsWithQuantity>();
        private List<ProductsWithQuantity> UnFilteredProducts = new List<ProductsWithQuantity>();
        private CancellationTokenSource _searchCts;
        public string? CategoryId { get; set; }
        [ObservableProperty]
        private string _totalInc = string.Empty;
        public int OldQty = 1;
        [ObservableProperty]
        private bool _paginationVisible = false;
        private PopupRecommendedProduct _currentPopup;
        private int _pageNumber = 1;
        private  int PageSize = 30;
        //private int _currentPage = 1;
        public int CurrentPage { get; set; } = 1;
        public int ProductCount { get; set; } = 0;
        public int ProductSearchCount { get; set; } = 0;
        public int ProductCatCount { get; set; } = 0;
        //public int CurrentPage
        //{
        //    get => _currentPage;
        //    set
        //    {
        //        if (_currentPage != value)
        //        {
        //            _currentPage = value;
        //            OnPropertyChanged(nameof(CurrentPage));
        //        }
        //    }
        //}
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            CategoryId = null;
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
                DebounceSearch(_searchText); // call your filter logic here
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
                    
                    cart.NettPrice = Math.Round((cart.Price * cart.Quantity) ?? 0, 2);
                    cart.VatTotal = Math.Round((cart.Price * (cart.TaxPercentage / 100)) ?? 0m, 2, MidpointRounding.AwayFromZero);
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
            ISVisibleSpinner = true;
            var products = new List<ProductsWithQuantity>();
            while (products.Count == 0)
            {
                try
                {
                    products = await _productRepository.GetProductsLocally(PageSize, _pageNumber);

                    if (products != null && products.Count > 0)
                    {
                        Console.WriteLine($"Found {products.Count} products at {DateTime.Now}");
                        // Do something with the products
                    }
                    else
                    {
                        Console.WriteLine($"No products found at {DateTime.Now}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching products: {ex.Message}");
                }

                // Wait for 1 minute
                await Task.Delay(TimeSpan.FromMinutes(1));
            }
            ISVisibleSpinner = true;
            //await LoadItems();
            //try
            //{
            //    var products = await _productRepository.GetProductsLocally();
            //    if (products.Count < 30)
            //    {
            //        var productsAPI = await _productRepository.GetProductsFromAPICall();
            //        if (productsAPI.Count > 0)
            //        {
            //            await _productRepository.InsertProducts(productsAPI);
            //        }
            //    }
            //}
            //catch(Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}

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
            try
            {
                ProductCount = await _productRepository.GetTotalProductCount();
                await FetchNewlyAddedProducts();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            CurrentPage = 1;
            ISVisibleSpinner = true;
            //await Task.Yield();
            SearchText = "";
          //  ISVisibleSpinner = false;
           // DeliveryDate = "Delivery Date: " + DateTime.Now.AddDays(1).ToString("dd MMM yyyy");
            if(CategoryId != null && CategoryId != "")
            {
                if(CategoryId == "0")
                {
                    //await Task.Yield();
                    await GetProducts();
                    ISVisibleSpinner = false;
                    return;
                }
                //await Task.Yield();
                await LoadProductsByCategory(CategoryId);
                ISVisibleSpinner = false;
                return;
            }
            else
            {
                //await Task.Yield();
                await GetProducts();
                ISVisibleSpinner = false;
                return;
            }

        }

        public async Task FetchNewlyAddedProducts()
        {
            if (ProductCount > 0)
            {
                var xyz = await _productRepository.FetchNewlyAddedProductsAndPriceUpdates();
            }
        }

        public async Task LoadProductsByCategory(string categoryId)
        {
            ProductCatCount = await _productRepository.GetTotalProductByCategoryCount(Int32.Parse(categoryId));
            try
            {
                UnFilteredProducts = await _productRepository.GetProductsByCategory(PageSize, CurrentPage, Int32.Parse(categoryId));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                var products = await _productRepository.GetProductsFromAPICall();
                if (products.Count > 0)
                {
                    UnFilteredProducts = await _productRepository.GetProductsByCategory(PageSize, CurrentPage, Int32.Parse(categoryId));
                }
            }
            Products = UnFilteredProducts;
            if (ProductCatCount > PageSize)
            {
                PaginationVisible = true;
            }
            ISVisibleSpinner = false;
           // CategoryId = "0";
            categoryId = "0";

        }

        [RelayCommand]
        private async Task NextPage()
        {
            var Products1 = Products;
            if (ProductCount <= 30)
            {
                return;
            }
            if (SearchText == "" && CategoryId == "0")
            {
                if (ProductCount <= PageSize)
                {
                    return;
                }
                CurrentPage++;
                await GetProducts();
            }
            if (CategoryId != "0" && CategoryId != null)
            {
                if(ProductCatCount <= PageSize)
                {
                    return;
                }
                CurrentPage++;
                Products = await _productRepository.GetProductsByCategory(PageSize, CurrentPage, Int32.Parse(CategoryId));
            }
            if (SearchText.Length > 0)
            {
                if (ProductSearchCount <= PageSize)
                {
                    return;
                }
                CurrentPage++;
                Products = await _productRepository.GetProductsBySearchWord(PageSize, CurrentPage, SearchText);
            }
            if (Products.Count == 0)
            {
                Products = Products1;
                CurrentPage--;
            }
        }
        [RelayCommand]
        private async Task PreviousPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                if (SearchText == "" && CategoryId == "0")
                {
                    await GetProducts();
                }
                if (CategoryId != "0" && CategoryId != null)
                {
                    Products = await _productRepository.GetProductsByCategory(PageSize, CurrentPage, Int32.Parse(CategoryId));
                }
                if (SearchText.Length > 0)
                {
                    Products = await _productRepository.GetProductsBySearchWord(PageSize, CurrentPage, SearchText);
                }
            }
        }
        private async Task GetProducts()
        {
            try
            {
                UnFilteredProducts = await _productRepository.GetProductsLocally(PageSize, CurrentPage);
                if(UnFilteredProducts.Count == 0)
                {
                    var products = await _productRepository.GetProductsFromAPICall();
                    if(products.Count > 0)
                    {
                        UnFilteredProducts = await _productRepository.GetProductsLocally(PageSize, CurrentPage);
                    }
                }
                ISVisibleSpinner = false;
                Products = UnFilteredProducts;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                var products = await _productRepository.GetProductsFromAPICall();
                if(products.Count > 0)
                {
                    UnFilteredProducts = await _productRepository.GetProductsLocally(PageSize, CurrentPage);
                }
                ISVisibleSpinner = false;
            }
            Products = UnFilteredProducts;
            if (ProductCount >= PageSize)
            {
                PaginationVisible = true;
            }
            CategoryId = "0";
        }

        private async Task GetSlots()
        {
            Slots = await _productRepository.GetSlotsFromAPICall();
        }

        [RelayCommand]
        private async Task ProductPopup(ProductsWithQuantity model)
        {
            OldQty = model.Quantity;
            _currentPopup = new PopupRecommendedProduct(model);
            await Application.Current.MainPage.ShowPopupAsync(_currentPopup);
        }

        private async void FilterProducts(string searchText)
        {
            var totalpro = 0;
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
              ////  if (searchText.Length > 0)
              ////  {
              //      var results = Products.Where(p =>
              //      p.Description.Contains(searchText, StringComparison.OrdinalIgnoreCase)
              //      ||
              //     p.Code.Contains(searchText, StringComparison.OrdinalIgnoreCase));
              //      Products = results.ToList();
              ////  }
              ///
              Products = await _productRepository.GetProductsBySearchWord(PageSize, CurrentPage, searchText);
                ProductSearchCount = await _productRepository.GetTotalProductBySearchCount(searchText);
            }
            catch (TaskCanceledException)
            {
                // Ignore cancellation
            }
            if (ProductSearchCount >= PageSize)
            {
                PaginationVisible = true;
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
