using Newtonsoft.Json;

namespace BetapetBotApi.FrontendModels
{
    public class Player
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("handle")]
        public string Handle { get; set; }

        [JsonProperty("name_first")]
        public string NameFirst { get; set; }

        [JsonProperty("name_last")]
        public string NameLast { get; set; }

        [JsonProperty("age")]
        public int Age { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("gender")]
        public string Gender { get; set; }

        [JsonProperty("location_text")]
        public string LocationText { get; set; }

        [JsonProperty("rating")]
        public int Rating { get; set; }

        [JsonProperty("won")]
        public int Won { get; set; }

        [JsonProperty("droped")]
        public int Dropped { get; set; }

        [JsonProperty("drawn")]
        public int Drawn { get; set; }

        [JsonProperty("games")]
        public int Games { get; set; }

        [JsonProperty("bingo")]
        public int Bingo { get; set; }
    }
}
