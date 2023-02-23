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
        public Dictionary<string, ConversationalState> States { get; private set; }

        public Bot(IPredictionServiceRepository predictionServiceRepository)
        {
            PredictionServiceRepository = predictionServiceRepository;
            States = new Dictionary<string, ConversationalState>();
        }

        public Conversation CreateConversation(string startState = "default")
        {
            if (!States.ContainsKey(startState))
                throw new Exception("The specified start state does not exist: " + startState);

            return new Conversation(States.Keys.ToList(), startState);
        }

        public ConversationalState GetCurrentState(Conversation conversation)
        {
            return States[conversation.CurrentStateName];
        }

        /// <summary>
        /// Starts a new conversation. Use the Bot.GetConversation() method to get a conversation instance to use if you do not have one already.
        /// </summary>
        /// <param name="conversation">The conversation instance</param>
        /// <returns></returns>
        public string? Start(Conversation conversation)
        {
            return GetCurrentState(conversation).EnterState(conversation);
        }

        public List<string> GetResponse(Conversation conversation, string input)
        {
            ConversationalState currentState = GetCurrentState(conversation);
            ConversationalState previousState = currentState;
            List<string> result = new List<string>();

            result!.AddIfNotNull(currentState.ExitState(conversation));

            string newStateName = currentState.GetNextState(input);

            if (States.TryGetValue(newStateName, out ConversationalState? newState))
            {
                result!.AddIfNotNull(newState.EnterState(conversation));
                currentState = newState;
                conversation.CurrentStateName = newStateName;

                while (currentState.ForwardState != null)
                {
                    if (currentState.ForwardState == "[previous]")
                    {
                        currentState = previousState;
                        conversation.CurrentStateName = previousState.Name;
                        break;
                    }

                    if (States.TryGetValue(currentState.ForwardState, out ConversationalState? newForwardedState))
                    {
                        result!.AddIfNotNull(currentState.ExitState(conversation));
                        result!.AddIfNotNull(newForwardedState.EnterState(conversation));
                        currentState = newForwardedState;
                        conversation.CurrentStateName = newForwardedState.Name;
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
            BotTrainingProgress botTrainingProgress = new BotTrainingProgress(trainingData.States.Where(x => x.ForwardState == null).Count());
            PredictionTrainingService trainingService = PredictionServiceRepository.GetPredictionTrainingServiceInstance();

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

        public async Task TrainAsync(TrainingData trainingData, IProgress<BotTrainingProgress>? progress = null)
        {
            Task trainingTask = Task.Run(() => { Train(trainingData, progress); });
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

                    foreach (ConversationalState state in States.Values)
                        state.SetOwner(this);

                    foreach (BotBlobFile.ModelByteArrayPosition position in blobFile.ModelPositions)
                    {
                        if (position != null && position.Name != null)
                        {
                            byte[] modelBytes = new byte[position.Length];
                            memoryStream.Seek(initialOffset + position.Start, SeekOrigin.Begin);
                            memoryStream.Read(modelBytes, 0, position.Length);

                            ConversationalState conversationalState = States[position.Name];
                            conversationalState.EnsureConversationServiceIsNotNull();

                            if (conversationalState != null && conversationalState.ConversationService != null)
                                conversationalState.ConversationService.LoadModel(modelBytes);
                        }
                    }
                }
            }
        }
    }
}
