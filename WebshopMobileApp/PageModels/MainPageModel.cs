using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WebshopMobileApp.Models;

namespace WebshopMobileApp.PageModels
{
    public partial class MainPageModel : ObservableObject
    {
        private readonly ProductRepository _productRepository;
        [ObservableProperty]
        private List<TblPromoPicturesSet> _slots = [];
        [ObservableProperty]
        private List<Category> _categories = [];
        [ObservableProperty]
        private List<ProductsWithQuantity> _products = [];
        [ObservableProperty]
        private bool _iSVisibleSpinner = true;
        [ObservableProperty]
        private bool _pleaseWaitMessage = true;
        [ObservableProperty]
        private bool _pleaseWaitSpinner = true;
        [ObservableProperty]
        private string _deliveryDate = "";
        //public List<(int CategoryId, string Category)> _categories { get; set; } = new();
        public MainPageModel(ProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        [RelayCommand]
        private async Task LoadItems()
        {
            DeliveryDate = "Delivery Date: " + DateTime.Now.AddDays(1).ToString("dd MMM yyyy");
            await GetSlots();
            await GetProducts();
            await GetCategories();
            ISVisibleSpinner = false;
            PleaseWaitSpinner = false;
            PleaseWaitMessage = false;
        }


        private async Task GetSlots()
        {
            try
            {
                Slots = await _productRepository.GetPromosLocally();
            }
            catch (Exception ex)
            {
                PleaseWaitSpinner = true;
                PleaseWaitMessage = true;
                Slots = await _productRepository.GetSlotsFromAPICall();
                Slots = await _productRepository.GetPromosLocally();
            }
        }

        [RelayCommand]
        private async Task GoToCatalog(int CategoryId)
        {
            if(CategoryId > 0)
            {
                await Shell.Current.GoToAsync($"//catalog?categoryId={CategoryId}");
            }
        }

        private async Task GetProducts()
        {
            ISVisibleSpinner = true;
           // PleaseWaitSpinner = true;
           // PleaseWaitMessage = true;
            try
            {
                Products = await _productRepository.GetProductsLocally(1, 1);
                if (Products.Count == 0)
                {
                    var products = await _productRepository.GetProductsFromAPICall();
                    if (products.Count > 0)
                    {
                        Products = await _productRepository.GetProductsLocally(1, 1);
                    }
                }
                ISVisibleSpinner = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                var products = await _productRepository.GetProductsFromAPICall();
                if (products.Count > 0)
                {
                    Products = await _productRepository.GetProductsLocally(1, 1);
                }
                ISVisibleSpinner = false;
            }
        }


        private async Task GetCategories()
        {
            try
            {
                Categories = await _productRepository.GetCategoriesLocally();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                PleaseWaitSpinner = true;
                PleaseWaitMessage = true;
                var getcat = await _productRepository.GetCategoriesFromAPICall();
                if (getcat == true)
                {
                    Categories = await _productRepository.GetCategoriesLocally();
                }
            }
        }
    }

}