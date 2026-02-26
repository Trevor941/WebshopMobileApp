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
                RemoteCertificateValidationCallback = (sender, cert, chain, errors) => true
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
            RestResponse response = new();
            
            
            response = await client.ExecuteAsync(request);
            var userResponse=new UserResponse();
            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    Console.WriteLine(response.Content);
                    if (response.Content != null)
                    {
                        userResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<UserResponse>(response.Content);
                        if (userResponse != null)
                        {
                            return userResponse;
                        }
                        else
                        {
                            userResponse = new UserResponse();
                            userResponse.Name = response.StatusCode.ToString();
                            return userResponse;
                        }
                    }
                    else
                    {
                        userResponse = new UserResponse();
                        userResponse.Name = "1" +  response.ToString();
                        return userResponse;
                    }
                    break;
                case System.Net.HttpStatusCode.Unauthorized:
                    userResponse = new UserResponse();
                    userResponse.Name = "Username and password did not match";
                    return userResponse;
                default:
                    userResponse = new UserResponse();
                    if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                    {
                        userResponse.Name = "2" + response.Content;
                    }
                    else
                    {
                        // log this
                        var error = response.ErrorException;
                        userResponse.Name = "2" + error!.ToString(); //response.StatusCode.ToString();
                    }
                    return userResponse;
            }


            //if (response.StatusCode == System.Net.HttpStatusCode.OK)
            //{
            //    Console.WriteLine(response.Content);
            //    if (response.Content != null)
            //    {
            //        var userResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<UserResponse>(response.Content);
            //        if (userResponse != null)
            //        {
            //            return userResponse;
            //        }
            //    }
            //}
            //if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            //{
            //        var userResponse = new UserResponse();
            //        userResponse.Name = "Username and password did not match";
            //        return userResponse;
            //}
            //if (response.StatusCode == System.Net.HttpStatusCode.BadRequest || response.StatusCode == System.Net.HttpStatusCode.MethodNotAllowed)
            //{
            //    var userResponse = new UserResponse();
            //    userResponse.Name = /*"Internal server error. Contact admin! Bad request and method not allowed" +*/ response.StatusCode.ToString();
            //    return userResponse;
            //}

           // UserResponse? userResponse1 = new UserResponse();
           //// userResponse1 = null;
           // userResponse1.Name = response!.Content!.ToString() + "TREV " + response.StatusCode.ToString();
           // return  userResponse1;
            // return new UserResponse();
        }
    }
}
