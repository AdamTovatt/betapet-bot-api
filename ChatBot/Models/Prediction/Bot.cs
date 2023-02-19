using ChatBot.Helpers;
using ChatBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Models.Prediction
{
    public class Bot
    {
        public IPredictionServiceRepository PredictionServiceRepository { get; private set; }
        public State CurrentState { get; set; }
        public Dictionary<string, State> States { get; private set; }

        public Bot(IPredictionServiceRepository predictionServiceRepository, State startState)
        {
            PredictionServiceRepository = predictionServiceRepository;
            CurrentState = startState;
            States = new Dictionary<string, State>();
        }

        public async Task<List<string>> GetResponsesAsync(string input)
        {
            List<string> result = new List<string>();

            result!.AddIfNotNull(CurrentState.LeaveState());

            string newStateName = await CurrentState.GetNextStateAsync(input);

            if(States.TryGetValue(newStateName, out State? newState))
            {
                result!.AddIfNotNull(newState.EnterState());
                CurrentState = newState;
            }
            else
            {
                throw new Exception("Error when switching to new state: " + newStateName + " (state not found!)");
            }

            return result;
        }
    }
}
