using Betapet;
using Betapet.Models;
using Betapet.Models.Communication;
using Betapet.Models.Communication.Responses;
using Betapet.Models.InGame;

namespace BetapetBot
{
    public class Bot
    {
        private BetapetManager betapet;
        private Lexicon lexicon;

        public Bot(string username, string password, string deviceId)
        {
            betapet = new BetapetManager(username, password, deviceId);
        }

        public void AddLexicon(List<string> words)
        {

        }

        public async Task<string> GetMessage()
        {
            RequestResponse message = await betapet.LoginAsync();
            RequestResponse response = await betapet.GetFriendsAsync();
            RequestResponse games = await betapet.GetGameAndUserListAsync();
            Board board = ((GamesAndUserListResponse)games.InnerResponse).Games[0].Board;

            Game game = ((GamesAndUserListResponse)games.InnerResponse).Games[0];

            /* play move
            Move move = new Move(game.Id, game.Turn);
            move.AddTile(new Tile("J", 7, 4));
            move.AddTile(new Tile("U", 7, 5));
            move.AddTile(new Tile("S", 7, 6));

            RequestResponse playResponse = await betapet.PlayMove(move);
            */

            Move move1 = new Move();
            move1.AddTile("M", 6, 5);
            move1.AddTile("Å", 6, 6);

            //MoveEvaluation evaluation1 = game.EvaluateMove(move1);

            Move move2 = new Move();
            move2.AddTile("E", 10, 8);
            move2.AddTile("L", 11, 8);  

            //MoveEvaluation evaluation2 = game.EvaluateMove(move2);

            Move move3 = new Move();
            move3.AddTile("U", 10, 10);
            move3.AddTile("F", 10, 11);

            MoveEvaluation evaluation3 = game.EvaluateMove(move3);

            //SendChatResponse chatResponse = (SendChatResponse)(await betapet.SendChatMessage(game.Id, "du är noob")).InnerResponse;
            RequestResponse getChatResponse = await betapet.GetChatMessagesAsync(game);

            //PlayMoveResponse playResponse = (PlayMoveResponse)(await betapet.PlayMoveAsync(move3, game)).InnerResponse;

            return string.Format("authkey: {0}, userid: {1}", ((LoginResponse)message.InnerResponse).AuthKey, ((LoginResponse)message.InnerResponse).UserId);
        }
    }
}