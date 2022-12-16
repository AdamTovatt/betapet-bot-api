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
        public List<GameUser> Friends { get; set; }

        public static FriendsResponse FromJson(string json)
        {
            return new FriendsResponse() { Friends = JsonConvert.DeserializeObject<List<GameUser>>(json) };
        }
    }
}
