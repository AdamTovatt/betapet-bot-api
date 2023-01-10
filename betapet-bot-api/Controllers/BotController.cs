using BetapetBot;
using BetapetBotApi.FrontendModels;
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
        [HttpPost("handleEverything")]
        public async Task<IActionResult> HandleEverything(string username, string password)
        {
            try
            {
                string connectionString = ConnectionStringHelper.GetConnectionStringFromUrl(EnvironmentHelper.GetEnvironmentVariable("DATABASE_URL"), SslMode.Prefer);
                Bot bot = new Bot(username, password, "FF1912DED13658C431A222B5A7EA1D6DC6569E2C1A11E185FF81E7823C896B46", connectionString);

                await bot.AcceptAllMatchRequests();

                List<BetapetBot.GameSummary> matchHandlingResponse = await bot.HandleAllMatches();

                await bot.UpdateRating();
                await bot.HandleChats();

                return new ApiResponse(matchHandlingResponse);
            }
            catch (ApiException exception)
            {
                return new ApiResponse(exception);
            }
        }

        [HttpGet("botStatus")]
        public async Task<IActionResult> GetMatches(string username, string password)
        {
            try
            {
                string connectionString = ConnectionStringHelper.GetConnectionStringFromUrl(EnvironmentHelper.GetEnvironmentVariable("DATABASE_URL"), SslMode.Prefer);

                if (string.IsNullOrEmpty(username))
                    username = EnvironmentHelper.GetEnvironmentVariable("USERNAME");
                if (string.IsNullOrEmpty(password))
                    password = EnvironmentHelper.GetEnvironmentVariable("PASSWORD");

                Bot bot = new Bot(username, password, "FF1912DED13658C431A222B5A7EA1D6DC6569E2C1A11E185FF81E7823C896B46", connectionString);

                List<Game> games = new List<Game>();

                List<Betapet.Models.Game> betapetGames = await bot.GetGamesAsync();
                for (int i = 0; i < betapetGames.Count; i++)
                {
                    games.Add(new Game(betapetGames[i]));
                }

                return new ApiResponse(new { games });
            }
            catch (ApiException exception)
            {
                return new ApiResponse(exception);
            }
        }

        [HttpGet("gameSummaries")]
        public async Task<IActionResult> GetGameSummaries()
        {
            try
            {
                string connectionString = ConnectionStringHelper.GetConnectionStringFromUrl(EnvironmentHelper.GetEnvironmentVariable("DATABASE_URL"), SslMode.Prefer);

                string username = EnvironmentHelper.GetEnvironmentVariable("USERNAME");
                string password = EnvironmentHelper.GetEnvironmentVariable("PASSWORD");

                Bot bot = new Bot(username, password, "FF1912DED13658C431A222B5A7EA1D6DC6569E2C1A11E185FF81E7823C896B46", connectionString);

                List<Betapet.Models.Game> betapetGames = await bot.GetGamesAsync();
                List<FrontendModels.GameSummary> gameSummaries = new List<FrontendModels.GameSummary>();

                foreach (Betapet.Models.Game game in betapetGames)
                {
                    gameSummaries.Add(new FrontendModels.GameSummary(game, bot.Betapet));
                }

                return new ApiResponse(gameSummaries);
            }
            catch (ApiException exception)
            {
                return new ApiResponse(exception);
            }
        }

        [HttpGet("rating")]
        public async Task<IActionResult> GetRating()
        {
            try
            {
                string connectionString = ConnectionStringHelper.GetConnectionStringFromUrl(EnvironmentHelper.GetEnvironmentVariable("DATABASE_URL"), SslMode.Prefer);

                string username = EnvironmentHelper.GetEnvironmentVariable("USERNAME");
                string password = EnvironmentHelper.GetEnvironmentVariable("PASSWORD");

                Bot bot = new Bot(username, password, "FF1912DED13658C431A222B5A7EA1D6DC6569E2C1A11E185FF81E7823C896B46", connectionString);

                return new ApiResponse(await bot.Database.GetRatingPointsAsync());
            }
            catch (ApiException exception)
            {
                return new ApiResponse(exception);
            }
        }
    }
}