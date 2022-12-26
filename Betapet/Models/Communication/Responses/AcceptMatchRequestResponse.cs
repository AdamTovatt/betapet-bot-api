using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betapet.Models.Communication.Responses
{
    public class AcceptMatchRequestResponse : BetapetResponse
    {
        [JsonProperty("result")]
        public bool Result { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        public static AcceptMatchRequestResponse FromJson(string json)
        {
            return JsonConvert.DeserializeObject<AcceptMatchRequestResponse>(json);
        }
    }
}
