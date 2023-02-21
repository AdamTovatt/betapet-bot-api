using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Models.Data
{
    public class TrainingData
    {
        public List<State> States { get; set; }

        private Dictionary<string, State> stateDictionary;

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

        public State? GetState(string name)
        {
            if (stateDictionary.TryGetValue(name, out State? state))
                return state;

            return null;
        }
    }
}
