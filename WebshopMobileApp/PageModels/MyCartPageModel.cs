
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using WebshopMobileApp.Models;
using WebshopMobileApp.Pages.Controls;
using WebshopMobileApp.Pages.TabbedPages;

namespace WebshopMobileApp.PageModels
{
    public partial class MyCartPageModel : ObservableObject, INotifyPropertyChanged
    {
        private readonly CartRepository _cartRepository;
        private readonly CustomerRepository _customerRepository;
        private readonly ProductRepository _productRepository;
        private readonly CustomerDeliveryDatesRepository _customerDeliveryDatesRepository;
        private readonly DeliverToRepository _deliverToRepository;
        private readonly DeliveryAddressRepository _deliveryAddressRepository;
        [ObservableProperty]
        private List<CartModel> _cart = [];
        [ObservableProperty]
        private List<TblPromoPicturesSet> _slots = [];
        [ObservableProperty]
        private List<ProductsWithQuantity> _products = [];
       // [ObservableProperty]
        private ProductsWithQuantity _product = new();
        [ObservableProperty]
        private string _description = string.Empty;
        [ObservableProperty]
        private string _fileUrl = string.Empty;
        [ObservableProperty]
        private string _totalIncStr = string.Empty;
        [ObservableProperty]
        private decimal _quantity = 1;
        public decimal? TotalInc { get; set; }
        public OrderHeader orderHeader = new OrderHeader();
        public int DeliveryAddressiD = 0;
        public int RouteId = 0;
        public int CustomerID = 0;
        public int NewOID = 0;
        public DeliverTo deliverto = new DeliverTo();
        public CustomerModel customer = new CustomerModel();
        public int OldQty = 1;
        private PopupSingleProduct _currentPopup;
        private PopupRecommendedProduct _currentRecommendPopup;
        ////[ObservableProperty]
        //List <CustomerDeliveryDatesModel> deliveryDatesModel = new List<CustomerDeliveryDatesModel>();
        //OrderHeader orderHeader = new();
        //public int RouteId = 0;
        //public int DeliveryAddressiD = 0;
        //public int CustomerID;
        //public DateTime DeliveryDate = DateTime.Now.AddDays(1);
        //public CustomerModel customer = new CustomerModel();
        //DeliverTo deliverto = new DeliverTo();
        //public string UserEmail = "";
        //List<DeliveryAddress>? DeliveryAddresses = new List<DeliveryAddress>();
        //public int? NewOID = 0;
        public MyCartPageModel(
            CartRepository cartRepository, 
            ProductRepository productRepository, 
            CustomerDeliveryDatesRepository customerDeliveryDatesRepository,
            CustomerRepository customerRepository,
            DeliverToRepository deliverToRepository,
            DeliveryAddressRepository deliveryAddressRepository
            )
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _customerDeliveryDatesRepository = customerDeliveryDatesRepository;
            _customerRepository = customerRepository;
            _deliverToRepository = deliverToRepository;
            _deliveryAddressRepository = deliveryAddressRepository;
        }


        [RelayCommand]
        private async Task LoadCartItems()
        {
            await GetRecommendedProducts();
            await GetCartItems();
            GetTotal();
        }

        [RelayCommand]
        private async Task ProductPopup(CartModel model)
        {
            OldQty = model.Quantity;
            _currentPopup = new PopupSingleProduct(model);
            await Application.Current.MainPage.ShowPopupAsync(_currentPopup);
        }

        [RelayCommand]
        private async Task RecommendedProductPopup(ProductsWithQuantity model)
        {
           // OldQty = model.Quantity;
            _currentRecommendPopup = new PopupRecommendedProduct(model);
            await Application.Current.MainPage.ShowPopupAsync(_currentRecommendPopup);
        }

        [RelayCommand]
        private async Task AddNewCartItem(ProductsWithQuantity product)
        {
            if (product != null)
            {
                var cart = new CartModel();
                cart.Quantity = product.Quantity;
                var existitem = await _cartRepository.CheckProductExist(product.Code.Trim());
                if(existitem.Id > 0)
                {
                    cart = existitem;
                    cart.Quantity = cart.Quantity + product.Quantity;
                }
                cart.ProductServerId = product.ProductServerId;
                cart.ProductCode = product.Code.Trim();
                cart.Price = product.Price;
                cart.PriceIncl = product.PriceIncl;
                cart.HasImage = product.HasImage;
                cart.Description = product.Description.Trim();
                cart.UnitOfSale = product.UOM.Trim();
                cart.TaxPercentage = product.TaxPercentage;

                cart.NettPrice = Math.Round((cart.Price * cart.Quantity) ?? 0m, 2);
                cart.VatTotal = Math.Round((cart.Price * (cart.TaxPercentage / 100)) ?? 0m, 2, MidpointRounding.AwayFromZero);
                var Vat = Math.Round(cart.NettPrice * (cart.TaxPercentage / 100), 2);
                cart.lineTotal = Math.Round(cart.NettPrice + Vat, 2);
                cart.TotalInc = 0;
                cart.FileUrl = product.FileUrl;
                await _cartRepository.InsertUpdateCart(cart);
                product = new();
                Cart = await _cartRepository.GetCartData();
                ((AppShell)Shell.Current).UpdateCartCount(Cart.Sum(p => p.Quantity));
                GetTotal();
                _currentRecommendPopup?.Close();
                product.Quantity = 1;
            }
        }

