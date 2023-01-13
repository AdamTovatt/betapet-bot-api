﻿using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        public async Task<List<RatingPoint>> GetRatingPointsAsync()
        {
            List<RatingPoint> result = new List<RatingPoint>();

            string query = @"SELECT rating, time_of_rating FROM rating_over_time ORDER BY time_of_rating";
            using (NpgsqlConnection connection = await GetConnectionAsync())
            using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
            {
                using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        result.Add(new RatingPoint()
                        {
                            Rating = (int)reader["rating"],
                            Time = (DateTime)reader["time_of_rating"]
                        });
                    }
                }
            }

            foreach (RatingPoint ratingPoint in result)
            {
                ratingPoint.XValue = ratingPoint.Time.ToString("yyyy-MM-dd");
            }

            return result;
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

        public async Task SaveChatMessage(NpgsqlConnection connection, int matchId, int localId, int authorId, string text, DateTime timeOfCreation, bool sentByUs)
        {
            string query = @"INSERT INTO chat_message
                            (match_id, local_id, author_id, text, time_of_creation, sent_by_us) 
                            SELECT
                            @match_id, @local_id, @author_id, @text, @time_of_creation, @sent_by_us
                            WHERE NOT EXISTS
                            (SELECT match_id, local_id
                            FROM chat_message
                            WHERE
                            match_id = @match_id AND
                            local_id = @local_id);";

            using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.Add("@match_id", NpgsqlDbType.Integer).Value = matchId;
                command.Parameters.Add("@local_id", NpgsqlDbType.Integer).Value = localId;
                command.Parameters.Add("@author_id", NpgsqlDbType.Integer).Value = authorId;
                command.Parameters.Add("@text", NpgsqlDbType.Varchar).Value = Regex.Unescape(text);
                command.Parameters.Add("@time_of_creation", NpgsqlDbType.Timestamp).Value = timeOfCreation;
                command.Parameters.Add("@sent_by_us", NpgsqlDbType.Boolean).Value = sentByUs;

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<byte[]> ReadModelAsync(string name)
        {
            string query = @"SELECT byte_data FROM network_model WHERE name = @name";

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (NpgsqlConnection connection = await GetConnectionAsync())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.Add("@name", NpgsqlDbType.Varchar).Value = name;

                    return (await command.ExecuteScalarAsync()) as byte[];
                }
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
