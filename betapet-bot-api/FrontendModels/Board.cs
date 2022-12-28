using Newtonsoft.Json;

namespace BetapetBotApi.FrontendModels
{
    public class Board
    {
        [JsonProperty("squares")]
        public Square[][] Squares { get; set; }

        [JsonProperty("playerState")]
        public PlayerState PlayerState { get; set; }

        [JsonProperty("opponentState")]
        public PlayerState OpponentState { get; set; }
    }
}
