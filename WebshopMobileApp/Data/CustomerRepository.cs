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
    public class CustomerRepository
    {
        private readonly ILogger _logger;
        public string API_URL = Constants.API_URL;
        public CustomerRepository(ILogger<CustomerModel> logger)
        {
            _logger = logger;
        }
        public async Task<CustomerModel> GetCustomerModelFromAPICall(int CustomerID)
        {
            int customerId = Preferences.Default.Get("customerId", 0);
            var options = new RestClientOptions(API_URL)
            {
                // MaxTimeout = -1,
            };
            var client = new RestClient(options);
            if (customerId == 0)
            {
                return new CustomerModel();
            }
            var request = new RestRequest($"{API_URL}/api/DeliveryDates/GetCustomerThreshold/{CustomerID}", Method.Get);
            RestResponse response = await client.ExecuteAsync(request);
            Console.WriteLine(response.Content);
            if (response.Content != null)
            {
                var customer = Newtonsoft.Json.JsonConvert.DeserializeObject<CustomerModel>(response.Content);
                if (customer != null)
                {
                    return customer;
                }
            }
            return new CustomerModel();
        }
    }
}
