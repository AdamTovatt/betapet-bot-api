using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatBot.Helpers;

namespace ChatBot.Models.Data.Parsing
{
    public class ParseError
    {
        public string Token { get; set; }
        public int TokenIndex { get; set; }
        public string[] Expected { get; set; }
        public string Got { get; set; }
        public string? Description { get; set; }

        public ParseError(string token, int tokenIndex, string[] expected, string got)
        {
            Token = token;
            TokenIndex = tokenIndex;
            Expected = expected;
            Got = got;
        }

        public override string ToString()
        {
            return string.Format("Error at \"{0}\" ({1}). Expected \"{2}\" but instead got \"{3}\". \n({4})", Token, TokenIndex, Expected.ToList().GetStringsWithOrBetween(), Got, Description);
        }
    }
}
