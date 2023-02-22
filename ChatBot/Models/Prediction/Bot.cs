using ChatBot.Helpers;
using ChatBot.Models.Data;
using ChatBot.Services;
using Newtonsoft.Json;
using System.Text;

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
                    if (CurrentState.ForwardState == "[previous]")
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

        public void Train(TrainingData trainingData, PredictionTrainingService trainingService, IProgress<BotTrainingProgress>? progress = null)
        {
            BotTrainingProgress botTrainingProgress = new BotTrainingProgress(trainingData.States.Where(x => x.ForwardState == null).Count());

            foreach (State state in trainingData.States)
            {
                botTrainingProgress.CurrentTask = state.Name;

                ConversationalState conversationalState = new ConversationalState(this, state.Name, state.EnterResponses, state.ExitResponses);
                conversationalState.ForwardState = state.ForwardState;

                if (state.ForwardState == null)
                {
                    if (progress != null) progress.Report(botTrainingProgress);

                    conversationalState.ConversationService = trainingService.CreateNewPredictionService();

                    conversationalState.ConversationService.LoadModel(trainingService.Train(state.Routes));

                    botTrainingProgress.CompletedTasks++;
                    if (progress != null) progress.Report(botTrainingProgress);
                }

                States.Add(state.Name, conversationalState);
            }
        }

        public async Task TrainAsync(TrainingData trainingData, PredictionTrainingService trainingService, IProgress<BotTrainingProgress>? progress = null)
        {
            Task trainingTask = Task.Run(() => { Train(trainingData, trainingService, progress); });
            await trainingTask;
        }

        public byte[] GetAsBytes()
        {
            BotBlobFile botBlobFile = new BotBlobFile(States);
            string fileJson = JsonConvert.SerializeObject(botBlobFile);
            byte[] fileJsonBytes = Encoding.UTF8.GetBytes(fileJson);

            int initialOffset = 4 + fileJsonBytes.Length;
            byte[] bytes = new byte[initialOffset + botBlobFile.LengthSum];

            BitConverter.GetBytes(fileJsonBytes.Length).CopyTo(bytes, 0);
            fileJsonBytes.CopyTo(bytes, 4);

            foreach (BotBlobFile.ModelByteArrayPosition position in botBlobFile.ModelPositions)
            {
                if (position != null && position.Name != null && States != null)
                {
                    ConversationalState state = States[position.Name];
                    if (state != null && state.ConversationService != null)
                    {
                        byte[] conversationBytes = state.ConversationService.GetBytes();
                        conversationBytes.CopyTo(bytes, position.Start + initialOffset);
                    }
                }
            }

            return bytes;
        }

        public void Load(byte[] bytes)
        {
            int jsonLength = BitConverter.ToInt32(bytes);

            using (MemoryStream memoryStream = new MemoryStream(bytes))
            {
                byte[] jsonBytes = new byte[jsonLength];
                memoryStream.Seek(4, SeekOrigin.Begin);
                memoryStream.Read(jsonBytes, 0, jsonLength);

                string json = Encoding.UTF8.GetString(jsonBytes);
                BotBlobFile? blobFile = JsonConvert.DeserializeObject<BotBlobFile>(json);

                int initialOffset = jsonBytes.Length + 4;

                if (blobFile != null)
                {
                    States = blobFile.States;

                    foreach (BotBlobFile.ModelByteArrayPosition position in blobFile.ModelPositions)
                    {
                        byte[] modelBytes = new byte[position.Length];
                        memoryStream.Seek(initialOffset + position.Start, SeekOrigin.Begin);
                        memoryStream.Read(modelBytes, 0, position.Length);

                        ConversationalState conversationalState = States[position.Name];
                        if (conversationalState != null && conversationalState.ConversationService != null)
                            conversationalState.ConversationService.LoadModel(modelBytes);
                    }
                }
            }
        }
    }
}
