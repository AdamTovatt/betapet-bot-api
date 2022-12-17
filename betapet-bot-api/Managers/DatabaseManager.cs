using Betapet.Models;
using BetapetBotApi.Helpers;
using Npgsql;
using NpgsqlTypes;
using WebApiUtilities.Helpers;

namespace BetapetBotApi.Managers
{
    public class DatabaseManager
    {
        public string ConnectionString { get; private set; }

        public DatabaseManager()
        {
            LoadConnectionString();
        }

        private void LoadConnectionString()
        {
            ConnectionString = ConnectionStringHelper.GetConnectionStringFromUrl(EnvironmentHelper.GetEnvironmentVariable("DATABASE_URL"));
        }

        public async Task<List<string>> GetWordsInLexiconAsync()
        {
            List<string> result = new List<string>();

            const string query = "SELECT word FROM lexicon";

            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
            {
                await connection.OpenAsync();

                using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        result.Add(reader["word"] as string);
                    }
                }
            }

            return result;
        }
    }
}
