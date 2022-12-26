using BetapetBot;
using BetapetBotApi.Helpers;
using Microsoft.AspNetCore.Mvc;
using Sakur.WebApiUtilities.Models;
using System.Diagnostics;
using WebApiUtilities.Helpers;

namespace BetapetBotApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BotController : ControllerBase
    {
        [HttpGet("handleMatches")]
        public async Task<IActionResult> HandleMatches(string username, string password)
        {
            try
            {
                string connectionString = ConnectionStringHelper.GetConnectionStringFromUrl(EnvironmentHelper.GetEnvironmentVariable("DATABASE_URL"), SslMode.Prefer);
                Bot bot = new Bot(username, password, "FF1912DED13658C431A222B5A7EA1D6DC6569E2C1A11E185FF81E7823C896B46", connectionString);

                await bot.AcceptAllMatchRequests();

                List<string> matchHandlingResponse = await bot.HandleAllMatches();
                return new ApiResponse(matchHandlingResponse);
            }
            catch (ApiException exception)
            {
                return new ApiResponse(exception);
            }
        }

        [HttpGet("prediction")]
        public async Task<IActionResult> GetPrediction()
        {
            string connectionString = ConnectionStringHelper.GetConnectionStringFromUrl(EnvironmentHelper.GetEnvironmentVariable("DATABASE_URL"), SslMode.Prefer);
            Bot bot = new Bot("DavidRdrgz", "gunnaral", "FF1912DED13658C431A222B5A7EA1D6DC6569E2C1A11E185FF81E7823C896B46", connectionString);
            return new ApiResponse(await bot.GetMessage());
        }

        [HttpGet("lexiconBenchMark")]
        public async Task<IActionResult> GetBenchMark(int count, int length)
        {
            if(count == 0 || length == 0)
                return new ApiResponse(new ApiException("Missing count and/or length in query parameter", System.Net.HttpStatusCode.BadRequest));

            string connectionString = ConnectionStringHelper.GetConnectionStringFromUrl(EnvironmentHelper.GetEnvironmentVariable("DATABASE_URL"), SslMode.Prefer);
            Bot bot = new Bot("DavidRdrgz", "gunnaral", "FF1912DED13658C431A222B5A7EA1D6DC6569E2C1A11E185FF81E7823C896B46", connectionString);

            List<string> wordsOfLetters = new List<string>();

            for (int i = 0; i < count; i++)
            {
                wordsOfLetters.Add(StringGenerator.GetRandomString(length));
            }

            Stopwatch stopwatch = Stopwatch.StartNew();
            await bot.GetPossibleWords(wordsOfLetters);
            stopwatch.Stop();

            return new ApiResponse(new { totalTime = stopwatch.Elapsed.TotalMilliseconds, averageTime = (float)stopwatch.Elapsed.TotalMilliseconds / (float)count });
        }
    }
}