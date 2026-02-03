using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RestSharp;
using WebshopMobileApp.Models;

namespace WebshopMobileApp.Data
{
    public class ProductRepository
    {
        private readonly ILogger _logger;
        public ProductRepository(ILogger<ProductsWithQuantity> logger)
        {
            _logger = logger;
        }
        public async Task<List<ProductsWithQuantity>> GetProductsFromAPICall()
        {
            string token = Preferences.Default.Get("token", "null");
            int customerId = Preferences.Default.Get("customerId", 0);

            var options = new RestClientOptions("https://orders.lumarfoods.co.za:20603")
            {
                // MaxTimeout = -1,
            };
            var client = new RestClient(options);
            if(customerId == 0)
            {
                return new List<ProductsWithQuantity>();
            }
            var deliveryDate = DateTime.Now.ToString("yyyy-MM-dd");
            var request = new RestRequest($"/api/Products/GetAllProducts?CustomerID={customerId}&Deliverydate={deliveryDate}", Method.Get);
           // var request = new RestRequest($"'https://orders.lumarfoods.co.za:20603/api/Products/GetAllProducts?CustomerId=2118&DeliveryDate=2026-", Method.Get);
            //request.AddHeader("Content-Type", "application/json");
            RestResponse response = await client.ExecuteAsync(request);
            Console.WriteLine(response.Content);
            if (response.Content != null)
            {
                var userResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ProductsWithQuantity>>(response.Content);
                if (userResponse != null)
                {
                    return userResponse; //.Take(5).ToList();
                }
            }
            return new List<ProductsWithQuantity>();
        }

        public async Task<List<TblPromoPicturesSet>> GetSlotsFromAPICall()
        {
            string token = Preferences.Default.Get("token", "null");
            int customerId = Preferences.Default.Get("customerId", 0);

            var options = new RestClientOptions("https://orders.lumarfoods.co.za:20603")
            {
                // MaxTimeout = -1,
            };
            var client = new RestClient(options);
            if (customerId == 0)
            {
                return new List<TblPromoPicturesSet>();
            }
            var deliveryDate = DateTime.Now.ToString("yyyy-MM-dd");
            var request = new RestRequest($"/api/Products/GetPromos", Method.Get);
            RestResponse response = await client.ExecuteAsync(request);
            Console.WriteLine(response.Content);
            if (response.Content != null)
            {
                var Response = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TblPromoPicturesSet>>(response.Content);
                if (Response != null)
                {
                    return Response;
                }
            }
            return new List<TblPromoPicturesSet>();
        }
    }
}
