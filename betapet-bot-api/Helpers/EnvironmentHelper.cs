using Sakur.WebApiUtilities.Models;

namespace BetapetBotApi.Helpers
{
    public static class EnvironmentHelper
    {
        private static Dictionary<string, string> variableCache = new Dictionary<string, string>();

        public static string GetEnvironmentVariable(string environmentVariableName, int minLength = 0)
        {
            if (variableCache.ContainsKey(environmentVariableName))
                return variableCache[environmentVariableName];

            string result = Environment.GetEnvironmentVariable(environmentVariableName);

            if (result == null)
                throw new ApiException(string.Format("No {0} in environment variables", environmentVariableName), System.Net.HttpStatusCode.InternalServerError);
            if (result.Length < minLength)
                throw new ApiException(string.Format("Invalid {0} in environment variables", environmentVariableName), System.Net.HttpStatusCode.InternalServerError);

            variableCache.Add(environmentVariableName, result);

            return result;
        }
    }
}
