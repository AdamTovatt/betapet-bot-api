using Betapet.Models;
using Betapet.Models.Communication.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetapetBot.Chat
{
    public class ChatScenario
    {
        public string TheirText { get; set; }
        public string OurText { get; set; }
        public bool HasResponded { get; set; }
        public Game Game { get; set; }

        public List<ChatMessage> Messages { get; set; }
    }
}
