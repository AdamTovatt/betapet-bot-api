using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betapet.Models.Communication.Responses
{
    public class PassTurnResponse : BetapetResponse
    {
        [JsonProperty("result")]
        public string Result { get; set; }

        public static PassTurnResponse FromJson(string json)
        {
            return JsonConvert.DeserializeObject<PassTurnResponse>(json);
        }
    }
}
