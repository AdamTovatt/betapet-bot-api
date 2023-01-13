using Microsoft.AspNetCore.Mvc.ViewFeatures;
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
        private static Neuron emptyNeuron = new Neuron(0);

        static void Main(string[] args)
        {
            SecretAppsettingReader secrets = new SecretAppsettingReader();
            SecretValues secretValues = secrets.ReadSection<SecretValues>("Configuration");

            Database database = new Database(ConnectionStringHelper.GetConnectionStringFromUrl(secretValues.DatabaseUrl, WebApiUtilities.Helpers.SslMode.Prefer));

            List<ChatMessage> messages = CleanMessages(database.GetChatMessages(), characters, blackListedUserIds);

            List<Conversation> conversations = GetConversations(messages);
            List<string> possibleAnswerWords = GetPossibleAnswerWords(conversations);
            int wordsToAnswerWith = 5;
            int maxInputLength = 22;

            List<TrainingSet> trainingSets = new List<TrainingSet>();
            conversations.ForEach(x => trainingSets.Add(GetTrainingSet(x, possibleAnswerWords, maxInputLength, characters, wordsToAnswerWith)));

            NeuralNetwork network = CreateNetwork(maxInputLength, characters.Length, 2, 0.8, possibleAnswerWords.Count * wordsToAnswerWith);
            network.Mutate(0.4, new Random());

            for (int i = 0; i < 10; i++)
            {
                foreach (TrainingSet trainingSet in trainingSets)
                    network.UpdateWeights(trainingSet.Input, trainingSet.Output, 0.3);
            }

            while (true)
            {
                Console.WriteLine("Enter input");
                Console.WriteLine(OutputWord.GetAsString(ParseOutput(network.RunCalculations(CreateInput(Console.ReadLine(), characters, maxInputLength)), wordsToAnswerWith, possibleAnswerWords)));
                Console.WriteLine("\n");
            }

            Console.ReadLine();
        }

        private static List<OutputWord> ParseOutput(Neuron[] neurons, int wordsToAnswerWith, List<string> possibleAnswerWords)
        {
            List<OutputWord> result = new List<OutputWord>();

            for (int i = 0; i < wordsToAnswerWith; i++)
            {
                double maxCertainty = 0;
                int maxCertaintyIndex = 0;

                for (int j = 0; j < possibleAnswerWords.Count; j++)
                {
                    double value = neurons[i * possibleAnswerWords.Count + j].value;

                    if(value > maxCertainty)
                    {
                        maxCertainty = value;
                        maxCertaintyIndex = j;
                    }
                }

                result.Add(new OutputWord() { Certainty = maxCertainty, Text = possibleAnswerWords[maxCertaintyIndex] });
            }

            return result;
        }

        private static double[] CreateInput(string inputText, string charactersToUse, int maxInputLength)
        {
            double[] input = new double[charactersToUse.Length * maxInputLength];

            int characterCount = 0;
            foreach (char character in inputText)
            {
                if (characterCount >= maxInputLength)
                    break;

                int characterIndex = charactersToUse.IndexOf(character);

                if (characterIndex < 0)
                    throw new Exception("Found invalid character in input. This is because the messages used to create this input has not been cleaned to comply with the characters to use");

                input[characterCount * charactersToUse.Length + characterIndex] = 1;

                characterCount++;
            }

            return input;
        }

        private static TrainingSet GetTrainingSet(Conversation conversation, List<string> possibleAnswerWords, int maxInputLength, string charactersToUse, int wordsToAnswerWith)
        {
            double[] input = CreateInput(conversation.Promt, charactersToUse, maxInputLength);
            Neuron[] output = new Neuron[possibleAnswerWords.Count * wordsToAnswerWith];

            int characterCount = 0;
            foreach (char character in conversation.Promt)
            {
                if (characterCount >= maxInputLength)
                    break;

                int characterIndex = charactersToUse.IndexOf(character);

                if (characterIndex < 0)
                    throw new Exception("Found invalid character in input. This is because the messages used to create this input has not been cleaned to comply with the characters to use");

                input[characterCount * charactersToUse.Length + characterIndex] = 1;

                characterCount++;
            }

            for (int i = 0; i < output.Length; i++)
            {
                output[i] = emptyNeuron;
            }

            int wordCount = 0;
            foreach (string word in conversation.Response.Split())
            {
                if (wordCount >= wordsToAnswerWith)
                    break;

                int wordIndex = possibleAnswerWords.IndexOf(word);

                if (wordIndex < 0)
                    throw new Exception("Word not found in possible answer words, the list of possible words has been incorrectly generated or this conversation has not been part of generating it");

                output[wordCount * possibleAnswerWords.Count + wordIndex] = new Neuron(1);

                wordCount++;
            }

            return new TrainingSet() { Input = input, Output = output };
        }

        private static List<string> GetPossibleAnswerWords(List<Conversation> conversations)
        {
            HashSet<string> result = new HashSet<string>();

            foreach (Conversation conversation in conversations)
            {
                foreach (string word in conversation.Response.Split())
                {
                    result.Add(word);
                }
            }

            return result.ToList();
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