using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Models.Prediction
{
    public class ConversationPrediction
    {
        [ColumnName("PredictedLabel")]
        public string Response { get; set; }

        [ColumnName("Score")]
        public float[] Score { get; set; }
    }
}
