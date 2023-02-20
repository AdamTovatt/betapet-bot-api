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
    public class PredictionService
    {
        private readonly MLContext _mlContext;
        private PredictionEngine<PromptResponsePair, ConversationPrediction>? _predEngine;

        public MLContext MlContext { get { return _mlContext; } }

        public PredictionService()
        {
            _mlContext = new MLContext(seed: 0);
        }

        public void LoadModel(string modelPath)
        {
            ITransformer loadedModel;
            using (var stream = new FileStream(modelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                loadedModel = _mlContext.Model.Load(stream, out var modelInputSchema);
            _predEngine = _mlContext.Model.CreatePredictionEngine<PromptResponsePair, ConversationPrediction>(loadedModel);
        }

        public void LoadModel(byte[] bytes)
        {
            ITransformer loadedModel;

            using (MemoryStream stream = new MemoryStream(bytes))
                loadedModel = _mlContext.Model.Load(stream, out var modelInputSchema);

            _predEngine = _mlContext.Model.CreatePredictionEngine<PromptResponsePair, ConversationPrediction>(loadedModel);
        }

        public List<ConversationResponse> PredictResponse(PromptResponsePair conversation)
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
