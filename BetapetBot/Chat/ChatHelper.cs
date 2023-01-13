using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetapetBot.Chat
{
    public class ChatHelper
    {
        ConversationService conversationService;

        public ChatHelper(byte[] neuralNetworkmodel)
        {
            conversationService = new ConversationService();
            conversationService.LoadModel(neuralNetworkmodel);
        }

        public string GetChatResponse(string message)
        {
            Conversation conversation = new Conversation() { Promt = message };

            List<ConversationResponse> result = conversationService.PredictResponse(conversation);

            StringBuilder stringBuilder = new StringBuilder(result[0].Text);
            stringBuilder[0] = char.ToUpper(result[0].Text[0]);

            return stringBuilder.ToString();
        }
    }
}
