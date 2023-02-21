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
            HandleBot().Wait();
        }

        public static async Task HandleBot()
        {
            FileTrainingDataProvider fileTrainingDataProvider = new FileTrainingDataProvider("c:\\users\\adam\\code\\betapet-bot-api\\chatbot\\TrainingData.txt");
            string trainingData = await fileTrainingDataProvider.GetTrainingDataAsync();

            ParseResult parseResult = Parser.ParseTrainingData(trainingData);

            if (parseResult.Error != null || parseResult.Data == null)
            {
                Console.WriteLine(parseResult.Error);
                return;
            }
            else
            {
                Progress<BotTrainingProgress> progress = new Progress<BotTrainingProgress>();

                progress.ProgressChanged += (sender, updatedProgress) => { Console.Clear(); Console.WriteLine(updatedProgress); };

                Bot bot = new Bot(null);

                await bot.TrainAsync(parseResult.Data, new SdcaPredictionTrainingService(), progress);

                Thread.Sleep(100);

                Console.Clear();
                Console.WriteLine(bot.Start());

                while (true)
                {
                    Console.WriteLine("You: ");
                    string? input = Console.ReadLine();

                    if (input != null)
                    {
                        Console.WriteLine("Bot: ");
                        foreach (string response in await bot.GetResponsesAsync(input))
                        {
                            Console.WriteLine(response);
                        }
                    }

                    Console.WriteLine();
                }
            }
        }
    }
}