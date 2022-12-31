using Betapet.Helpers;
using Betapet.Models;
using Betapet.Models.Communication;
using Betapet.Models.Communication.Responses;
using Betapet.Models.InGame;
using System.Text;

namespace Betapet
{
    public class BetapetManager
    {
        public int UserId { get; private set; }

        private ApiHelper api;
        private string? username;
        private string? password;
        private string deviceId;

        private LoginResponse? loginResponse;

        private Dictionary<int, User> userDictionary;

        /// <summary>
        /// Default constructor
        /// </summary>
        public BetapetManager(string username, string password, string deviceId)
        {
            api = new ApiHelper();
            userDictionary = new Dictionary<int, User>();
            this.deviceId = deviceId;
            this.username = username;
            this.password = password;
        }

        private async Task VerifyLoginAsync()
        {
            if (loginResponse == null)
            {
                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    RequestResponse loginResponse = await LoginAsync();
                    if (!loginResponse.Success)
                        throw new Exception("Username and or password seems to be invalid. They are: " + username + " and " + password);
                }
                else
                {
                    throw new Exception("This Betapet-instance needs to be logged in but is not!");
                }
            }
        }

        /// <summary>
        /// Method for loggin in this instance of Betapet. Will save the credentials if successfull
        /// </summary>
        /// <param name="username">The username to use</param>
        /// <param name="password">The password to use</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<RequestResponse> LoginAsync()
        {
            if (string.IsNullOrEmpty(username))
                throw new Exception("Username is missing");
            if (string.IsNullOrEmpty(password))
                throw new Exception("Password is missing");

            Request request = new Request("/login.php", false, true);
            request.AddParameter("username", username);
            request.AddParameter("password", password);

            HttpResponseMessage response = await api.GetResponseAsync(request);

            if (response.IsSuccessStatusCode)
            {
                loginResponse = LoginResponse.FromJson(await response.Content.ReadAsStringAsync());
                UserId = int.Parse(loginResponse.UserId);
                return new RequestResponse(loginResponse);
            }

            return new RequestResponse(false);
        }

        /// <summary>
        /// Will get the match request that this account has
        /// </summary>
        /// <returns></returns>
        public async Task<RequestResponse> GetMatchRequestsAsync()
        {
            await VerifyLoginAsync();

            if (loginResponse == null)
                throw new Exception("Not logged in when attempting to get match request");

            Request request = new Request("/matchmake.php", loginResponse);
            request.AddParameter("type", "vsfind");

            HttpResponseMessage response = await api.GetResponseAsync(request);

            if(response.IsSuccessStatusCode)
                return new RequestResponse(MatchRequestResponse.FromJson(await response.Content.ReadAsStringAsync()));

            return new RequestResponse(false);
        }

        /// <summary>
        /// Will accept a mach request
        /// </summary>
        /// <param name="gameId">The matchId of the match request to accept</param>
        /// <returns></returns>
        public async Task<RequestResponse> AcceptMatchRequestAsync(int gameId)
        {
            await VerifyLoginAsync();

            if (loginResponse == null)
                throw new Exception("Not logged in when attempting to accept match request");

            Request request = new Request("/matchmake.php", loginResponse);
            request.AddParameter("type", "vsaccept");
            request.AddParameter("gameid", gameId.ToString());

            HttpResponseMessage responseMessage = await api.GetResponseAsync(request);

            if (responseMessage.IsSuccessStatusCode)
                return new RequestResponse(AcceptMatchRequestResponse.FromJson(await responseMessage.Content.ReadAsStringAsync()));

            return new RequestResponse(false);
        }

        /// <summary>
        /// Method for getting a list of friends of the current user
        /// </summary>
        /// <returns></returns>
        public async Task<RequestResponse> GetFriendsAsync()
        {
            await VerifyLoginAsync();

            if (loginResponse == null)
                throw new Exception("Not logged in when attempting to get friends");

            Request request = new Request("/friends.php", loginResponse);
            request.AddParameter("action", "get");

            HttpResponseMessage response = await api.GetResponseAsync(request);

            if (response.IsSuccessStatusCode)
                return new RequestResponse(FriendsResponse.FromJson(await response.Content.ReadAsStringAsync()));

            return new RequestResponse(false);
        }

        /// <summary>
        /// Method for getting list of games that are currently active
        /// </summary>
        /// <returns></returns>
        public async Task<RequestResponse> GetGameAndUserListAsync()
        {
            await VerifyLoginAsync();

            if (loginResponse == null)
                throw new Exception("Not logged in when attempting to get games");

            Request request = new Request("/matchmake.php", loginResponse);
            request.AddParameter("device_id", deviceId);
            request.AddParameter("type", "gameanduserlist");

            HttpResponseMessage response = await api.GetResponseAsync(request);

            if (response.IsSuccessStatusCode)
            {
                GamesAndUserListResponse result = GamesAndUserListResponse.FromJson(await response.Content.ReadAsStringAsync());

                foreach(User user in result.Users)
                {
                    if (userDictionary.ContainsKey(user.Id))
                        userDictionary[user.Id] = user;
                    else
                        userDictionary.Add(user.Id, user);
                }

                return new RequestResponse(result);
            }

            return new RequestResponse(false);
        }

        /// <summary>
        /// Method for playing a move
        /// </summary>
        /// <param name="move"></param>
        /// <returns></returns>
        public async Task<RequestResponse> PlayMoveAsync(Move move, Game game)
        {
            await VerifyLoginAsync();

            if (loginResponse == null)
                throw new Exception("Not logged in when attempting to play move");

            Request request = new Request("/play.php", loginResponse);
            request.AddParameter("type", "word");
            request.AddParameter("gameid", game.Id.ToString());
            request.AddParameter("word", move.ToMoveString());
            request.AddParameter("turn", game.Turn.ToString());

            HttpResponseMessage response = await api.GetResponseAsync(request);

            PlayMoveResponse playMoveResponse = PlayMoveResponse.FromJson(await response.Content.ReadAsStringAsync());
            return new RequestResponse(playMoveResponse, playMoveResponse.Result);
        }

