using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Models.Prediction
{
    public class ConversationResponse
    {
        public string Text { get; set; }
        public float Probability { get; set; }

        public ConversationResponse(string text, float probability)
        {
            Text = text;
            Probability = probability;
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", Probability.ToString("0.0"), Text);
        }
    }
}
