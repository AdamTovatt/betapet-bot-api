using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betapet.Models.InGame
{
    public class InGameUser
    {
        [JsonProperty("userid")]
        public int Id { get; set; }

        [JsonProperty("hand")]
        public string Hand { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }

        [JsonProperty("chat")]
        public int Chat { get; set; }

        [JsonProperty("bingos")]
        public int Bingos { get; set; }

        [JsonProperty("hand_cnt")]
        public int HandCount { get; set; }
    }
}
