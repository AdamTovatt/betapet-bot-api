using Betapet.Helpers;
using Betapet.Models.Communication;
using Betapet.Models.Communication.Responses;

namespace Betapet
{
    public class BetapetManager
    {
        private ApiHelper api;
        private string? username;
        private string? password;
        private string deviceId;

        private LoginResponse? loginResponse;

        /// <summary>
        /// Default constructor
        /// </summary>
        public BetapetManager(string username, string password, string deviceId)
        {
            api = new ApiHelper();
            this.deviceId = deviceId;
            this.username = username;
            this.password = password;
        }

        private async Task VerifyLoginAsync()
        {
            if(loginResponse == null)
            {
                if(!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
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

            if(response.IsSuccessStatusCode)
            {
                loginResponse = LoginResponse.FromJson(await response.Content.ReadAsStringAsync());
                return new RequestResponse(loginResponse);
            }

            return new RequestResponse(false);
        }

        /// <summary>
        /// Method for getting a list of friends of the current user
        /// </summary>
        /// <returns></returns>
        public async Task<RequestResponse> GetFriends()
        {
            await VerifyLoginAsync();

            if (loginResponse == null)
                throw new Exception("Not logged in when attempting to get friends");

            Request request = new Request("/friends.php", loginResponse);
            request.AddParameter("action", "get");

            HttpResponseMessage response = await api.GetResponseAsync(request);

            if(response.IsSuccessStatusCode)
            {
                return new RequestResponse(FriendsResponse.FromJson(await response.Content.ReadAsStringAsync()));
            }

            return new RequestResponse(false);
        }

        /// <summary>
        /// Method for getting list of games that are currently active
        /// </summary>
        /// <returns></returns>
        public async Task<RequestResponse> GetGameAndUserList()
        {
            await VerifyLoginAsync();

            if (loginResponse == null)
                throw new Exception("Not logged in when attempting to get games");

            Request request = new Request("/matchmake.php", loginResponse);
            request.AddParameter("device_id", deviceId);
            request.AddParameter("type", "gameanduserlist");

            HttpResponseMessage response = await api.GetResponseAsync(request);

            if(response.IsSuccessStatusCode)
            {
                return new RequestResponse(GamesAndUserListResponse.FromJson(await response.Content.ReadAsStringAsync()));
            }

            return new RequestResponse(false);
        }
    }
}