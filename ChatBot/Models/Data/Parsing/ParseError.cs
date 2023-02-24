using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatBot.Helpers;

namespace ChatBot.Models.Data.Parsing
{
    /// <summary>
    /// A class containing any parsing errors that occurred
    /// </summary>
    public class ParseError
    {
        /// <summary>
        /// The token that the error occurred at
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// The index of the token that the error occurred at
        /// </summary>
        public int TokenIndex { get; set; }
        /// <summary>
        /// A list of things that the parser expected
        /// </summary>
        public string[] Expected { get; set; }
        /// <summary>
        /// What the parser got
        /// </summary>
        public string Got { get; set; }
        /// <summary>
        /// A description describing the problem
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Public constructor for creating a parse error object
        /// </summary>
        /// <param name="token">The token that the error occurred at</param>
        /// <param name="tokenIndex">The index of the token that the error occurred at</param>
        /// <param name="expected">What the parser was expecting</param>
        /// <param name="got">What the parser got</param>
        public ParseError(string token, int tokenIndex, string[] expected, string got)
        {
            Token = token;
            TokenIndex = tokenIndex;
            Expected = expected;
            Got = got;
        }

        /// <summary>
        /// Will return the error formatted for readability. Good for giving the user error messages of what went wrong with the parsing
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Error at \"{0}\" ({1}). Expected \"{2}\" but instead got \"{3}\". \n({4})", Token, TokenIndex, Expected.ToList().GetStringsWithOrBetween(), Got, Description);
        }
    }
}
