using Newtonsoft.Json;

namespace BetapetBotApi.FrontendModels
{
    public class Letter
    {
        [JsonProperty("stringValue")]
        public string StringValue { get; set; }

        [JsonProperty("scoreValue")]
        public int ScoreValue { get; set; }
    }
}
