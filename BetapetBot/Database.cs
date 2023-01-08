using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetapetBot
{
    public class Database
    {
        public string ConnectionString { get; set; }

        public Database(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public async Task<int> GetLastRating()
        {
            string query = @"SELECT rating FROM rating_over_time ORDER BY time_of_rating DESC LIMIT 1";
            using (NpgsqlConnection connection = await GetConnectionAsync())
            using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
            {
                return (int)(await command.ExecuteScalarAsync() as int? ?? 0);
            }
        }

        public async Task AddRating(int rating)
        {
            string query = @"INSERT INTO rating_over_time (rating) VALUES (@rating)";
            using (NpgsqlConnection connection = await GetConnectionAsync())
            using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.Add("@rating", NpgsqlDbType.Integer).Value = rating;

                await command.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// Will return a connection
        /// </summary>
        /// <returns></returns>
        public async Task<NpgsqlConnection> GetConnectionAsync()
        {
            NpgsqlConnection connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();
            return connection;
        }
    }
}
