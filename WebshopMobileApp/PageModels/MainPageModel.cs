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
        private bool _iSVisibleSpinner = true;
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
            await GetProducts();
            await GetSlots();
            ISVisibleSpinner = false;
            DeliveryDate = "Delivery Date: " + DateTime.Now.AddDays(1).ToString("dd MMM yyyy"); 
        }

        private async Task GetProducts()
        {

            try
            {
            //   await _productRepository.GetProductsFromAPICall();
                await GetCategories();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
              var getit =  await _productRepository.GetProductsFromAPICall();
                if(getit == true)
                {
                    await GetCategories();
                }
            }
           
        }

        private async Task GetSlots()
        {
            Slots = await _productRepository.GetSlotsFromAPICall();
        }

        [RelayCommand]
        private async Task GoToCatalog(int CategoryId)
        {
            if(CategoryId > 0)
            {
                await Shell.Current.GoToAsync($"//catalog?categoryId={CategoryId}");
            }
        }


        private async Task GetCategories()
        {
            Categories = await _productRepository.GetCategoriesLocally();
        }
    }

}