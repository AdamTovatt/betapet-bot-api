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
        //private const string trainingDataUrl = "https://docs.google.com/document/d/1OdQ3M7j9gjKa4s5mZmK-5XogeGp_87MsTCuEIAVrjt8/export?format=txt";
        private const string trainingDataUrl = "https://docs.google.com/document/d/10lIUjb6ww8uxYNFv74RQXEwdDQyTsY5roqk425PZMHc/export?format=txt";

        static void Main(string[] args)
        {
            SecretAppsettingReader secrets = new SecretAppsettingReader();
            SecretValues secretValues = secrets.ReadSection<SecretValues>("Configuration");

            //get database connection
            //Database database = new Database(ConnectionStringHelper.GetConnectionStringFromUrl(secretValues.DatabaseUrl, WebApiUtilities.Helpers.SslMode.Prefer));

            //get cleaned messages
            //List<ChatMessage> messages = CleanMessages(database.GetChatMessages(), characters, blackListedUserIds);

            //create training data
            //List<Conversation> conversations = GetConversations(messages);
            //List<Conversation> conversations = LoadConversations("chatData.txt");
            Console.WriteLine("Getting training data");

            DocumentData documentData = new DocumentData(LoadTrainingDocument(trainingDataUrl, true));
            List<Conversation> conversations = documentData.Conversations;

            Console.WriteLine("Starting Training");
            //create and train model
            ConversationTrainingService trainingService = new ConversationTrainingService();
            trainingService.Train(conversations, "model.zip");

            //load model
            ConversationService conversationService = new ConversationService();
            conversationService.LoadModel("model.zip");

            //database.SaveModel(File.ReadAllBytes("model.zip"), "chat_model_boring");

            //conversationService.LoadModel(database.ReadModel("chat_model"));

            while (true)
            {
                Console.WriteLine("Enter input");
                string input = Console.ReadLine();
                string prediction = MakePrediction(conversationService, input);
                
                if(documentData.ResponseData.Length > 0)
                {
                    Console.WriteLine(documentData.GetResponse(prediction));
                }
                else
                {
                    Console.WriteLine(prediction);
                }

                Console.WriteLine("\n");
            }

            Console.ReadLine();
        }

        private static List<Conversation> ParseConversations(string[] lines)
        {
            List<Conversation> result = new List<Conversation>();

            Conversation conversation = null;

            int type = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrEmpty(lines[i]))
                    continue;

                if (type == 0)
                {
                    string line = lines[i].ToLower();

                    if (!line.StartsWith("q:"))
                        throw new Exception("Error in training data! Expected the beggning of an answer with \"Q:\" but instead got: " + line);
                    conversation = new Conversation() { Promt = line.Substring(2) };
                }
                if (type == 1)
                {
                    string line = lines[i].ToLower();

                    if (!line.StartsWith("a:"))
                        throw new Exception("Error in training data! Expected the beggning of an answer with \"A:\" but instead got: " + line);

                    conversation.Response = line.Substring(2);
                    result.Add(conversation);
                }

                type++;
                if (type == 2)
                    type = 0;
            }

            return result;
        }

        private static string LoadTrainingDocument(string path)
        {
            return File.ReadAllText(path);
        }

        private static string LoadTrainingDocument(string url, bool isFromUrl)
        {
            HttpClient client = new HttpClient();

            HttpResponseMessage response = client.GetAsync(url).Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return response.Content.ReadAsStringAsync().Result;

            throw new Exception("Error when reading training data google docs, url: " + url);
        }

        private static void SaveConversations(List<Conversation> conversations, string path)
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach(Conversation conversation in conversations)
            {
                stringBuilder.AppendLine(conversation.Promt);
                stringBuilder.AppendLine(conversation.Response);
                stringBuilder.AppendLine();
            }

            File.WriteAllText(path, stringBuilder.ToString());
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