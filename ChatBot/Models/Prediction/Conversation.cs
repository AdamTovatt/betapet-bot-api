using Newtonsoft.Json;

namespace ChatBot.Models.Prediction
{
    /// <summary>
    /// Class for storing information about a conversation that a user has with a bot
    /// </summary>
    public class Conversation
    {
        /// <summary>
        /// The name of the current state in the conversation
        /// </summary>
        public string CurrentStateName { get; set; }

        /// <summary>
        /// A dictionary containing information about how many times each state has been visited
        /// </summary>
        public Dictionary<string, int> RecentVistsPerState { get; set; }

        internal Conversation()
        {
            RecentVistsPerState = new Dictionary<string, int>();
            CurrentStateName = "default";
        }

        internal Conversation(List<string> states, string startStateName)
        {
            RecentVistsPerState = new Dictionary<string, int>();

            foreach(string state in states)
            {
                RecentVistsPerState.Add(state, 0);
            }

            CurrentStateName = startStateName;
        }

        /// <summary>
        /// Will get the times a state with the provided name has been visited during this conversation
        /// </summary>
        /// <param name="stateName">The name to look for</param>
        /// <returns></returns>
        public int GetRecentVisits(string stateName)
        {
            return RecentVistsPerState[stateName];
        }

        /// <summary>
        /// Will add to a the visit count for a state
        /// </summary>
        /// <param name="name">The state to add to the count for</param>
        /// <param name="amountToAdd">The amount to add</param>
        public void AddToConversationalState(string name, int amountToAdd)
        {
            RecentVistsPerState[name] += amountToAdd;
        }

        /// <summary>
        /// Will deserialize the object from json
        /// </summary>
        /// <param name="json">The json to deserialize from</param>
        /// <returns></returns>
        public static Conversation? FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Conversation>(json);
        }

        /// <summary>
        /// Will serialize the object to json
        /// </summary>
        /// <returns></returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
