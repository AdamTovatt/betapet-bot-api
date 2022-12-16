using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betapet.Models.Communication.Responses
{
    public class LoginResponse : BetapetResponse
    {
        [JsonProperty("userid")]
        public string? UserId { get; set; }

        [JsonProperty("authkey")]
        public string? AuthKey { get; set; }

        public static LoginResponse FromJson(string json)
        {
            LoginResponse response = JsonConvert.DeserializeObject<LoginResponse>(json);

            if (response == null)
                throw new Exception("Result became null when deserializing login response from string: " + json);

            return response;
        }
    }
}
