using Newtonsoft.Json;

namespace ChatBot.Models.Prediction
{
    public class Conversation
    {
        public string CurrentStateName { get; set; }
        public Dictionary<string, int> RecentVistsPerState { get; set; }

        public Conversation()
        {
            RecentVistsPerState = new Dictionary<string, int>();
            CurrentStateName = "default";
        }

        public Conversation(List<string> states, string startStateName)
        {
            RecentVistsPerState = new Dictionary<string, int>();

            foreach(string state in states)
            {
                RecentVistsPerState.Add(state, 0);
            }

            CurrentStateName = startStateName;
        }

        public int GetRecentVisits(string stateName)
        {
            return RecentVistsPerState[stateName];
        }

        public void AddToConversationalState(string name, int amountToAdd)
        {
            RecentVistsPerState[name] += amountToAdd;
        }

        public static Conversation? FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Conversation>(json);
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
