using Betapet.Helpers;
using Betapet.Models.Communication;

namespace Betapet
{
    public class BetapetManager
    {
        private ApiHelper api;
        private string? username;
        private string? password;

        public BetapetManager()
        {
            api = new ApiHelper();
        }

        public async Task<string> LoginAsync(string username, string password)
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
                this.username = username;
                this.password = password;
            }

            return await response.Content.ReadAsStringAsync();
        }
    }
}