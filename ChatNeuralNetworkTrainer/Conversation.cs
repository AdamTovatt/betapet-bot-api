using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatNeuralNetworkTrainer
{
    public class Conversation
    {
        public string Promt { get; set; }
        public string Response { get; set; }

        public override string ToString()
        {
            return string.Format("{0} --- {1} --- promptLength: {2}", Promt, Response, Promt.Length);
        }
    }
}
