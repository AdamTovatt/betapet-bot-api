using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betapet.Models.Communication.Responses
{
    public class MatchRequestResponse : BetapetResponse
    {
        public List<MatchRequestResponseItem> MatchRequests { get; set; }

        public static MatchRequestResponse FromJson(string json)
        {
            if (string.IsNullOrEmpty(json) || json == "[]")
                return new MatchRequestResponse() { MatchRequests = new List<MatchRequestResponseItem>() };

            return JsonConvert.DeserializeObject<MatchRequestResponse>(json);
        }
    }

    public class MatchRequestResponseItem
    {
        [JsonProperty("gameid")]
        public int GameId { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("host_id")]
        public int HostId { get; set; }

        [JsonProperty("opponent_id")]
        public int OpponentId { get; set; }

        [JsonProperty("board_type")]
        public int BoardType { get; set; }

        [JsonProperty("wordlist")]
        public int WordList { get; set; }

        [JsonProperty("rating")]
        public int Rating { get; set; }

        [JsonProperty("gender")]
        public int Gender { get; set; }
    }
}
