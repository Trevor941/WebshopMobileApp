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

    public class CustomerDeliveryDatesRepository
    {
        private readonly ILogger _logger;
        public string API_URL = Constants.API_URL;
        public CustomerDeliveryDatesRepository(ILogger<ProductsWithQuantity> logger)

        {
            _logger = logger;
        }
        public async Task<List<CustomerDeliveryDatesModel>> GetCustomerDeliveryDatesFromAPICall(int CustomerID)
        {
            int customerId = Preferences.Default.Get("customerId", 0);
            var options = new RestClientOptions(API_URL)
            {
                // MaxTimeout = -1,
                RemoteCertificateValidationCallback = (sender, cert, chain, errors) => true
            };
            var client = new RestClient(options);
            if (customerId == 0)
            {
                return new List<CustomerDeliveryDatesModel>();
            }
            var request = new RestRequest($"{API_URL}/api/DeliveryDates/GetCustomerDeliveryDates/{CustomerID}", Method.Get);
            RestResponse response = await client.ExecuteAsync(request);
            Console.WriteLine(response.Content);
            if (response.Content != null)
            {
                var dates = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CustomerDeliveryDatesModel>>(response.Content);
                if (dates != null)
                {
                    return dates; 
                }
            }
            return new List<CustomerDeliveryDatesModel>();
        }
    }
   
}
    
