namespace ChatBot.Models.Data
{
    /// <summary>
    /// A class containing training data for a bot
    /// </summary>
    public class TrainingData
    {
        /// <summary>
        /// A list of all declared states for the bot
        /// </summary>
        public List<State> States { get; set; }

        private Dictionary<string, State> stateDictionary;

        /// <summary>
        /// Public constructor for creating a training data object
        /// </summary>
        /// <param name="states">The list of states that exist in this training data</param>
        /// <exception cref="Exception"></exception>
        public TrainingData(List<State> states)
        {
            States = states;

            stateDictionary = new Dictionary<string, State>();

            foreach(State state in States)
            {
                if (stateDictionary.ContainsKey(state.Name))
                    throw new Exception("Duplicate of state called: " + state.Name + " found. This is probably because it is declared twice in the training data");

                stateDictionary.Add(state.Name, state);
            }
        }

        /// <summary>
        /// Will return a name with from a specified name. Will return null if no state is found
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public State? GetState(string name)
        {
            if (stateDictionary.TryGetValue(name, out State? state))
                return state;

            return null;
        }
    }
}
