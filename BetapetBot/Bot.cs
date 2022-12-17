using Betapet;
using Betapet.Models;
using Betapet.Models.Communication;
using Betapet.Models.Communication.Responses;
using Betapet.Models.InGame;

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
            Board board = ((GamesAndUserListResponse)games.InnerResponse).Games[0].Board;

            Game game = ((GamesAndUserListResponse)games.InnerResponse).Games[0];

            /* play move
            Move move = new Move(game.Id, game.Turn);
            move.AddTile(new Tile("J", 7, 4));
            move.AddTile(new Tile("U", 7, 5));
            move.AddTile(new Tile("S", 7, 6));

            RequestResponse playResponse = await betapet.PlayMove(move);
            */

            //SendChatResponse chatResponse = (SendChatResponse)(await betapet.SendChatMessage(game.Id, "du är noob")).InnerResponse;
            RequestResponse getChatResponse = await betapet.GetChatMessages(game);

            return string.Format("authkey: {0}, userid: {1}", ((LoginResponse)message.InnerResponse).AuthKey, ((LoginResponse)message.InnerResponse).UserId);
        }
    }
}