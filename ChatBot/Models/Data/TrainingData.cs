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

        public TrainingData(List<State> states)
        {
            States = states;
        }
    }
}
