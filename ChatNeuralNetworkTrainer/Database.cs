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

            using(NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
            {
                connection.Open();

                using(NpgsqlDataReader reader = command.ExecuteReader())
                {
                    while(reader.Read())
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
    }
}
