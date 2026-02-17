using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using RestSharp;
using WebshopMobileApp.Models;

namespace WebshopMobileApp.Data
{
    public class LoginRepository
    {
        private readonly ILogger _logger;
        public string API_URL = Constants.API_URL;
        public LoginRepository(ILogger<UserResponse> logger)
        {
            _logger = logger;
        }
        public async Task<UserResponse> LoginAPICall(string username, string password)
        {
            var options = new RestClientOptions(API_URL)
            {
                // MaxTimeout = -1,
            };
            var client = new RestClient(options);
            var request = new RestRequest("/api/Authentication/login", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            var userrequest = new LoginModel() 
            { 
                Username = username,
                Password = password
            };
            var body = Newtonsoft.Json.JsonConvert.SerializeObject(userrequest);
            request.AddStringBody(body, DataFormat.Json);
            RestResponse response = await client.ExecuteAsync(request);
            Console.WriteLine(response.Content);
             if(response.Content != null)
            {
                var userResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<UserResponse>(response.Content);
                if (userResponse != null)
                {
                    return userResponse;
                }
            }
             return new UserResponse();
        }
    }
}
