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

            MoveEvaluation evaluation1 = await EvaluateMoveAsync(move1, game);

            //SendChatResponse chatResponse = (SendChatResponse)(await betapet.SendChatMessage(game.Id, "du är noob")).InnerResponse;
            RequestResponse getChatResponse = await betapet.GetChatMessagesAsync(game);

            //PlayMoveResponse playResponse = (PlayMoveResponse)(await betapet.PlayMoveAsync(move3, game)).InnerResponse;

            return string.Format("authkey: {0}, userid: {1}", ((LoginResponse)message.InnerResponse).AuthKey, ((LoginResponse)message.InnerResponse).UserId);
        }

        public async Task<List<string>> GetPossibleWords(string letters)
        {
            return await lexicon.GetPossibleWordsAsync(letters);
        }

        public async Task GetPossibleWords(List<string> wordOfLetters)
        {
            using (NpgsqlConnection connection = await lexicon.GetConnectionAsync())
            {
                foreach (string letters in wordOfLetters)
                {
                    await lexicon.GetPossibleWordsAsync(letters, connection);
                }
            }
        }

        public async Task<MoveEvaluation> EvaluateMoveAsync(Move move, Game game)
        {
            if (move.Tiles.Count == 0)
                return MoveEvaluation.ImpossibleMove;

            if (!game.Hand.ContainsTiles(move.Tiles))
                return MoveEvaluation.ImpossibleMove;

            foreach (Tile tile in move.Tiles)
            {
                if (game.Board.Tiles[tile.X, tile.Y].Type == TileType.Letter)
                    return MoveEvaluation.ImpossibleMove;
            }

            List<List<Tile>> words = new List<List<Tile>>();

            Direction moveDirection = Direction.None;

            if (move.Tiles.Count > 1)
            {
                moveDirection = move.Tiles[0].X > move.Tiles[1].X ? Direction.Horizontal : Direction.Vertical;
            }

            foreach (Tile tile in move.Tiles)
            {
                words.Add(game.Board.ScanForTiles(move, tile, moveDirection == Direction.Horizontal ? Direction.Vertical : Direction.Horizontal));
            }

            words.Add(game.Board.ScanForTiles(move, move.Tiles[0], moveDirection));

            if (!words.Any(x => x.Count > 1))
                return MoveEvaluation.ImpossibleMove;

            List<int> pointsPerWord = new List<int>();
            UniqueTileCollection multiplyTiles = new UniqueTileCollection();

            foreach (List<Tile> word in words)
            {
                if (!await lexicon.GetWordExistsAsync(word.ToTileString()))
                    return MoveEvaluation.ImpossibleMove;

                int wordPoints = 0;
                foreach (Tile tile in word)
                {
                    wordPoints += tile.PointValue * game.Board.GetPositionLetterMultiplier(tile.X, tile.Y);

                    if (game.Board.GetPositionWordMultiplier(tile.X, tile.Y) > 1)
                        multiplyTiles.AddTile(game.Board.Tiles[tile.X, tile.Y]);
                }

                foreach (Tile tile in multiplyTiles.Tiles)
                    wordPoints *= tile.NumericValue;

                pointsPerWord.Add(wordPoints);
                multiplyTiles.Clear();
            }

            return new MoveEvaluation(true, pointsPerWord.Sum());
        }
    }
}