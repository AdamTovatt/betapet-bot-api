using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.ML;
using Npgsql;
using ReadSecretsConsole;
using System.Text;
using WebApiUtilities.Helpers;

namespace ChatNeuralNetworkTrainer
{
    internal class Program
    {
        private static int[] blackListedUserIds = { 814453 };
        private const string characters = " .,abcdefghijklmnopqrstuvwxyzåäö!?";

        static void Main(string[] args)
        {
            SecretAppsettingReader secrets = new SecretAppsettingReader();
            SecretValues secretValues = secrets.ReadSection<SecretValues>("Configuration");

            //get database connection
            Database database = new Database(ConnectionStringHelper.GetConnectionStringFromUrl(secretValues.DatabaseUrl, WebApiUtilities.Helpers.SslMode.Prefer));

            //get cleaned messages
            List<ChatMessage> messages = CleanMessages(database.GetChatMessages(), characters, blackListedUserIds);

            //create training data
            List<Conversation> conversations = GetConversations(messages);

            //create and train model
            //ConversationTrainingService trainingService = new ConversationTrainingService();
            //trainingService.Train(conversations, "model.zip");

            //load model
            ConversationService conversationService = new ConversationService();
            //conversationService.LoadModel("model.zip");

            database.SaveModel(File.ReadAllBytes("model.zip"), "chat_model");

            conversationService.LoadModel(database.ReadModel("chat_model"));

            while (true)
            {
                Console.WriteLine("Enter input");
                Console.WriteLine(MakePrediction(conversationService, Console.ReadLine()));
                Console.WriteLine("\n");
            }

            Console.ReadLine();
        }

        private static string MakePrediction(ConversationService labelService, string prompt)
        {
            Conversation conversation = new Conversation() { Promt = prompt };

            List<ConversationResponse> result = labelService.PredictResponse(conversation);

            return result[0].Text;
        }

        private static List<Conversation> GetConversations(List<ChatMessage> messages)
        {
            List<Conversation> conversations = new List<Conversation>();

            Dictionary<int, List<ChatMessage>> chats = new Dictionary<int, List<ChatMessage>>();

            foreach (ChatMessage message in messages)
            {
                if (chats.TryGetValue(message.MatchId, out List<ChatMessage> chat))
                {
                    chat.Add(message);
                }
                else
                {
                    chats.Add(message.MatchId, new List<ChatMessage> { message });
                }
            }

            foreach (int key in chats.Keys)
            {
                List<ChatMessage> chat = chats[key];

                if (chat.Count > 0)
                {
                    StringBuilder promt = new StringBuilder();
                    StringBuilder answer = new StringBuilder();

                    int firstId = chat[0].AuthorId;
                    bool isOnFirstId = true;

                    foreach (ChatMessage message in chat)
                    {
                        if (message.AuthorId == firstId)
                        {
                            if (isOnFirstId)
                            {
                                promt.Append(message.Text);
                                promt.Append(" ");
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            if (isOnFirstId)
                                isOnFirstId = false;

                            answer.Append(message.Text);
                            answer.Append(" ");
                        }
                    }

                    Conversation conversation = new Conversation()
                    {
                        Promt = promt.ToString().Trim(),
                        Response = answer.ToString().Trim(),
                    };

                    if (conversation.Promt.Length > 0 && conversation.Response.Length > 0)
                        conversations.Add(conversation);
                }
            }

            return conversations;
        }

        private static List<ChatMessage> CleanMessages(List<ChatMessage> messagesToClean, string charactersToAllow, int[] bannedUsers)
        {
            messagesToClean = messagesToClean.Where(x => !bannedUsers.Contains(x.AuthorId)).ToList();
            messagesToClean.ForEach(x => x.Text = x.Text.ToLower());

            foreach (ChatMessage message in messagesToClean)
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (char character in message.Text.Where(x => charactersToAllow.Contains(x)))
                    stringBuilder.Append(character);
                message.Text = stringBuilder.ToString().Trim();
            }

            return messagesToClean;
        }
    }
}