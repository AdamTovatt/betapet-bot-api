using System.Text;

namespace BetapetBotApi.Helpers
{
    public class StringGenerator
    {
        private const string characters = "ABCDEFGHIJKLMNOPRSTUVWXYZÅÄÖ";

        private static Random random = new Random();

        public static string GetRandomString(int length)
        {
            char[] chars = new char[length];

            for (int i = 0; i < length; i++)
            {
                chars[i] = characters[random.Next(0, characters.Length)];
            }

            return new string(chars);
        }
    }
}
