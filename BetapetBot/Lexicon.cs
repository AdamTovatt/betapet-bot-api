using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BetapetBot
{
    public class Lexicon
    {
        private const string characters = "ABCDEFGHIJKLMNOPRSTUVWXYZÅÄÖ";
        private Dictionary<char, short> characterIndexes;

        public string ConnectionString { get; private set; }

        public Lexicon(string connectionString)
        {
            ConnectionString = connectionString;

            if (characterIndexes == null)
            {
                characterIndexes = new Dictionary<char, short>();

                for (short i = 0; i < characters.Length; i++)
                {
                    characterIndexes.Add(characters[i], i);
                }
            }
        }

        /// <summary>
        /// Disables a word in the lexicon so it will not be included in the possible words
        /// </summary>
        /// <param name="word">The word to disable</param>
        /// <returns></returns>
        public async Task DisableLexiconWord(string word)
        {
            string query = @"UPDATE lexicon SET disable = true WHERE word = @word";
            using (NpgsqlConnection connection = await GetConnectionAsync())
            using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.Add("@word", NpgsqlDbType.Varchar).Value  = word;
                
                await command.ExecuteNonQueryAsync();
            }
        }

        private async Task<List<string>> GetPossibleWordsWildCardHandledAsync(string letters, NpgsqlConnection connection)
        {
            List<string> result = new List<string>();

            const string query = @"SELECT word FROM lexicon WHERE
                                    a < @a AND b < @b AND c < @c AND d < @d AND e < @e AND f < @f AND
                                    g < @g AND h < @h AND i < @i AND j < @j AND k < @k AND l < @l AND
                                    m < @m AND n < @n AND o < @o AND p < @p AND r < @r AND s < @s AND
                                    t < @t AND u < @u AND v < @v AND w < @w AND x < @x AND y < @y AND
                                    z < @z AND a1 < @a1 AND a2 < @a2 AND o2 < @o2;";

            using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
            {
                short[] letterCount = new short[characters.Length];

                HashSet<char> checkedChars = new HashSet<char>();
                char currentLetter;
                for (short i = 0; i < letters.Length; i++)
                {
                    currentLetter = letters[i];

                    if (checkedChars.Contains(currentLetter))
                        continue;

                    try
                    {
                        short characterIndex;
                        if (characterIndexes.TryGetValue(currentLetter, out characterIndex))
                        {
                            letterCount[characterIndexes[currentLetter]] = GetLetterCount(letters, currentLetter);
                        }
                        else
                        {
                            StringBuilder stringBuilder = new StringBuilder();

                            foreach(char key in characterIndexes.Keys)
                            {
                                stringBuilder.Append(key);
                                stringBuilder.Append(", ");
                            }

                            throw new Exception("Tried to get \"" + currentLetter + "\" from characterIndexes but it does not exist. CharaterIndexesCount: " + characterIndexes.Count() + " lettes: " + letters + " characterIndexes: " + stringBuilder.ToString());
                        }
                    }
                    catch
                    {
                        throw;
                    }
                }

                for (int i = 0; i < characters.Length - 3; i++)
                {
                    command.Parameters.Add(string.Format("@{0}", characters[i]), NpgsqlDbType.Smallint).Value = letterCount[i] + 1; // + 1 since we do <
                }

                command.Parameters.Add(string.Format("@a1", characters[25]), NpgsqlDbType.Smallint).Value = letterCount[25] + 1; //handle å ä ö separately
                command.Parameters.Add(string.Format("@a2", characters[26]), NpgsqlDbType.Smallint).Value = letterCount[26] + 1; //since the names of their parameters
                command.Parameters.Add(string.Format("@o2", characters[27]), NpgsqlDbType.Smallint).Value = letterCount[27] + 1; //are inconsistent. This will introduce a bug if the length of the characters string is changed!!

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

        /// <summary>
        /// Will return a list of all possible words when given a string of letters to look for
        /// </summary>
        /// <param name="letters">The letters available to create a word</param>
        /// <param name="connection">The sql connection to use</param>
        /// <returns></returns>
        public async Task<List<string>> GetPossibleWordsAsync(string letters, NpgsqlConnection connection)
        {
            if (!letters.Contains('.'))
                return await GetPossibleWordsWildCardHandledAsync(letters.ToUpper(), connection);
            else
            {
                HashSet<string> result = new HashSet<string>();

                int wildCardCount = letters.Count(character => character == '.');
                string withoutWildCards = letters.Replace(".", "").ToUpper();

                if(wildCardCount == 1)
                {
                    for (int i = 0; i < characters.Length; i++)
                    {
                        foreach(string possibleWord in await GetPossibleWordsWildCardHandledAsync(string.Format("{0}{1}", withoutWildCards, characters[i]), connection))
                        {
                            result.Add(possibleWord);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < characters.Length; i++)
                    {
                        for (int j = 0; j < characters.Length; j++)
                        {
                            foreach (string possibleWord in await GetPossibleWordsWildCardHandledAsync(string.Format("{0}{1}{2}", withoutWildCards, characters[i], characters[j]), connection))
                            {
                                result.Add(possibleWord);
                            }
                        }
                    }
                }

                return result.ToList();
            }
        }

        /// <summary>
        /// Will return a list of all possible words when given a string of letters to look for
        /// </summary>
        /// <param name="letters">The letters available to create a word</param>
        /// <returns></returns>
        public async Task<List<string>> GetPossibleWordsAsync(string letters)
        {
            using (NpgsqlConnection connection = await GetConnectionAsync())
            {
                return await GetPossibleWordsAsync(letters, connection);
            }
        }

        /// <summary>
        /// Will return a connection that can be used to get possible words
        /// </summary>
        /// <returns></returns>
        public async Task<NpgsqlConnection> GetConnectionAsync()
        {
            NpgsqlConnection connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();
            return connection;
        }

        public async Task<bool> GetWordExistsAsync(string word, NpgsqlConnection connection)
        {
            const string query = "SELECT COUNT(word) FROM lexicon WHERE word = @word;";

            using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.Add("@word", NpgsqlDbType.Varchar).Value = word;

                return (long)await command.ExecuteScalarAsync() > 0;
            }
        }

        public async Task<bool> GetWordExistsAsync(string word)
        {
            using (NpgsqlConnection connection = await GetConnectionAsync())
            {
                return await GetWordExistsAsync(word, connection);
            }
        }

        private int GetLetterValue(string word)
        {
            BitArray bitArray = new BitArray(32);

            for (int i = 0; i < characters.Length; i++)
            {
                bool exists = false;

                for (int c = 0; c < word.Length; c++)
                {
                    if (word[c] == characters[i])
                    {
                        exists = true;
                        break;
                    }
                }

                bitArray[i] = exists;
            }

            int[] array = new int[1];
            bitArray.CopyTo(array, 0);
            return array[0];
        }

        private short GetLetterCount(string word, char c)
        {
            short count = 0;

            for (int i = 0; i < word.Length; i++)
            {
                if (word[i] == c)
                    count++;
            }

            return count;
        }
    }
}