        /// <summary>
        /// Method for sending a chat message
        /// </summary>
        /// <param name="gameId">The id of the game the message should be sent in</param>
        /// <param name="message">The message that should be sent</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<RequestResponse> SendChatMessageAsync(Game game, string message)
        {
            await VerifyLoginAsync();

            if (loginResponse == null)
                throw new Exception("Not logged in when attempting to send chat");

            Request request = new Request("/chat.php", loginResponse);
            request.AddParameter("type", "set");
            request.AddParameter("gameid", game.Id.ToString());
            request.AddParameter("msg", message);

            HttpResponseMessage response = await api.GetResponseAsync(request);

            return new RequestResponse(SendChatResponse.FromJson(await response.Content.ReadAsStringAsync()));
        }

        /// <summary>
        /// Method for getting all chat messages in a game
        /// </summary>
        /// <param name="gameId">The game for which the chat messages should be fetched</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<RequestResponse> GetChatMessagesAsync(Game game)
        {
            await VerifyLoginAsync();

            if (loginResponse == null)
                throw new Exception("Not logged in when attempting to get chats");

            Request request = new Request("/chat.php", loginResponse);
            request.AddParameter("type", "get");
            request.AddParameter("gameid", game.Id.ToString());
            request.AddParameter("timestamp", "1");

            HttpResponseMessage response = await api.GetResponseAsync(request);

            return new RequestResponse(GetChatResponse.FromJson(await response.Content.ReadAsStringAsync()));
        }

        /// <summary>
        /// Method for swapping tiles
        /// </summary>
        /// <param name="game">The game in which the tiles should be swapped</param>
        /// <param name="tiles">The tiles to swap</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<RequestResponse> SwapTilesAsync(Game game, List<Tile> tiles)
        {
            await VerifyLoginAsync();

            if (loginResponse == null)
                throw new Exception("Not logged in when attempting to swap tiles");

            Request request = new Request("/play.php", loginResponse);
            request.AddParameter("type", "swap");
            request.AddParameter("gameid", game.Id.ToString());
            request.AddParameter("turn", game.Turn.ToString());
            request.AddParameter("tiles", tiles.ToTileString());

            HttpResponseMessage response = await api.GetResponseAsync(request);

            return new RequestResponse(SwapTilesResponse.FromJson(await response.Content.ReadAsStringAsync()));
        }

        /// <summary>
        /// Will pass the turn
        /// </summary>
        /// <param name="game">The game to pass the turn in</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<RequestResponse> PassTurnAsync(Game game)
        {
            await VerifyLoginAsync();

            if (loginResponse == null)
                throw new Exception("Not logged in when attempting to pass turn");

            Request request = new Request("/play.php", loginResponse);
            request.AddParameter("type", "pass");
            request.AddParameter("gameid", game.Id.ToString());
            request.AddParameter("turn", game.Turn.ToString());

            HttpResponseMessage response = await api.GetResponseAsync(request);

            if (response.IsSuccessStatusCode)
                return new RequestResponse(PassTurnResponse.FromJson(await response.Content.ReadAsStringAsync()));

            return new RequestResponse(false);
        }

        /// <summary>
        /// Will create a game for someone to join
        /// </summary>
        /// <param name="boardType">The desired board type. Default is 2 which is "standard"</param>
        /// <param name="wordList">The desired word list. Default is 2 which is "böjningar"</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<RequestResponse> CreateGameAsync(int boardType = 2, int wordList = 2)
        {
            await VerifyLoginAsync();

            if (loginResponse == null)
                throw new Exception("Not logged in when attempting to create random match");

            Request request = new Request("/matchmake.php", loginResponse);
            request.AddParameter("type", "rndgamer");
            request.AddParameter("board_type", boardType.ToString());
            request.AddParameter("rating_lvl", "0");
            request.AddParameter("dict", wordList.ToString());

            HttpResponseMessage response = await api.GetResponseAsync(request);

            if (response.IsSuccessStatusCode)
                return new RequestResponse(CreateGameResponse.FromJson(await response.Content.ReadAsStringAsync()));

            return new RequestResponse(false);
        }

        /// <summary>
        /// Will challenge another player to a game
        /// </summary>
        /// <param name="playerId">The id of the player to challenge</param>
        /// <param name="boardType">The desired board type. Default is 2 which is "standard"</param>
        /// <param name="wordList">The desired word list. Default is 2 which is "böjningar"</param>
        /// <returns></returns>
        public async Task<RequestResponse> ChallengePlayerAsync(int playerId, int boardType = 2, int wordList = 2)
        {
            await VerifyLoginAsync();

            if (loginResponse == null)
                throw new Exception("Not logged in when attempting to create random match");

            Request request = new Request("/matchmake.php", loginResponse);
            request.AddParameter("type", "vsrequest");
            request.AddParameter("board_type", boardType.ToString());
            request.AddParameter("req_userid", playerId.ToString());
            request.AddParameter("dict", wordList.ToString());

            HttpResponseMessage response = await api.GetResponseAsync(request);

            if (response.IsSuccessStatusCode)
                return new RequestResponse(ChallengePlayerResponse.FromJson(await response.Content.ReadAsStringAsync()));

            return new RequestResponse(false);
        }

        public User GetUserInfo(int userId)
        {
            if(userDictionary.ContainsKey(userId))
            {
                return userDictionary[userId];
            }

            return null;
        }
    }
}