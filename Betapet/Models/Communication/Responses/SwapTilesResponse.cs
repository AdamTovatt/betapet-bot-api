using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betapet.Models.Communication.Responses
{
    public class SwapTilesResponse : BetapetResponse
    {
        [JsonProperty("result")]
        public string Result { get; set; }

        [JsonProperty("end")]
        public bool End { get; set; }

        [JsonProperty("swap_count")]
        public int SwapCount { get; set; }

        [JsonProperty("tiles")]
        public string Tiles { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        public static SwapTilesResponse FromJson(string json)
        {
            return JsonConvert.DeserializeObject<SwapTilesResponse>(json);
        }
    }
}
