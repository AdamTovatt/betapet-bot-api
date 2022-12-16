using Betapet;
using Betapet.Helpers;
using Betapet.Models.Communication;
using Betapet.Models.Communication.Responses;

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
            RequestResponse message = await betapet.LoginAsync(Username, Password);
            RequestResponse response = await betapet.GetFriends();
            return string.Format("authkey: {0}, userid: {1}", ((LoginResponse)message.InnerResponse).AuthKey, ((LoginResponse)message.InnerResponse).UserId);
        }
    }
}