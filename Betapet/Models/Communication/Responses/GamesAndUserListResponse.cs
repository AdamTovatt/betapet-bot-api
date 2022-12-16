using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betapet.Models.Communication.Responses
{
    public class GamesAndUserListResponse : BetapetResponse
    {
        [JsonProperty("time")]
        public int Time { get; set; }

        [JsonProperty("users")]
        public List<User> Users { get; set; }

        [JsonProperty("games")]
        public List<Game> Games { get; set; }

        public static GamesAndUserListResponse FromJson(string json)
        {
            return JsonConvert.DeserializeObject<GamesAndUserListResponse>(json);
        }
    }
}
