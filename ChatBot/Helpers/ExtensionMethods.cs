using ChatBot.Models.Data;
using System.Text;

namespace ChatBot.Helpers
{
    public static class ExtensionMethods
    {
        private static Random? random;

        public static T TakeRandomElement<T>(this List<T> list)
        {
            if (random == null)
                random = new Random();

            return list[random.Next(list.Count)];
        }

        public static List<T> AddIfNotNull<T>(this List<T> list, T element)
        {
            if (element == null)
                return list;

            list.Add(element);
            return list;
        }

        public static string ReadTokensUntill(this List<string> tokens, int startIndex, out int indexOffset, out string? stoppingToken, params string[] stopTokens)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stoppingToken = null;

            indexOffset = 0;
            for (int i = startIndex; i < tokens.Count; i++)
            {
                if (stopTokens.Contains(tokens[i]))
                {
                    stoppingToken = tokens[i];
                    break;
                }

                stringBuilder.Append(tokens[i]);
                stringBuilder.Append(" ");

                indexOffset++;
            }

            if (stringBuilder.Length > 0)
                stringBuilder.Length--;

            return stringBuilder.ToString();
        }

        public static string Get(this List<string> tokens, int index)
        {
            if (index > tokens.Count - 1)
                return "[end of file]";

            return tokens[index];
        }

        public static string GetStringsWithOrBetween(this List<string> strings)
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (string word in strings)
            {
                stringBuilder.Append(word);
                stringBuilder.Append(" or ");
            }

            stringBuilder.Length = stringBuilder.Length - 4;

            return stringBuilder.ToString();
        }

        public static List<string> GetNames(this List<State> states)
        {
            List<string> result = new List<string>();

            foreach(State state in states)
            {
                result.Add(state.Name);
            }

            return result;
        }
    }
}
