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
    public class DeliverToRepository
    {
        private readonly ILogger _logger;
        public string API_URL = Constants.API_URL;
        public DeliverToRepository(ILogger<CustomerModel> logger)
        {
            _logger = logger;
        }
        public async Task<DeliverTo> GetDeliverToFromAPICall(string CustomerCode, string UserEmail)
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
                return new DeliverTo();
            }
            var request = new RestRequest($"{API_URL}/api/DeliveryDates/GetDeliveryTos?customerCode='{CustomerCode}'&onlineEmailAddress={UserEmail}", Method.Get);
            RestResponse response = await client.ExecuteAsync(request);
            Console.WriteLine(response.Content);
            if (response.Content != null)
            {
                var deliverTo = Newtonsoft.Json.JsonConvert.DeserializeObject<DeliverTo>(response.Content);
                if (deliverTo != null)
                {
                    return deliverTo;
                }
            }
            return new DeliverTo();
        }
    }
}
