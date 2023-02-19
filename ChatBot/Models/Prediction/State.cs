using ChatBot.Helpers;
using ChatBot.Models.Data;
using ChatBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Models.Prediction
{
    public class State
    {
        public string Name { get; private set; }
        public int RecentVisits { get; private set; }

        private Dictionary<int, List<string>> enterResponses;
        private Dictionary<int, List<string>> leaveResponses;

        private PredictionService? conversationService;
        private Bot owner;

        public State(Bot owner, string name, Dictionary<int, List<string>> enterResponses, Dictionary<int, List<string>> leaveResponses)
        {
            this.owner = owner;
            Name = name;
            this.enterResponses = enterResponses;
            this.leaveResponses = leaveResponses;
        }

        public async Task<string> GetNextStateAsync(string input)
        {
            if (conversationService == null)
                await LoadConversationServiceAsync();

            List<ConversationResponse> responses = conversationService!.PredictResponse(new Conversation(input));
            return responses.First().Text;
        }

        public async Task LoadConversationServiceAsync()
        {
            conversationService = await owner.PredictionServiceRepository.GetPredictionServiceAsync(Name);
        }

        public string? EnterState()
        {
            string? response = GetRightResponse(enterResponses);
            RecentVisits++;
            return response;
        }

        public string? LeaveState()
        {
            return GetRightResponse(leaveResponses);
        }

        private string? GetRightResponse(Dictionary<int, List<string>> responseLists)
        {
            if (responseLists.Count == 0) return null;

            if(responseLists.TryGetValue(RecentVisits, out List<string>? rightResponseList))
            {
                return rightResponseList.TakeRandomElement();
            }
            else
            {
                return responseLists[responseLists.Keys.Max()].TakeRandomElement();
            }
        }
    }
}
