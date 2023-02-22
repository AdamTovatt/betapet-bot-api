using ChatBot.Helpers;
using ChatBot.Models.Data;
using ChatBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
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

        public async Task<string> GetNextStateAsync(string input)
        {
            if (ForwardState != null)
                return ForwardState;

            if (ConversationService == null)
                await LoadConversationServiceAsync();

            List<ConversationResponse> responses = ConversationService!.PredictResponse(new PromptResponsePair(input));
            return responses.First().Text;
        }

        public async Task LoadConversationServiceAsync()
        {
            ConversationService = await owner.PredictionServiceRepository.GetPredictionServiceAsync(Name);
        }

        public string? EnterState()
        {
            string? response = GetRightResponse(EnterResponses);
            RecentVisits++;
            return response;
        }

        public string? ExitState()
        {
            return GetRightResponse(ExitResponses);
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
