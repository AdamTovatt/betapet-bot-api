using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betapet.Models.Communication.Responses
{
    public class CreateGameResponse : BetapetResponse
    {
        [JsonProperty("result")]
        public string Result { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        public static CreateGameResponse FromJson(string json)
        {
            return JsonConvert.DeserializeObject<CreateGameResponse>(json);
        }
    }
}
