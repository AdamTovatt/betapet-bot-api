using Betapet;
using Betapet.Helpers;
using Betapet.Models;
using Betapet.Models.Communication;
using Betapet.Models.Communication.Responses;
using Betapet.Models.InGame;
using Npgsql;

namespace BetapetBot
{
    public class Bot
    {
        private BetapetManager betapet;
        private Lexicon lexicon;

        public Bot(string username, string password, string deviceId, string connectionString)
        {
            betapet = new BetapetManager(username, password, deviceId);
            lexicon = new Lexicon(connectionString);
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

            List<string> possibleWords = await GetPossibleWords(game.Hand.ToTileString());

            Move move1 = new Move();
            move1.AddTile("M", 6, 5);
            move1.AddTile("Å", 6, 6);

            MoveEvaluation evaluation1 = game.EvaluateMove(move1);

            //SendChatResponse chatResponse = (SendChatResponse)(await betapet.SendChatMessage(game.Id, "du är noob")).InnerResponse;
            RequestResponse getChatResponse = await betapet.GetChatMessagesAsync(game);

            //PlayMoveResponse playResponse = (PlayMoveResponse)(await betapet.PlayMoveAsync(move3, game)).InnerResponse;

            return string.Format("authkey: {0}, userid: {1}", ((LoginResponse)message.InnerResponse).AuthKey, ((LoginResponse)message.InnerResponse).UserId);
        }

        public async Task<List<string>> GetPossibleWords(string letters)
        {
            return await lexicon.GetPossibleWords(letters);
        }

        public async Task GetPossibleWords(List<string> wordOfLetters)
        {
            using (NpgsqlConnection connection = await lexicon.GetConnection())
            {
                foreach (string letters in wordOfLetters)
                {
                    await lexicon.GetPossibleWords(letters, connection);
                }
            }
        }
    }
}