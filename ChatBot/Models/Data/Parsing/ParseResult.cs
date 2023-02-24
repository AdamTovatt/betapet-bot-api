namespace ChatBot.Models.Data.Parsing
{
    /// <summary>
    /// The result from performing a Parse on a training data file
    /// </summary>
    public class ParseResult
    {
        /// <summary>
        /// If any errors occured this property will contain a ParseError object, otherwise it will be null
        /// </summary>
        public ParseError? Error { get; set; }
        /// <summary>
        /// If no errors occured this property will contain training data object that was parsed. If errors occured this will be null
        /// </summary>
        public TrainingData? Data { get; set; }

        /// <summary>
        /// Create a new ParseResult with a ParseError object. Used when parsing failed
        /// </summary>
        /// <param name="error"></param>
        public ParseResult(ParseError error)
        {
            Error = error;
        }

        /// <summary>
        /// Create a new ParseResult with a training data object. Used when parsing went well
        /// </summary>
        /// <param name="trainingData"></param>
        public ParseResult(TrainingData trainingData)
        {
            Data = trainingData;
        }

        /// <summary>
        /// Will create an error result
        /// </summary>
        /// <param name="token">The token that the error occured at</param>
        /// <param name="tokenIndex">The index of the token that the error occured at</param>
        /// <param name="got">What the parser got</param>
        /// <param name="expected">A list of things that the parser expected</param>
        /// <returns></returns>
        public static ParseResult CreateError(string token, int tokenIndex, string got, params string[] expected)
        {
            return new ParseResult(new ParseError(token, tokenIndex, expected, got));
        }

        /// <summary>
        /// Will add a description to an error object
        /// </summary>
        /// <param name="description">The description to add</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public ParseResult AddErrorDescription(string description)
        {
            if(Error == null)
                throw new Exception("Can't add error description to a parse result without an error");

            Error.Description = description;
            return this;
        }
    }
}
