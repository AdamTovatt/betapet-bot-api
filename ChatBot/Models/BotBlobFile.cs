using ChatBot.Models.Prediction;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Models
{
    internal class BotBlobFile
    {
        internal class ModelByteArrayPosition
        {
            [JsonProperty]
            public string Name { get; set; }

            [JsonProperty]
            public int Start { get; set; }

            [JsonProperty]
            public int Length { get; set; }

            internal ModelByteArrayPosition(string name, int start, int length)
            {
                Name = name;
                Start = start;
                Length = length;
            }
        }

        public Dictionary<string, ConversationalState> States { get; set; }
        public List<ModelByteArrayPosition> ModelPositions { get; set; }

        [JsonIgnore]
        public int LengthSum { get; set; }

        internal BotBlobFile()
        {
            States = new Dictionary<string, ConversationalState>();
            ModelPositions = new List<ModelByteArrayPosition>();
        }

        internal BotBlobFile(Dictionary<string, ConversationalState> states)
        {
            States = states;

            Dictionary<string, byte[]> modelsAsByteArrays = new Dictionary<string, byte[]>();

            foreach (ConversationalState state in States.Values)
            {
                if (state.ConversationService != null)
                    modelsAsByteArrays.Add(state.Name, state.ConversationService.GetBytes());
            }

            ModelPositions = new List<ModelByteArrayPosition>();

            int currentOffset = 0;
            foreach (string key in modelsAsByteArrays.Keys)
            {
                ModelPositions.Add(new ModelByteArrayPosition(key, currentOffset, modelsAsByteArrays[key].Length));
                currentOffset += modelsAsByteArrays[key].Length;
                LengthSum += modelsAsByteArrays[key].Length;
            }
        }
    }
}
