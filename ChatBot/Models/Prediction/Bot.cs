using ChatBot.Helpers;
using ChatBot.Models.Data;
using ChatBot.Services;
using Newtonsoft.Json;
using System.Text;

namespace ChatBot.Models.Prediction
{
    public class Bot
    {
        /// <summary>
        /// The prediction service repository of this bot. Used to creat correct trainers and predictors
        /// </summary>
        public IPredictionServiceRepository PredictionServiceRepository { get; private set; }

        /// <summary>
        /// Dictionary containing all states for this bot
        /// </summary>
        public Dictionary<string, ConversationalState> States { get; private set; }

        /// <summary>
        /// Constructor without parameters. Will use new SdcaPredictionServiceRepository() for the prediction service repository
        /// </summary>
        public Bot()
        {
            PredictionServiceRepository = new SdcaPredictionServiceRepository();
            States = new Dictionary<string, ConversationalState>();
        }

        /// <summary>
        /// Constructor taking a prediction service repository as a paramter. The prediction service repository will later be used to create the correct trainers and predictors
        /// </summary>
        /// <param name="predictionServiceRepository">The prediction service repository to use. Using new SdcaPredictionServiceRepository() is recommended</param>
        public Bot(IPredictionServiceRepository predictionServiceRepository)
        {
            PredictionServiceRepository = predictionServiceRepository;
            States = new Dictionary<string, ConversationalState>();
        }

        /// <summary>
        /// Will create a new conversation to use with this bot. Will thrown an exception if the provided start state does not exist
        /// </summary>
        /// <param name="startState">The state to start the conversation in. If none is provided it will look for a state called "default"</param>
        /// <returns>An instance of a Conversation object</returns>
        /// <exception cref="Exception">Will thrown an exception if the provided start state does not exist</exception>
        public Conversation CreateConversation(string startState = "default")
        {
            if (!States.ContainsKey(startState))
                throw new Exception("The specified start state does not exist: " + startState);

            return new Conversation(States.Keys.ToList(), startState);
        }

        /// <summary>
        /// Will get the current state of the conversation
        /// </summary>
        /// <param name="conversation"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Will get a response from an input
        /// </summary>
        /// <param name="conversation">The conversation to get the response in. Contains information about what has been said previously and what state the conversation is currently in</param>
        /// <param name="input">The text input to respond to</param>
        /// <returns>A list of strings that are the response messages. Will also modify the conversation object</returns>
        /// <exception cref="Exception"></exception>
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

        /// <summary>
        /// Train the bot with training data
        /// </summary>
        /// <param name="trainingData">The training data to train with. Can be obtained from the Parser class which parses a training data file to a training data object</param>
        /// <param name="progress">Optional parameter for providing a progress reporter. Use an instance of Progress<BotTrainingProgress></param>
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

        /// <summary>
        /// Train the bot with training data asynchronously
        /// </summary>
        /// <param name="trainingData">The training data to train with. Can be obtained from the Parser class which parses a training data file to a training data object</param>
        /// <param name="progress">Optional parameter for providing a progress reporter. Use an instance of Progress<BotTrainingProgress></param>
        public async Task TrainAsync(TrainingData trainingData, IProgress<BotTrainingProgress>? progress = null)
        {
            Task trainingTask = Task.Run(() => { Train(trainingData, progress); });
            await trainingTask;
        }

        /// <summary>
        /// Will return this bot as a big byte array containing everything that is needed to recreate the bot at a later point in time. Usefull for saving a trained bot so that it doesn't have to be retrained every time it the program restarts or similar
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Will load a bot from a byte array that was created with the GetAsBytes() method
        /// </summary>
        /// <param name="bytes"></param>
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
