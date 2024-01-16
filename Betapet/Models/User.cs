using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betapet.Models
{
    public class User
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
        public int? Age { get; set; }

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

        public override string ToString()
        {
            return Handle;
        }
    }
}
