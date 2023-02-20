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
    public class Bot
    {
        public IPredictionServiceRepository PredictionServiceRepository { get; private set; }
        public ConversationalState? CurrentState { get; set; }
        public Dictionary<string, ConversationalState> States { get; private set; }

        public Bot(IPredictionServiceRepository predictionServiceRepository)
        {
            PredictionServiceRepository = predictionServiceRepository;
            States = new Dictionary<string, ConversationalState>();
        }

        public string? Start(string startState = "default")
        {
            if (CurrentState == null)
            {
                CurrentState = States[startState];
            }

            return CurrentState.EnterState();
        }

        public async Task<List<string>> GetResponsesAsync(string input)
        {
            if (CurrentState == null)
                throw new Exception("The bot needs to be started with the Start() method before it can be used");

            ConversationalState previousState = CurrentState;
            List<string> result = new List<string>();

            result!.AddIfNotNull(CurrentState.ExitState());

            string newStateName = await CurrentState.GetNextStateAsync(input);

            if (States.TryGetValue(newStateName, out ConversationalState? newState))
            {
                result!.AddIfNotNull(newState.EnterState());
                CurrentState = newState;

                while (CurrentState.ForwardState != null)
                {
                    if(CurrentState.ForwardState == "[previous]")
                    {
                        CurrentState = previousState;
                        break;
                    }

                    if (States.TryGetValue(CurrentState.ForwardState, out ConversationalState? newForwardedState))
                    {
                        result!.AddIfNotNull(CurrentState.ExitState());
                        result!.AddIfNotNull(newForwardedState.EnterState());
                        CurrentState = newForwardedState;
                    }
                    else
                    {
                        throw new Exception("Error when switching to new state: " + newStateName + " (state not found!)");
                    }
                }
            }
            else
            {
                throw new Exception("Error when switching to new state: " + newStateName + " (state not found!)");
            }

            return result;
        }

        public void Train(TrainingData trainingData, IProgress<BotTrainingProgress>? progress = null)
        {
            PredictionTrainingService trainingService = new PredictionTrainingService();
            BotTrainingProgress botTrainingProgress = new BotTrainingProgress(trainingData.States.Where(x => x.ForwardState == null).Count());

            foreach (State state in trainingData.States)
            {
                botTrainingProgress.CurrentTask = state.Name;

                ConversationalState conversationalState = new ConversationalState(this, state.Name, state.EnterResponses, state.ExitResponses);
                conversationalState.ForwardState = state.ForwardState;

                if (state.ForwardState == null)
                {
                    if (progress != null) progress.Report(botTrainingProgress);

                    conversationalState.ConversationService = new PredictionService();

                    conversationalState.ConversationService.LoadModel(trainingService.Train(state.Routes));

                    botTrainingProgress.CompletedTasks++;
                    if (progress != null) progress.Report(botTrainingProgress);
                }

                States.Add(state.Name, conversationalState);
            }
        }

        public async Task TrainAsync(TrainingData trainingData, IProgress<BotTrainingProgress>? progress = null)
        {
            Task trainingTask = Task.Run(() => { Train(trainingData, progress); });
            await trainingTask;
        }
    }
}
