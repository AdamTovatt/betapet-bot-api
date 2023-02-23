using ChatBot.Helpers;
using ChatBot.Models.Data;
using ChatBot.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Models.Prediction
{
    public class ConversationalState
    {
        public string Name { get; private set; }
        public string? ForwardState { get; set; }

        [JsonIgnore]
        public int RecentVisits { get; private set; }

        public Dictionary<int, List<string>> EnterResponses { get; set; }
        public Dictionary<int, List<string>> ExitResponses { get; set; }

        [JsonIgnore]
        public PredictionService? ConversationService { get; set; }
        private Bot owner;

        public ConversationalState(Bot owner, string name, Dictionary<int, List<string>> enterResponses, Dictionary<int, List<string>> exitResponses)
        {
            this.owner = owner;
            Name = name;
            EnterResponses = enterResponses;
            ExitResponses = exitResponses;
        }

        public string GetNextState(string input)
        {
            if (ForwardState != null)
                return ForwardState;

            if (ConversationService == null)
                throw new Exception("Conversation service is null for a state that needs predictions, this should not happen! The state is: " + Name);

            List<ConversationResponse> responses = ConversationService!.PredictResponse(new PromptResponsePair(input));
            return responses.First().Text;
        }

        public string? EnterState(Conversation conversation)
        {
            string? response = GetRightResponse(EnterResponses, conversation);
            conversation.AddToConversationalState(Name, 1);
            return response;
        }

        public string? ExitState(Conversation conversation)
        {
            return GetRightResponse(ExitResponses, conversation);
        }

        public void SetOwner(Bot owner)
        {
            this.owner = owner;
        }

        public void EnsureConversationServiceIsNotNull()
        {
            if (ConversationService != null)
                return;

            ConversationService = owner.PredictionServiceRepository.GetPredictionServiceInstance();
        }

        private string? GetRightResponse(Dictionary<int, List<string>> responseLists, Conversation conversation)
        {
            if (responseLists.Count == 0) return null;

            if(responseLists.TryGetValue(conversation.GetRecentVisits(Name), out List<string>? rightResponseList))
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
