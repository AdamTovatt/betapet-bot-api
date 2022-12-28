using Newtonsoft.Json;

namespace BetapetBotApi.FrontendModels
{
    public class PlayerState
    {
        [JsonProperty("userId")]
        public int UserId { get; set; }

        [JsonProperty("hand")]
        public List<Letter> Hand { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }

        [JsonProperty("handCount")]
        public int HandCount { get; set; }
    }
}
