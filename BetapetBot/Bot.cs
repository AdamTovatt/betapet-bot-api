using Betapet;
using Betapet.Models.Communication;
using Betapet.Models.Communication.Responses;

namespace BetapetBot
{
    public class Bot
    {
        BetapetManager betapet;

        public Bot(string username, string password, string deviceId)
        {
            betapet = new BetapetManager(username, password, deviceId);
        }

        public async Task<string> GetMessage()
        {
            RequestResponse message = await betapet.LoginAsync();
            RequestResponse response = await betapet.GetFriends();
            RequestResponse games = await betapet.GetGameAndUserList();
            
            
            return string.Format("authkey: {0}, userid: {1}", ((LoginResponse)message.InnerResponse).AuthKey, ((LoginResponse)message.InnerResponse).UserId);
        }
    }
}