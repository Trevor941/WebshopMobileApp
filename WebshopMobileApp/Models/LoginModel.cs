using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System;
using System.Text.Json.Serialization;

namespace WebshopMobileApp.Models
{
    public class LoginModel
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }


  public class UserResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("customerId")]
        public int CustomerId { get; set; }
    }

    

}
