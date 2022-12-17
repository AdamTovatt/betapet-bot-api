using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betapet.Models.Communication.Responses
{
    public class GetChatResponse : BetapetResponse
    {
        public List<ChatMessage> Messages { get; set; }

        public static GetChatResponse FromJson(string json)
        {
            return new GetChatResponse() { Messages = JsonConvert.DeserializeObject<List<ChatMessage>>(json) };
        }
    }

    public class ChatMessage
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("userid")]
        public int UserId { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("msg")]
        public string Message { get; set; }

        public override string ToString()
        {
            return string.Format("{0}: {1}", UserId, Message);
        }
    }
}