        [RelayCommand]
        private async Task DeleteCartItem(CartModel model)
        {
             await _cartRepository.DeleteCartItem(model.ProductServerId);
            await GetCartItems();
            GetTotal();
            ((AppShell)Shell.Current).UpdateCartCount(Cart.Sum(p => p.Quantity));
        }

        private async Task GetRecommendedProducts()
        {
            Products = await _productRepository.GetRecommendedProducts();
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


        string GetTotal()
        {
            TotalInc = 0;
            foreach (CartModel cartModel in Cart)
            {
                TotalInc += cartModel.Quantity * cartModel.PriceIncl;
            }
            TotalIncStr = $"Total: R {TotalInc.Value.ToString("N2")} Inc";
            return $"TOTAL :  R {TotalInc.Value.ToString("N2")}";
        }



        async void GetDeliveryDate()
        {
            //show check out dialog
            var DeliveryDate = DateTime.Now;
            DeliveryDate = DeliveryDate.AddDays(1);
            var deliveryDatesModel = new List<CustomerDeliveryDatesModel>();
            var CustomerID = Preferences.Default.Get("customerId", 0);
            DayOfWeek today = DateTime.Now.DayOfWeek;
            deliveryDatesModel = await _customerDeliveryDatesRepository.GetCustomerDeliveryDatesFromAPICall(CustomerID);
            if (deliveryDatesModel.Count > 0)
            {
                var deliverydatetoday = deliveryDatesModel.Find(x => x.Deliverydates == today.ToString());

                var nextdeliverydate = deliveryDatesModel.Find(x => x.Id > (int)today);
                if (nextdeliverydate == null)
                {
                    nextdeliverydate = deliveryDatesModel.FirstOrDefault();
                }
                DateTime currentDate = DateTime.Now;
                int daysUntilNextDD = (nextdeliverydate!.Id - (int)currentDate.DayOfWeek + 7) % 7;
                if (daysUntilNextDD == 0)
                {
                    daysUntilNextDD = 7;
                }
                DateTime nextDD = currentDate.AddDays(daysUntilNextDD);
                DeliveryDate = nextDD;
                orderHeader.DeliveryDate = DeliveryDate;
                Console.WriteLine("Next Delivery Date is on: " + nextDD);

            }
            else
            {
                orderHeader.DeliveryDate = DateTime.Now.AddDays(1);
                return;
            }
        }

        public async Task GetCustDetails()
        {
             CustomerID = Preferences.Default.Get("customerId", 0);
            var UserEmail = Preferences.Default.Get("userEmail", "");
            customer = await _customerRepository.GetCustomerModelFromAPICall(CustomerID);
               deliverto = await _deliverToRepository.GetDeliverToFromAPICall(customer.CustomerCode, UserEmail);  
            if (deliverto != null)
            {
                if (deliverto.DeliveryAddressesId > 1)
                {
                    DeliveryAddressiD = deliverto.DeliveryAddressesId;
                    RouteId = deliverto.tblCustomer_tblRoutesSetItem;
                }
                else
                {
                    if (customer != null)
                    {
                        RouteId = customer.CustomerRoute;
                    }
                }
            }
            var deliveryDatesModel = await _customerDeliveryDatesRepository.GetCustomerDeliveryDatesFromAPICall(CustomerID);
            // deliveryDatesModel = (await httpClient.GetFromJsonAsync<List<CustomerDeliveryDatesModel>>($"api/DeliveryDates/GetCustomerDeliveryDates/{CustomerID}"))!;
        }


        [RelayCommand]
        async Task CheckoutCart()
        {
            // SpinerVisibleProperty = true;
            // SuggestedDialog = false;
              GetDeliveryDate();
             await GetCustDetails();
            string OrderNo = "Order From MobileTest";
             OrderNo = await Application.Current.MainPage.DisplayPromptAsync("Dear User", "Please put in your Order No", "Save", "Cancel");

            var customer = new CustomerModel();
            var deliverto = new DeliverTo();
            string UserEmail = "";
            DateTime DeliveryDate = DateTime.Now;
            
            orderHeader.DeliveryDate = DeliveryDate;
            orderHeader.CustomerId = CustomerID;
            orderHeader.sCustCode = customer.CustomerCode! ?? "";
            orderHeader.UserCreated = UserEmail;
            orderHeader.OrderNo = OrderNo;
            if (deliverto.UniqueDeliveryAddresses == true)
            {
               var DeliveryAddresses = await _deliveryAddressRepository.GetDeliveryAddressFromAPICall(CustomerID.ToString());
                //await httpClient.GetFromJsonAsync<List<DeliveryAddress>>(
                //$"api/DeliveryDates/GetDeliveryAddresses/{CustomerID}")!;
            }
            else
            {
                orderHeader.tblOrders_tblDeliveryAddress = DeliveryAddressiD;
                orderHeader.RouteId = RouteId;
            }

            //var dh = CalculateTotalInc();
            Console.WriteLine($"Order Total is {TotalInc}");
            if (TotalInc < customer.WebShopMinValueThreshold)
            {
                //  OrderLessThanThousand = true;
            }
            //Building API model
            List<CartModel> Cart = new List<CartModel>();
            //using var db = await DbFactory.Create<IndexDb>();
            //{
            //    Cart = db.cart.ToList();
            //}
            Cart = await _cartRepository.GetCartData();
            var CartItems = new List<CartModelToPost>();
            CartPostRequest postRequest = new();
            postRequest.OrderHeader = orderHeader;
            Console.WriteLine(orderHeader.OrderId);
            foreach (var item in Cart)
            {
                var cartItem = new CartModelToPost();
                cartItem.Id = item.Id;
                cartItem.ProductID = item.ProductServerId;
                cartItem.ProductCode = item.ProductCode;
                cartItem.Quantity = item.Quantity;
                cartItem.Price = Math.Round(item.Price ?? 0m, 2);
                cartItem.PriceIncl = Math.Round(item.PriceIncl ?? 0m, 2);
                cartItem.HasImage = item.HasImage;
                cartItem.Description = item.Description;
                cartItem.UnitOfSale = item.UnitOfSale;
                cartItem.TaxPercentage = item.TaxPercentage;
                cartItem.TotalInc = item.TotalInc;
                cartItem.lineTotal = item.lineTotal;
                cartItem.NettPrice = item.NettPrice;
                cartItem.VatTotal = item.VatTotal;
                Console.WriteLine(item.Id);
                CartItems.Add(cartItem);
            }
            postRequest.CartItems = CartItems;


            NewOID = await _cartRepository.PostCart(postRequest);
            if(NewOID > 0)
            {
                
                await Application.Current.MainPage.DisplayAlert("THANK YOU!", "Your order was created successfully", "OK");
                await Shell.Current.GoToAsync($"//catalog?categoryId=0");
                await ClearCart();
                NewOID = 0;
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Internal Server Error", "Contact the tech support for assistance", "OK");
            }
           
        }

        public async Task ClearCart()
        {
            await _cartRepository.ClearCart();
            Cart =  await _cartRepository.GetCartData();
            ((AppShell)Shell.Current).UpdateCartCount(Cart.Sum(p => p.Quantity));
            TotalInc = 0;
            TotalIncStr = $"Total: R {TotalInc.Value.ToString("N2")} Inc";
        }

        [RelayCommand]
        private void IncrementQuantity(CartModel product)
        {
            product.Quantity++;
            Console.WriteLine($"clicked increment button, {product.Description}, quntity is {product.Quantity}");
        }


        [RelayCommand]
        private async Task DecrementQuantity(CartModel product)
        {
            Console.WriteLine($"clicked decrement button , {product.Description}, quntity is {product.Quantity}");
            if (product.Quantity > 1)
            {
                product.Quantity--;

            }
        }

        [RelayCommand]
        private async Task CancelCartQtyUpdate(CartModel model)
        {
            _currentPopup?.Close();
            model.Quantity = OldQty;
        }

        [RelayCommand]
        private void RecommendedProdIncrementQuantity(ProductsWithQuantity product)
        {
            product.Quantity++;
        }


        [RelayCommand]
        private async Task RecommendedProdDecrementQuantity(ProductsWithQuantity product)
        {
            if (product.Quantity > 1)
            {
                product.Quantity--;

            }
        }

        [RelayCommand]
        private async Task RecommendedProdCancelCartQtyUpdate(ProductsWithQuantity model)
        {
            _currentRecommendPopup?.Close();
            model.Quantity = OldQty;
        }


        [RelayCommand]
        public async Task UpdateCart(CartModel cart)
        {
           
            if (cart != null)
            {
              
                cart.Quantity = cart.Quantity;
                cart.Price = Math.Round(cart.Price ?? 0m, 2);
                cart.PriceIncl = Math.Round(cart.PriceIncl * cart.Quantity ?? 0m, 2);
                cart.NettPrice = Math.Round((cart.Price * cart.Quantity) ?? 0m, 2);
                cart.VatTotal = Math.Round((cart.Price * (cart.TaxPercentage / 100)) ?? 0m, 2, MidpointRounding.AwayFromZero);
                var Vat = Math.Round(cart.NettPrice * (cart.TaxPercentage / 100), 2);
                cart.lineTotal = Math.Round(cart.NettPrice + Vat, 2);
                cart.TotalInc = 0;

                await _cartRepository.InsertUpdateCart(cart);
                cart = new();
                _currentPopup?.Close();
                 Cart = await _cartRepository.GetCartData();
                ((AppShell)Shell.Current).UpdateCartCount(Cart.Sum(p => p.Quantity));
                GetTotal();
                OldQty = 1;
            }
        }

    }
}
