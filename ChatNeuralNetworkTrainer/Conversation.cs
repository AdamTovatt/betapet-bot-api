using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ChatNeuralNetworkTrainer
{
    [DataContract]
    public class Conversation
    {
        [DataMember(Name = "Prompt")]
        public string Promt { get; set; }

        [DataMember(Name = "Response")]
        public string Response { get; set; }

        public override string ToString()
        {
            return string.Format("{0} --- {1} --- promptLength: {2}", Promt, Response, Promt.Length);
        }
    }

    public class ConversationPrediction
    {
        [ColumnName("PredictedLabel")]
        public string Response { get; set; }

        [ColumnName("Score")]
        public float[] Score { get; set; }
    }

    public class ConversationResponse
    {
        public string Text { get; set; }
        public float Probability { get; set; }

        public override string ToString()
        {
            return string.Format("{0}: {1}", Probability.ToString("0.0"), Text);
        }
    }
}
