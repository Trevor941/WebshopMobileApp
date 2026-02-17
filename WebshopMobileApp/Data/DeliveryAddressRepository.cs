using Microsoft.Extensions.Logging;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebshopMobileApp.Models;

namespace WebshopMobileApp.Data
{
    public class DeliveryAddressRepository
    {
        private readonly ILogger _logger;
        public string API_URL = Constants.API_URL;
        public DeliveryAddressRepository(ILogger<DeliveryAddress> logger)
        {
            _logger = logger;
        }
        public async Task<List<DeliveryAddress>> GetDeliveryAddressFromAPICall(string CustomerID)
        {
            int customerId = Preferences.Default.Get("customerId", 0);
            var options = new RestClientOptions(API_URL)
            {
                // MaxTimeout = -1,
            };
            var client = new RestClient(options);
            if (customerId == 0)
            {
                return new List<DeliveryAddress>();
            }
            var request = new RestRequest($"{API_URL}/api/DeliveryDates/GetDeliveryAddresses/{CustomerID}", Method.Get);
            RestResponse response = await client.ExecuteAsync(request);
            Console.WriteLine(response.Content);
            if (response.Content != null)
            {
                var deliveryAddress = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DeliveryAddress>>(response.Content);
                if (deliveryAddress != null)
                {
                    return deliveryAddress;
                }
            }
            return new List<DeliveryAddress>();
        }
    }
}
