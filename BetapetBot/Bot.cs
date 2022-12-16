using Betapet;
using Betapet.Helpers;
using Betapet.Models.Communication;

namespace BetapetBot
{
    public class Bot
    {
        public string Username { get; private set; }
        public string Password { get; private set; }

        BetapetManager betapet;

        public Bot(string username, string password)
        {
            Username = username;
            Password = password;

            betapet = new BetapetManager();
        }

        public async Task<string> GetMessage()
        {
            string message = await betapet.LoginAsync(Username, Password);
            return string.Format("{0} login result", message);
        }
    }
}