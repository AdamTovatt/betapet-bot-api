using Newtonsoft.Json;

namespace BetapetBotApi.FrontendModels
{
    public class Square
    {
        [JsonProperty("letter")]
        public Letter Letter { get; set; }

        [JsonProperty("multiplier")]
        public int Multiplier { get; set; }

        [JsonProperty("multiplyWord")]
        public bool MultiplyWord { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("x")]
        public int X { get; set; }

        [JsonProperty("y")]
        public int Y { get; set; }
    }
}
