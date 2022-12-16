using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Betapet.Models.Communication.Responses
{
    public class BetapetResponse
    {
        [JsonProperty("result")]
        public bool Result { get; set; }
    }
}
