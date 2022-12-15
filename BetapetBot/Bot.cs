namespace BetapetBot
{
    public class Bot
    {
        public string Username { get; private set; }
        public string Password { get; private set; }

        public Bot(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public string GetMessage()
        {
            return string.Format("{0} is the username", Username);
        }
    }
}