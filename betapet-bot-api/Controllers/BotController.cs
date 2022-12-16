using BetapetBot;
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
            Bot bot = new Bot("DavidRdrgz", "gunnaral");
            return new ApiResponse(bot.GetMessage());
        }
    }
}