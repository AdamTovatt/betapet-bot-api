using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betapet.Models.Communication.Responses
{
    public class ChallengePlayerResponse : BetapetResponse
    {
        [JsonProperty("result")]
        public bool Result { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        public static ChallengePlayerResponse FromJson(string json)
        {
            return JsonConvert.DeserializeObject<ChallengePlayerResponse>(json);
        }
    }
}
