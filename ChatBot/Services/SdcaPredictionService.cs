using ChatBot.Models.Data;
using ChatBot.Models.Prediction;
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Services
{
    public class SdcaPredictionService : PredictionService
    {
        public override List<ConversationResponse> PredictResponse(PromptResponsePair conversation)
        {
            if(_predEngine == null)
                throw new Exception("Prediction engine is null! A model needs to be loaded before a response can be predicted!");

            List<ConversationResponse> result = new List<ConversationResponse>();

            ConversationPrediction prediction = new ConversationPrediction();
            _predEngine.Predict(conversation, ref prediction);

            VBuffer<ReadOnlyMemory<char>> labelBuffer = new VBuffer<ReadOnlyMemory<char>>();
            _predEngine.OutputSchema["Score"].Annotations.GetValue("SlotNames", ref labelBuffer);
            string[]? labels = labelBuffer.DenseValues().Select(l => l.ToString()).ToArray();

            for (int i = 0; i < prediction.Score.Length; i++)
            {
                result.Add(new ConversationResponse(labels[i], prediction.Score[i]));
            }

            result = result.OrderByDescending(x => x.Probability).ToList();

            return result;
        }
    }
}
