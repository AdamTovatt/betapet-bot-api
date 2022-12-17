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

        public async Task<User> GetUserByEmailAsync(string email)
        {
            User result = null;

            const string query = "SELECT id, name, email, password, created_date, role FROM site_user WHERE email = @email";

            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
            {
                await connection.OpenAsync();

                command.Parameters.Add("@email", NpgsqlDbType.Varchar).Value = email.ToLower();

                using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        result = User.FromReader(reader);
                        return result;
                    }
                }
            }

            return result;
        }
    }
}
