using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betapet.Models.Communication.Responses
{
    public class FriendsResponse : BetapetResponse
    {
        public List<Friend> Friends { get; set; }

        public static FriendsResponse FromJson(string json)
        {
            return new FriendsResponse() { Result = true, Friends = JsonConvert.DeserializeObject<List<Friend>>(json) };
        }
    }

    public class Friend
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("handle")]
        public string Handle { get; set; }

        [JsonProperty("access_lvl")]
        public int AccessLevel { get; set; }

        [JsonProperty("name_first")]
        public string FirstName { get; set; }

        [JsonProperty("name_last")]
        public string LastName { get; set; }

        [JsonProperty("age")]
        public int Age { get; set; }

        [JsonProperty("rating")]
        public int Rating { get; set; }

        [JsonProperty("won")]
        public int Won { get; set; }

        [JsonProperty("lost")]
        public int Lost { get; set; }

        [JsonProperty("droped")]
        public int Dropped { get; set; }

        [JsonProperty("games")]
        public int Games { get; set; }

        [JsonProperty("bingos")]
        public int Bingos { get; set; }
    }
}
