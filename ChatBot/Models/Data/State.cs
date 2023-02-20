namespace ChatBot.Models.Data
{
    public class State
    {
        public enum ResponseType
        {
            Enter, Exit
        }

        public string Name { get; set; }
        public List<PromptResponsePair> Routes { get; set; }
        public Dictionary<int, List<string>> EnterResponses { get; set; }
        public Dictionary<int, List<string>> ExitResponses { get; set; }
        public string? ForwardState { get; set; }

        public State(string name)
        {
            Name = name;
            Routes = new List<PromptResponsePair>();
            EnterResponses = new Dictionary<int, List<string>>();
            ExitResponses = new Dictionary<int, List<string>>();
        }

        public State(string name, Dictionary<int, List<string>> enterResponses, Dictionary<int, List<string>> exitResponses, string forwardState)
        {
            Name = name;
            EnterResponses = enterResponses;
            ExitResponses = exitResponses;
            ForwardState = forwardState;
            Routes = new List<PromptResponsePair>();
        }

        public void AddRoutes(List<PromptResponsePair> routes)
        {
            Routes.AddRange(routes);
        }

        public void AddResponse(ResponseType responseType, int recentVisitsValue, string textValue)
        {
            Dictionary<int, List<string>> responseDictionary = responseType == ResponseType.Enter ? EnterResponses : ExitResponses;

            if(!responseDictionary.ContainsKey(recentVisitsValue))
            {
                responseDictionary.Add(recentVisitsValue, new List<string>());
            }

            responseDictionary[recentVisitsValue].Add(textValue);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
