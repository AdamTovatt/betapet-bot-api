using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatNeuralNetworkTrainer
{
    public class ConversationService
    {
        private readonly MLContext _mlContext;
        private PredictionEngine<Conversation, ConversationPrediction> _predEngine;

        public MLContext MlContext { get { return _mlContext;  } }

        public ConversationService()
        {
            _mlContext = new MLContext(seed: 0);
        }

        public void LoadModel(string modelPath)
        {
            ITransformer loadedModel;
            using (var stream = new FileStream(modelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                loadedModel = _mlContext.Model.Load(stream, out var modelInputSchema);
            _predEngine = _mlContext.Model.CreatePredictionEngine<Conversation, ConversationPrediction>(loadedModel);
        }

        public void LoadModel(byte[] bytes)
        {
            ITransformer loadedModel;

            using (MemoryStream stream = new MemoryStream(bytes))
                loadedModel = _mlContext.Model.Load(stream, out var modelInputSchema);

            _predEngine = _mlContext.Model.CreatePredictionEngine<Conversation, ConversationPrediction>(loadedModel);
        }

        public List<ConversationResponse> PredictResponse(Conversation conversation)
        {
            List<ConversationResponse> result = new List<ConversationResponse>();

            ConversationPrediction prediction = new ConversationPrediction();
            _predEngine.Predict(conversation, ref prediction);

            VBuffer<ReadOnlyMemory<char>> labelBuffer = new VBuffer<ReadOnlyMemory<char>>();
            _predEngine.OutputSchema["Score"].Annotations.GetValue("SlotNames", ref labelBuffer);
            string[]? labels = labelBuffer.DenseValues().Select(l => l.ToString()).ToArray();

            for (int i = 0; i < prediction.Score.Length; i++)
            {
                result.Add(new ConversationResponse() { Probability = prediction.Score[i], Text = labels[i] });
            }

            result = result.OrderByDescending(x => x.Probability).ToList();

            return result;
        }
    }
}
