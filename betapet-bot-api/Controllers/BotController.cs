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
            Bot bot = new Bot("Orvar", "pastry_boy2");
            return new ApiResponse(bot.GetMessage());
        }
    }
}