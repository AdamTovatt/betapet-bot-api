using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betapet.Models.Communication.Responses
{
    public class SendChatResponse : BetapetResponse
    {
        [JsonProperty("result")]
        public bool Result { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("msg")]
        public string Message { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("userid")]
        public int UserId { get; set; }

        [JsonProperty("ignored")]
        public int Ignored { get; set; }

        public static SendChatResponse FromJson(string json)
        {
            return JsonConvert.DeserializeObject<SendChatResponse>(json);
        }
    }
}
