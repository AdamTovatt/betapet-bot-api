using Betapet.Models.InGame;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Betapet.Models
{
    public class Game
    {
        [JsonProperty("gameid")]
        public int Id { get; set; }

        [JsonProperty("start_time")]
        public DateTime StartTime { get; set; }

        [JsonProperty("board_type")]
        public int BoardType { get; set; }

        [JsonProperty("player_cnt")]
        public int PlayerCount { get; set; }

        [JsonProperty("wordlist")]
        public int WordList { get; set; }

        [JsonProperty("activity")]
        public int Activity { get; set; }

        [JsonProperty("activity_time")]
        public DateTime ActivityTime { get; set; }

        [JsonProperty("active")]
        public int Active { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("board_data")]
        public string BoardData { get; set; }

        [JsonProperty("board_data_org")]
        public string OriginalBoardData { get; set; }

        [JsonProperty("turn")]
        public int Turn { get; set; }

        [JsonProperty("last_chat_time")]
        public DateTime LastChatTime { get; set; }

        [JsonProperty("user_list")]
        public List<InGameUser> UserList { get; set; }

        [JsonProperty("tiles_left")]
        public int TilesLeft { get; set; }

        [JsonProperty("tiles_percent")]
        public int TilesPercent { get; set; }

        [JsonProperty("fails")]
        public int Fails { get; set; }

        [JsonProperty("swap_count")]
        public int SwapCount { get; set; }

        [JsonProperty("words_first")]
        public string FirstWord { get; set; }

        [JsonProperty("words")]
        public string Words { get; set; }

        [JsonProperty("bingo")]
        public bool Bingo { get; set; }

        [JsonProperty("mark")]
        public List<string> Mark { get; set; }

        public static Game FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Game>(json);
        }
    }
}
