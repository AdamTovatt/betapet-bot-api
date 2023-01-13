using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatNeuralNetworkTrainer
{
    public class OutputWord
    {
        public string Text { get; set; }
        public double Certainty { get; set; }

        public static string GetAsString(List<OutputWord> words)
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (OutputWord word in words)
            {
                stringBuilder.Append(string.Format("{0}({1}) ", word.Text, word.Certainty.ToString("0.00")));
            }

            return stringBuilder.ToString();
        }
    }
}
