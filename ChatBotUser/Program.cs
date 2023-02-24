using ChatBot.Models;
using ChatBot.Models.Data;
using ChatBot.Models.Data.Parsing;
using ChatBot.Models.Prediction;
using ChatBot.Services;

namespace ChatBotUser
{
    public class ConsoleChatBot
    {
        public static void Main()
        {
            Console.WriteLine("Train?");
            HandleBot(Console.ReadLine()!.ToLower().Contains("y")).Wait();
        }

        public static async Task HandleBot(bool train)
        {
            FileTrainingDataProvider fileTrainingDataProvider = new FileTrainingDataProvider("c:\\users\\adam\\code\\betapet-bot-api\\chatbot\\Chapter1.txt");
            string trainingData = await fileTrainingDataProvider.GetTrainingDataAsync();

            ParseResult parseResult = Parser.ParseTrainingData(trainingData);

            Bot bot = new Bot(new SdcaPredictionServiceRepository());

            if (train)
            {
                if (parseResult.Error != null || parseResult.Data == null)
                {
                    Console.WriteLine(parseResult.Error);
                    return;
                }
                else
                {
                    //training code
                    Progress<BotTrainingProgress> progress = new Progress<BotTrainingProgress>();
                    progress.ProgressChanged += (sender, updatedProgress) => { Console.Clear(); Console.WriteLine(updatedProgress); };
                    await bot.TrainAsync(parseResult.Data, progress);

                    File.WriteAllBytes("model.bin", bot.GetAsBytes());

                    Thread.Sleep(100);

                    Console.Clear();
                }
            }
            else
            {
                bot.Load(File.ReadAllBytes("model.bin"));
            }

            Conversation conversation = bot.CreateConversation();

            Console.WriteLine(bot.Start(conversation) + "\n");

            while (true)
            {
                Console.WriteLine("\nYou: ");
                string? input = Console.ReadLine();

                if (input != null)
                {
                    Console.WriteLine("\nBot: ");
                    foreach (string response in bot.GetResponse(conversation, input))
                    {
                        Console.WriteLine(response);
                    }
                }

                Console.WriteLine();
            }
        }
    }
}