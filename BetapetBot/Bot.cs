using Betapet;
using Betapet.Helpers;
using Betapet.Models;
using Betapet.Models.Communication;
using Betapet.Models.Communication.Responses;
using Betapet.Models.InGame;
using BetapetBot.Chat;
using Microsoft.VisualBasic;
using Npgsql;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace BetapetBot
{
    public class Bot
    {
        private BetapetManager betapet;
        private Lexicon lexicon;
        private Database database;

        public BetapetManager Betapet { get { return betapet; } }
        public Database Database { get { return database; } }
        public ChatHelper ChatHelper { get; private set; }

        public static int AverageTimePerMove { get; private set; }

        public Bot(string username, string password, string deviceId, string connectionString)
        {
            betapet = new BetapetManager(username, password, deviceId);
            lexicon = new Lexicon(connectionString);
            database = new Database(connectionString);
        }

        public async Task LoadChatHelperAsync()
        {
            try
            {
                ChatHelper = new ChatHelper(await database.ReadModelAsync("chat_model_boring"));
            }
            catch
            {
                throw new Exception("Error when creating chat helper");
            }
        }

        public async Task AcceptAllMatchRequests()
        {
            await betapet.LoginAsync();

            RequestResponse matchesRequestRespone = await betapet.GetMatchRequestsAsync();

            if (!matchesRequestRespone.Success)
                return; //maybe we should do something??

            MatchRequestResponse matchResponse = (MatchRequestResponse)matchesRequestRespone.InnerResponse;

            if (matchResponse.MatchRequests == null)
                return; //maybe do something?

            foreach (MatchRequestResponseItem requestItem in matchResponse.MatchRequests)
            {
                await betapet.AcceptMatchRequestAsync(requestItem.GameId);
            }
        }

        public async Task<List<Game>> GetGamesAsync()
        {
            await betapet.LoginAsync();
            RequestResponse requestResponse = await betapet.GetGameAndUserListAsync();

            GamesAndUserListResponse gameResponse = (GamesAndUserListResponse)requestResponse.InnerResponse;
            return gameResponse.Games;
        }

        public async Task UpdateRating()
        {
            try
            {
                if (!betapet.LoggedIn)
                    await betapet.LoginAsync();

                RequestResponse requestResponse = await betapet.GetGameAndUserListAsync();
                GamesAndUserListResponse gameResponse = (GamesAndUserListResponse)requestResponse.InnerResponse;

                User us = gameResponse.Users.Where(x => x.Id == betapet.UserId).FirstOrDefault();

                if (us == null)
                    return;

                int currentRating = us.Rating;
                if (await database.GetLastRating() == currentRating)
                    return;

                await database.AddRating(currentRating);
            }
            catch
            {
                return;
            }
        }

        public async Task<List<ChatScenario>> GetChatScenariosAsync()
        {
            List<ChatScenario> chatScenarios = new List<ChatScenario>();

            if (!betapet.LoggedIn)
                await betapet.LoginAsync();

            RequestResponse requestResponse = await betapet.GetGameAndUserListAsync();
            GamesAndUserListResponse gameResponse = (GamesAndUserListResponse)requestResponse.InnerResponse;

            using (NpgsqlConnection connection = await database.GetConnectionAsync())
            {
                foreach (Game game in gameResponse.Games)
                {
                    if (game.LastChatTime != game.StartTime)
                    {
                        RequestResponse chatMessagesResponse = await betapet.GetChatMessagesAsync(game);
                        if (chatMessagesResponse.Success)
                        {
                            GetChatResponse chatResponse = (GetChatResponse)chatMessagesResponse.InnerResponse;

                            StringBuilder theirText = new StringBuilder();
                            StringBuilder ourText = new StringBuilder();
                            bool opponentMessage = chatResponse.Messages[0].UserId != betapet.UserId;
                            bool ourMessage = chatResponse.Messages[0].UserId == betapet.UserId;
                            bool didAddMessage = false;

                            foreach (ChatMessage message in chatResponse.Messages)
                            {
                                if ((DateTime.Now - message.Created).TotalHours < 24)
                                {
                                    if (message.UserId == betapet.UserId) //our message
                                    {
                                        if (opponentMessage)
                                            ourText.Clear();

                                        opponentMessage = false;
                                        ourMessage = true;
                                        didAddMessage = true;
                                        ourText.Append(message.Message);
                                        ourText.Append(" ");
                                        message.OurMessage = true;
                                    }
                                    else //their message
                                    {
                                        if (ourMessage)
                                            theirText.Clear();

                                        ourMessage = false;
                                        opponentMessage = true;

                                        didAddMessage = true;
                                        theirText.Append(message.Message);
                                        theirText.Append(" ");
                                    }
                                }

                                await database.SaveChatMessage(connection, game.Id, message.Id, message.UserId, message.Message, message.Created, message.UserId == betapet.UserId);
                            }

                            if (chatResponse.Messages.Count > 0 && didAddMessage)
                            {
                                ChatScenario chatScenario = new ChatScenario()
                                {
                                    HasResponded = chatResponse.Messages.Last().UserId == betapet.UserId,
                                    OurText = Regex.Unescape(ourText.ToString()),
                                    TheirText = Regex.Unescape(theirText.ToString()),
                                    Game = game,
                                    Messages = chatResponse.Messages,
                                };

                                chatScenarios.Add(chatScenario);
                            }
                        }
                    }
                }
            }

            return chatScenarios;
        }

        public async Task HandleChats()
        {
            await LoadChatHelperAsync();

            try
            {
                List<ChatScenario> chatScenarios = (await GetChatScenariosAsync()).Where(x => !x.HasResponded).ToList();
                if (chatScenarios.Count > 0)
                {
                    foreach (ChatScenario chatScenario in chatScenarios)
                    {
                        if (chatScenario.Messages.Where(x => x.OurMessage).Count() < 2)
                        {
                            string ourResponse = ChatHelper.GetChatResponse(chatScenario.TheirText);

                            if (ourResponse != "-" && chatScenario.Messages.Where(x => Regex.Escape(x.Message.ToLower()) == ourResponse).Count() == 0)
                            {
                                RequestResponse response = await betapet.SendChatMessageAsync(chatScenario.Game, ourResponse);
                            }
                        }
                    }
                }

                /*
                foreach(Game game in gameResponse.Games)
                {
                    if (game.Finished && (DateTime.Now - game.ActivityTime).TotalMinutes < 10)
                    {
                        string theirName = betapet.GetUserInfo(game.TheirUser.Id).Handle;

                        RequestResponse chatMessagesResponse = await betapet.GetChatMessagesAsync(game);
                        if (chatMessagesResponse.Success)
                        {
                            GetChatResponse chatResponse = (GetChatResponse)chatMessagesResponse.InnerResponse;

                            int userId = betapet.UserId;
                            if (chatResponse.Messages.Where(x => x.UserId == userId && x.Message.ToLower().Contains("tfgm")).Count() == 0) //if we have not already said tfgm
                            {
                                RequestResponse sendChatResponse = await betapet.SendChatMessageAsync(game, GetEndOfGameMessage(game.TheirUser.Score > game.OurUser.Score));
                            }
                        }
                    }
                }*/
            }
            catch { return; }
        }

        private string GetEndOfGameMessage(bool theyWon)
        {
            if (theyWon)
            {
                return "Tfgm och grattis till vinst!";
            }
            else
            {
                return "Tfgm";
            }
        }

        public async Task<List<GameSummary>> HandleAllMatches()
        {
            List<GameSummary> result = new List<GameSummary>();
            Stopwatch stopwatch = Stopwatch.StartNew();

            await betapet.LoginAsync();

            RequestResponse requestResponse = await betapet.GetGameAndUserListAsync();
            GamesAndUserListResponse gameResponse = (GamesAndUserListResponse)requestResponse.InnerResponse;

            if (gameResponse.Games.Where(x => !x.Finished).Count() < 25)
            {
                RequestResponse createGameRequestResponse = await betapet.CreateGameAsync();
            }

            foreach (Game game in gameResponse.Games)
            {
                if (game.OurTurn && !game.Finished)
                {
                    List<WordLine> wordLines = GetWordLines(game);
                    List<Move> moves = await GenerateMovesFromWordLinesAsync(game, wordLines);

                    await SimulateFutureAsync(game, moves, 10, 10, 5);

                    bool performedMove = false;
                    foreach (Move move in moves)
                    {
                        RequestResponse playRequestResponse = await betapet.PlayMoveAsync(move, game);
                        if (playRequestResponse != null)
                        {
                            if (playRequestResponse.Success)
                            {
                                result.Add(new GameSummary(game, betapet) { LastAction = string.Format("Played \"{0}\" for {1} points", move.Tiles.ToTileString(), move.Evaluation.Points) });
                                performedMove = true;
                                break;
                            }
                            else
                            {
                                PlayMoveResponse playMove = (PlayMoveResponse)playRequestResponse.InnerResponse;
                                if (playMove.CodeType == CodeType.Word && game.WordList == 2) // 2 is böjningar, which are all words, so if the word didn't even exist there we will remove it
                                {
                                    await lexicon.DisableLexiconWord(move.ToString());
                                }
                            }
                        }
                    }

                    if (!performedMove)
                    {
                        if (game.TilesLeft == 0)
                        {
                            RequestResponse passTurnResponse = await betapet.PassTurnAsync(game);
                            result.Add(new GameSummary(game, betapet) { LastAction = "Passed turn since no tiles are left" });
                        }
                        else
                        {
                            SwapTilesResponse swapResponse = (SwapTilesResponse)(await betapet.SwapTilesAsync(game, game.Hand)).InnerResponse;
                            if (swapResponse.SwapCount > 0)
                            {
                                result.Add(new GameSummary(game, betapet) { LastAction = "Swapped " + swapResponse.SwapCount.ToString() + " tiles" });
                            }
                            else
                            {
                                result.Add(new GameSummary(game, betapet) { LastAction = "Tried to swap tiles but failed" });
                            }
                        }
                    }
                }
                else
                {
                    result.Add(new GameSummary(game, betapet));
                }
            }

            stopwatch.Stop();

            if (result.Count > 0)
                AverageTimePerMove = (int)(stopwatch.ElapsedMilliseconds / (double)result.Count);

            return result;
        }

        /// <summary>
        /// Will simulate the future for a game given a list of moves. Will simulate the future for each move until maxMovesToSimulate is reached
        /// </summary>
        /// <param name="game">The game to simulate the future for</param>
        /// <param name="moves">The list of moves to simulate for</param>
        /// <param name="maxSimulationsPerMove">The max amount of simulated tile draws per move simulation</param>
        /// <param name="maxMovesToSimulate">The max amount of moves to simulate the future for</param>
        /// <param name="topMovesToUse">How many of the best possible moves in a simulated future should be used to calculate an average move potential for that future</param>
        /// <returns></returns>
        private async Task SimulateFutureAsync(Game game, List<Move> moves, int maxSimulationsPerMove, int maxMovesToSimulate, int topMovesToUse)
        {
            int simulatedMoves = 0;

            double bestLeadGainageFound = 0;
            foreach (Move move in moves)
            {
                simulatedMoves++;
                await SimulateFutureAsync(game, move, maxSimulationsPerMove, topMovesToUse, true); //true because we only want to simulate the opponent for now, only one step into the future

                if (simulatedMoves >= maxMovesToSimulate)
                    break;

                double thisLeadGainage = (double)move.Evaluation.LeadGainage;
                if (thisLeadGainage > bestLeadGainageFound)
                    bestLeadGainageFound = thisLeadGainage;

                if (bestLeadGainageFound > 0 && thisLeadGainage < 0) //if we have found a move that will increase our lead and are now looking at moves that won't we should stop
                    break;
            }

            List<Move> simulatedMovesList = moves.Take(simulatedMoves).OrderByDescending(x => x.Evaluation.LeadGainage).ToList();
            List<Move> otherMoves = moves.Skip(simulatedMoves).ToList();

            moves.Clear();
            moves.AddRange(simulatedMovesList);
            moves.AddRange(otherMoves);
        }

        /// <summary>
        /// Simulates the future of a single move in a single game. Will update the move evaulation for the move sent in as a parameter
        /// </summary>
        /// <param name="game">The game to simulate for</param>
        /// <param name="move">The move to simulate the next possible moves for</param>
        /// <param name="maxSimulationsPerMove">The amount of simulation samples for this move. This is the number of random pickups it will simulate the best moves for</param>
        /// <param name="topMovesToUse">How many of the best scored possible next moves it should include in the average potential score of that future. For example, 1 will only include the best move for a simulated pickup, 2 will include the 2 best moves and so on</param>
        /// <param name="simulateOpponent">If we will simulate the opponent making a move or not</param>
        /// <returns></returns>
        private async Task SimulateFutureAsync(Game game, Move move, int maxSimulationsPerMove, int topMovesToUse, bool simulateOpponent)
        {
            Game copiedGame = Game.FromJson(game.ToJson());
            copiedGame.ApplyMove(move);

            int tilesToPickUp = simulateOpponent ? 7 : move.Tiles.Where(x => !x.IsFromWordLine).Count(); //if it is the opponent we will simulate we take the whole hand
            List<Tile> tiles = copiedGame.HiddenTiles;

            List<Tile> simulatedHand = new List<Tile>(); //create the simulated hand  
            List<Tile> tilesLeftAfterMove = null;

            if(!simulateOpponent)
            {
                tilesLeftAfterMove = new List<Tile>();
                tilesLeftAfterMove.AddRange(copiedGame.Hand);
            }

            double totalPossibleScore = 0;
            for (int i = 0; i < maxSimulationsPerMove; i++)
            {
                simulatedHand.Clear();

                if (!simulateOpponent) //if we are simulating our selves we will use the hand we have and then pick up tiles, otherwise leave it empty so we simulate a full hand pickup
                    simulatedHand.AddRange(tilesLeftAfterMove);

                simulatedHand.AddRange(tiles.TakeRandomTiles(tilesToPickUp));

                copiedGame.SetSimulatedHand(simulatedHand);

                List<WordLine> wordLines = GetWordLines(copiedGame);
                List<Move> moves = await GenerateMovesFromWordLinesAsync(copiedGame, wordLines);

                int movesToUse = Math.Min(moves.Count, topMovesToUse);
                double possibleScore = 0;
                for (int j = 0; j < movesToUse; j++)
                {
                    possibleScore += moves[j].Evaluation.Points;
                }

                totalPossibleScore += possibleScore / (double)movesToUse;
            }

            if (simulateOpponent)
                move.Evaluation.HasSimulatedOpponent = true;

            move.Evaluation.AverageSimulatedOpponentPoints = totalPossibleScore / (double)maxSimulationsPerMove;
        }

        public async Task<List<Move>> CheckForWildcards(Game game, List<Move> moves)
        {
            List<Move> result = new List<Move>();

            string tiles = game.Hand.ToTileString();
            foreach (Move move in moves)
            {
                Move checkResult = await CheckForWildCards(tiles, move);
                if (checkResult != null)
                    result.Add(checkResult);
            }

            return result;
        }

        private async Task<Move> CheckForWildCards(string tiles, Move move)
        {
            foreach (Tile tile in move.Tiles)
            {
                if (tile.IsFromWordLine)
                    continue;

                if (!tiles.Any(c => c == tile.StringValue[0]))
                {
                    tile.WildCard = true;
                    int indexOfWildCard = tiles.IndexOf('.');

                    if (indexOfWildCard == -1)
                        return null;

                    tiles = tiles.Remove(indexOfWildCard, 1);
                }
                else
                    tiles = tiles.Remove(tiles.IndexOf(tile.StringValue[0]), 1);
            }

            return move;
        }

        public async Task<List<Move>> GenerateMovesFromWordLinesAsync(Game game, List<WordLine> wordLines)
        {
            List<Move> moves = new List<Move>();
            int worstScore = 0;

            string hand = game.Hand.ToTileString();
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

                            Move move = CreateMoveFromPosition(new Position(shiftedX, shiftedY), candidateWord, wordLine, game.Board, hand, game.IsFirstMove);
                            if (move != null && !(game.IsFirstMove && (move.Tiles[0].X != 7 || move.Tiles[0].Y != 7)))
                            {
                                MoveEvaluation evaluation = await EvaluateMoveAsync(move, game, wordLine.Letters, connection);

                                if (evaluation.Possible)
                                {
                                    move.Evaluation = evaluation;

                                    if (worstScore == 0)
                                    {
                                        moves.Add(move);
                                        worstScore = evaluation.Points;
                                    }
                                    else
                                    {
                                        if (moves.Count > 0 && moves[moves.Count - 1].Evaluation.Points > evaluation.Points)
                                        {
                                            moves.Add(move);
                                        }
                                        else
                                        {
                                            for (int i = 0; i < moves.Count; i++)
                                            {
                                                if (moves[i].Evaluation.Points < evaluation.Points)
                                                {
                                                    worstScore = moves[i].Evaluation.Points;
                                                    moves.Insert(i, move);
                                                    break;
                                                }
                                            }
                                        }
                                    }

                                    if (moves.Count > 1000)
                                    {
                                        moves.RemoveRange(990, 9);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            moves = await CheckForWildcards(game, moves);
            moves = moves.OrderByDescending(move => move.Evaluation.Points).ToList();

            return moves;
        }

        private Move CreateMoveFromPosition(Position position, string word, WordLine wordLine, Board board, string hand, bool firstMove)
        {
            if ((wordLine.Direction == Direction.Horizontal ? position.X : position.Y) + word.Length > 15 || position.X < 0 || position.Y < 0)
                return null;

            List<char> missingHandLettes = null;
            for (int i = 0; i < word.Length; i++)
            {
                if (hand.Contains(word[i]))
                    hand.Remove(hand.IndexOf(word[i]), 1);
                else
                {
                    if (missingHandLettes == null)
                        missingHandLettes = new List<char>();

                    missingHandLettes.Add(word[i]);
                }
            }

            bool anyTileConnected = false;
            Move move = new Move();
            bool hasMissingHandLetters = missingHandLettes != null && missingHandLettes.Count > 0;

            for (int offset = 0; offset < word.Length; offset++)
            {
                int x = position.X + (wordLine.Direction == Direction.Horizontal ? offset : 0);
                int y = position.Y + (wordLine.Direction == Direction.Vertical ? offset : 0);

                if (!anyTileConnected && (board.TilesConnected[x, y] || firstMove))
                    anyTileConnected = true;

                if (hasMissingHandLetters && missingHandLettes.Contains(word[offset]))
                {
                    if (!wordLine.Letters.Any(tile => tile.StringValue == word[offset].ToString() && tile.X == x && tile.Y == y))
                        return null;
                }

                string letter = word[offset].ToString();
                move.AddTile(letter, x, y, board.HasLetterAtPosition(x, y, letter));
            }

            if (!anyTileConnected)
                return null;

            return move;
        }

        public List<WordLine> GetWordLines(Game game)
        {
            if (game.IsFirstMove)
                return new List<WordLine>() { new WordLine(Direction.Horizontal, new Position(7, 7)), new WordLine(Direction.Vertical, new Position(7, 7)) };

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
                        currentTile = game.Board.GetTileAtPosition(positionToCheck, wordLine.StartPosition.Y);
                    else
                        currentTile = game.Board.GetTileAtPosition(wordLine.StartPosition.X, positionToCheck);

                    if (currentTile != null && currentTile.Type == TileType.Letter)
                        wordLine.Letters.Add(currentTile);
                }
            }

            return wordLines;
        }

        public async Task<MoveEvaluation> EvaluateMoveAsync(Move move, Game game, List<Tile> additionalTiles, NpgsqlConnection sqlConnection = null)
        {
            if (move.Tiles.Count == 0)
                return MoveEvaluation.ImpossibleMove;

            if (!game.Hand.AddTiles(additionalTiles).ContainsTiles(move.Tiles))
                return MoveEvaluation.ImpossibleMove;

            bool oneTileWasNew = false;
            foreach (Tile tile in move.Tiles)
            {
                if (tile.X > 14 || tile.X < 0 || tile.Y > 14 || tile.Y < 0)
                    return MoveEvaluation.ImpossibleMove;

                Tile boardTile = game.Board.GetTileAtPosition(tile.X, tile.Y);
                if (boardTile.Type == TileType.Letter)
                {
                    if (boardTile.StringValue != tile.StringValue)
                        return MoveEvaluation.ImpossibleMove;
                }
                else if (!oneTileWasNew)
                    oneTileWasNew = true;
            }

            if (!oneTileWasNew)
                return MoveEvaluation.ImpossibleMove;

            List<Tile> tilesToRemove = new List<Tile>();
            foreach (Tile tile in move.Tiles)
            {
                if (game.Board.TileIsLetter(game.Board.GetTileAtPosition(tile.X, tile.Y)))
                    tilesToRemove.Add(tile);
            }

            Direction moveDirection = Direction.None;

            if (move.Tiles.Count > 1)
            {
                moveDirection = move.Tiles[0].X != move.Tiles[1].X ? Direction.Horizontal : Direction.Vertical;
            }

            if (!game.IsFirstMove && !move.IsConnected(game.Board))
                return MoveEvaluation.ImpossibleMove;

            List<List<Tile>> words = new List<List<Tile>>();

            foreach (Tile tile in move.Tiles)
            {
                if (moveDirection != Direction.None)
                    words.Add(game.Board.ScanForTiles(move, tile, moveDirection == Direction.Horizontal ? Direction.Vertical : Direction.Horizontal));
            }

            if (moveDirection != Direction.None)
                words.Add(game.Board.ScanForTiles(move, move.Tiles[0], moveDirection));

            if (!words.Any(x => x.Count > 1))
                return MoveEvaluation.ImpossibleMove;

            words = words.RemoveDuplicates();
            words = words.Where(x => x.Count > 1 && !x.All(tile => game.Board.HasLetterAtPosition(tile.X, tile.Y, tile.StringValue))).ToList();

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

            int bingoScore = 0;

            if (move.Tiles.Where(x => !x.IsFromWordLine).Count() == 7)
                bingoScore = 50;

            return new MoveEvaluation(true, pointsPerWord.Sum() + bingoScore) { TilesFromBoard = tilesToRemove };
        }
    }
}