using BetapetBot;
using BetapetBotApi.Managers;
using Microsoft.AspNetCore.Mvc;
using Sakur.WebApiUtilities.Models;

namespace BetapetBotApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BotController : ControllerBase
    {
        [HttpGet("prediction")]
        public async Task<IActionResult> GetPrediction()
        {
            Bot bot = new Bot("DavidRdrgz", "gunnaral", "FF1912DED13658C431A222B5A7EA1D6DC6569E2C1A11E185FF81E7823C896B46");
            bot.AddLexicon(await (new DatabaseManager()).GetWordsInLexiconAsync());
            return new ApiResponse(await bot.GetMessage());
        }
    }
}