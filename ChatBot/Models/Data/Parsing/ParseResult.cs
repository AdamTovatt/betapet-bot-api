namespace ChatBot.Models.Data.Parsing
{
    public class ParseResult
    {
        public ParseError? Error { get; set; }
        public TrainingData? Data { get; set; }

        public ParseResult(ParseError error)
        {
            Error = error;
        }

        public ParseResult(TrainingData trainingData)
        {
            Data = trainingData;
        }

        public static ParseResult CreateError(string token, int tokenIndex, string got, params string[] expected)
        {
            return new ParseResult(new ParseError(token, tokenIndex, expected, got));
        }

        public ParseResult AddErrorDescription(string description)
        {
            if(Error == null)
                throw new Exception("Can't add error description to a parse result without an error");

            Error.Description = description;
            return this;
        }
    }
}
