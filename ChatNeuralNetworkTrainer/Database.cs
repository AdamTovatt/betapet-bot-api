using Microsoft.AspNetCore.Mvc;
using Microsoft.ML;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ChatNeuralNetworkTrainer
{
    public class Database
    {
        private string connectionString;

        public Database(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public List<ChatMessage> GetChatMessages()
        {
            List<ChatMessage> result = new List<ChatMessage>();

            string query = @"SELECT * FROM chat_message";

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
            {
                connection.Open();

                using (NpgsqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ChatMessage chatMessage = new ChatMessage()
                        {
                            AuthorId = (int)reader["author_id"],
                            Id = (int)reader["id"],
                            LocalId = (int)reader["local_id"],
                            Text = reader["text"] as string,
                            TimeOfCreation = (DateTime)reader["time_of_creation"],
                            SentByUs = (bool)reader["sent_by_us"],
                            MatchId = (int)reader["match_id"],
                        };

                        result.Add(chatMessage);
                    }
                }
            }

            return result;
        }

        public void SaveModel(byte[] bytes, string name)
        {
            string query = @"INSERT INTO network_model (name, byte_data) VALUES (@name, @byte_data)
                             ON CONFLICT (name) DO UPDATE SET byte_data = @byte_data";

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    connection.Open();

                    command.Parameters.Add("@name", NpgsqlDbType.Varchar).Value = name;
                    command.Parameters.Add("@byte_data", NpgsqlDbType.Bytea).Value = bytes;

                    command.ExecuteNonQuery();
                }
            }
        }

        public byte[] ReadModel(string name)
        {
            string query = @"SELECT byte_data FROM network_model WHERE name = @name";

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    connection.Open();

                    command.Parameters.Add("@name", NpgsqlDbType.Varchar).Value = name;

                    return command.ExecuteScalar() as byte[];
                }
            }
        }
    }
}
