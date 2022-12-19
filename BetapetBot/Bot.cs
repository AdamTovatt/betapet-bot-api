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

            Game game = ((GamesAndUserListResponse)games.InnerResponse).Games[0];


            if (game.OurTurn)
            {
                List<WordLine> wordLines = GetWordLines(game);

                List<Move> moves = await GenerateMovesFromWordLinesAsync(game, wordLines);

                foreach (Move move in moves)
                {
                    RequestResponse playRequestResponse = await betapet.PlayMoveAsync(move, game);
                    if (playRequestResponse != null && playRequestResponse.Success)
                        return string.Format("Played \"{0}\" for {1} points. Server response: {2}", move.Tiles.ToTileString(), move.Evaluation.Points, ((PlayMoveResponse)playRequestResponse.InnerResponse).Code);
                }
            }
            else
            {
                return "Not our turn";
            }

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

        public async Task<List<Move>> GenerateMovesFromWordLinesAsync(Game game, List<WordLine> wordLines)
        {
            List<Move> moves = new List<Move>();

            using (NpgsqlConnection connection = await lexicon.GetConnectionAsync())
            {
                foreach (WordLine wordLine in wordLines)
                {
                    List<string> possibleWords = await lexicon.GetPossibleWordsAsync(wordLine.Letters.AddTiles(game.Hand).ToTileString());

                    foreach (string candidateWord in possibleWords)
                    {
                        for (int startPositionOffset = 0; startPositionOffset < 15; startPositionOffset++)
                        {
                            int shiftedX = wordLine.StartPosition.X + (wordLine.Direction == Direction.Horizontal ? startPositionOffset : 0);
                            int shiftedY = wordLine.StartPosition.Y + (wordLine.Direction == Direction.Vertical ? startPositionOffset : 0);

                            Move move = CreateMoveFromPosition(new Position(shiftedX, shiftedY), candidateWord, wordLine.Direction);
                            if (move != null)
                            {
                                MoveEvaluation evaluation = await EvaluateMoveAsync(move, game, connection);

                                if (evaluation.Possible)
                                {
                                    move.Evaluation = evaluation;
                                    moves.Add(move);
                                }
                            }
                        }
                    }
                }
            }

            moves = moves.OrderByDescending(move => move.Evaluation.Points).ToList();

            return moves;
        }

        private Move CreateMoveFromPosition(Position position, string word, Direction direction)
        {
            Move move = new Move();

            if ((direction == Direction.Horizontal ? position.X : position.Y) + word.Length > 14)
                return null;

            for (int offset = 0; offset < word.Length; offset++)
            {
                int x = position.X + (direction == Direction.Horizontal ? offset : 0);
                int y = position.Y + (direction == Direction.Vertical ? offset : 0);

                move.AddTile(word[offset].ToString(), x, y);
            }

            return move;
        }

        public List<WordLine> GetWordLines(Game game)
        {
            List<WordLine> wordLines = new List<WordLine>();

            List<Position> startPositionsX = new List<Position>();
            List<Position> startPositionsY = new List<Position>();
            bool preX = false;
            bool preY = false;

            Position lastAddedPositionX = null;
            Position lastAddedPositionY = null;

            for (int x = 0; x < 15; x++)
            {
                bool found = false;
                for (int y = 0; y < 15; y++)
                {
                    Tile currentTile = game.Board.Tiles[x, y];
                    if (currentTile.Type == TileType.Letter)
                    {
                        int startY = currentTile.Y - game.Hand.Count;
                        if (startY < 0)
                            startY = 0;

                        lastAddedPositionX = new Position(currentTile.X, startY);
                        startPositionsX.Add(lastAddedPositionX);

                        if (!preX)
                        {
                            startPositionsX.Add(new Position(currentTile.X - 1, startY));
                            preX = true;
                        }

                        found = true;
                        break;
                    }
                }

                if (!found && preX)
                {
                    if (lastAddedPositionX != null)
                        startPositionsX.Add(new Position(lastAddedPositionX.X + 1, lastAddedPositionX.Y));

                    break;
                }
            }

            foreach (Position position in startPositionsX)
            {
                wordLines.Add(new WordLine(Direction.Vertical, position));
            }

            for (int y = 0; y < 15; y++)
            {
                bool found = false;
                for (int x = 0; x < 15; x++)
                {
                    Tile currentTile = game.Board.Tiles[x, y];
                    if (currentTile.Type == TileType.Letter)
                    {
                        int startX = currentTile.X - game.Hand.Count;
                        if (startX < 0)
                            startX = 0;

                        lastAddedPositionY = new Position(startX, currentTile.Y);
                        startPositionsY.Add(lastAddedPositionY);

                        if (!preY)
                        {
                            startPositionsY.Add(new Position(startX, currentTile.Y - 1));
                            preY = true;
                        }

                        found = true;
                        break;
                    }
                }

                if (!found && preY)
                {
                    if (lastAddedPositionY != null)
                        startPositionsY.Add(new Position(lastAddedPositionY.X, lastAddedPositionY.Y + 1));

                    break;
                }
            }

            foreach (Position position in startPositionsY)
            {
                wordLines.Add(new WordLine(Direction.Horizontal, position));
            }

            foreach (WordLine wordLine in wordLines)
            {
                for (int i = 0; i < 15; i++)
                {
                    int positionToCheck = wordLine.Direction == Direction.Horizontal ? wordLine.StartPosition.X : wordLine.StartPosition.Y;
                    positionToCheck += i;
                    if (positionToCheck > 14)
                        break;

                    Tile currentTile;

                    if (wordLine.Direction == Direction.Horizontal)
                        currentTile = game.Board.Tiles[positionToCheck, wordLine.StartPosition.Y];
                    else
                        currentTile = game.Board.Tiles[wordLine.StartPosition.X, positionToCheck];

                    if (currentTile.Type == TileType.Letter)
                        wordLine.Letters.Add(currentTile);
                }
            }

            return wordLines;
        }

        public async Task<MoveEvaluation> EvaluateMoveAsync(Move move, Game game, NpgsqlConnection sqlConnection = null)
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

            if (!move.Tiles.Any(t => game.Board.GetHasConnectedTiles(t)))
                return MoveEvaluation.ImpossibleMove;

            List<List<Tile>> words = new List<List<Tile>>();

            Direction moveDirection = Direction.None;

            if (move.Tiles.Count > 1)
            {
                moveDirection = move.Tiles[0].X > move.Tiles[1].X ? Direction.Horizontal : Direction.Vertical;
            }

            foreach (Tile tile in move.Tiles)
            {
                if (moveDirection != Direction.None)
                    words.Add(game.Board.ScanForTiles(move, tile, moveDirection == Direction.Horizontal ? Direction.Vertical : Direction.Horizontal));
            }

            if (moveDirection != Direction.None)
                words.Add(game.Board.ScanForTiles(move, move.Tiles[0], moveDirection));

            if (!words.Any(x => x.Count > 1))
                return MoveEvaluation.ImpossibleMove;

            List<int> pointsPerWord = new List<int>();
            UniqueTileCollection multiplyTiles = new UniqueTileCollection();

            NpgsqlConnection connection = null;

            try
            {
                connection = sqlConnection ?? await lexicon.GetConnectionAsync();

                foreach (List<Tile> word in words)
                {
                    if (!await lexicon.GetWordExistsAsync(word.ToTileString(), connection))
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
            }
            catch
            {
                throw;
            }
            finally
            {
                if (sqlConnection == null && connection != null)
                {
                    await connection.CloseAsync();
                    await connection.DisposeAsync();
                }
            }

            return new MoveEvaluation(true, pointsPerWord.Sum());
        }
    }
}