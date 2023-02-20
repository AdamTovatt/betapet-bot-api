using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ChatBot.Models.Data.Parsing
{
    public static class Tokenizer
    {
        public static List<string> GetTokens(string document)
        {
            List<string> tokens = new List<string>();

            foreach(string token in Regex.Split(document, "([:{}\"\" ])"))
            {
                string currentToken = token.Trim();

                if (!string.IsNullOrEmpty(currentToken))
                    tokens.Add(currentToken);
            }

            return tokens;
        }
    }
}
