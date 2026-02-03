using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WebshopMobileApp.Models;

namespace WebshopMobileApp.PageModels
{
    public partial class MainPageModel : ObservableObject
    {
        private readonly ProductRepository _productRepository;
        [ObservableProperty]
        private List<ProductsWithQuantity> _products = [];
        [ObservableProperty]
        private List<TblPromoPicturesSet> _slots = [];
        [ObservableProperty]
        private List<Category> _categories = [];
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
        }

        private async Task GetProducts()
        {
            Products = await _productRepository.GetProductsFromAPICall();
            if(Products.Count > 0)
            {
                await GetCategories();
            }
        }

        private async Task GetSlots()
        {
            Slots = await _productRepository.GetSlotsFromAPICall();
        }


        private async Task GetCategories()
        {
            var cati = Products.Where(p => !string.IsNullOrEmpty(p.Category))
              .GroupBy(p => new { p.CategoryId, p.Category })
              .Select(g => (g.Key.CategoryId, g.Key.Category)).OrderBy(x => x.Category).ToList();
            foreach (var cat in cati)
            {
                var realcat = new Category();
                realcat.CategoryId = cat.CategoryId;
                realcat.CategoryName = cat.Category;
                realcat.FileUrl = "https://orders.lumarfoods.co.za:20603/categories/" + cat.CategoryId + ".png";
                Categories.Add(realcat);    
            }
        }
    }

}