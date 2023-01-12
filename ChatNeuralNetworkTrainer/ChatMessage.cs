using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatNeuralNetworkTrainer
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public int MatchId { get; set; }
        public int LocalId { get; set; }
        public int AuthorId { get; set; }
        public string Text { get; set; }
        public DateTime TimeOfCreation { get; set; }
        public bool SentByUs { get; set; }

        public override string ToString()
        {
            return string.Format("{0}: {1}", AuthorId, Text);
        }
    }
}
