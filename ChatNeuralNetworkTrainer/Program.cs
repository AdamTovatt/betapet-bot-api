using Npgsql;
using ReadSecretsConsole;
using System.Text;
using TotteML;
using WebApiUtilities.Helpers;

namespace ChatNeuralNetworkTrainer
{
    internal class Program
    {
        private static int[] blackListedUserIds = { 814453 };
        private const string characters = " .,abcdefghijklmnopqrstuvwxyzåäö!?";

        static void Main(string[] args)
        {
            NeuralNetwork neuralNetwork = new NeuralNetwork();

            SecretAppsettingReader secrets = new SecretAppsettingReader();
            SecretValues secretValues = secrets.ReadSection<SecretValues>("Configuration");

            Database database = new Database(ConnectionStringHelper.GetConnectionStringFromUrl(secretValues.DatabaseUrl, WebApiUtilities.Helpers.SslMode.Prefer));

            List<ChatMessage> messages = CleanMessages(database.GetChatMessages(), characters, blackListedUserIds);

            List<Conversation> conversations = GetConversations(messages);

            Console.ReadLine();
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

        private static NeuralNetwork CreateNetwork(int textLength, int possibleLettersCount, int hiddenLayers, double hiddenLayerScale, int possibleOutputs)
        {
            int[] size = new int[hiddenLayers + 2];
            size[0] = textLength * possibleLettersCount;

            for (int i = 1; i < hiddenLayers + 1; i++)
            {
                size[i] = (int)(size[0] * hiddenLayerScale);
            }

            size[hiddenLayers + 1] = possibleOutputs;

            return new NeuralNetwork(size, 0.5);
        }
    }
}