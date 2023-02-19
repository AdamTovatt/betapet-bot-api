using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Models.Data
{
    [DataContract]
    public class Conversation
    {
        [DataMember(Name = "Prompt")]
        public string Prompt { get; set; }

        [DataMember(Name = "Response")]
        public string? Response { get; set; }

        public Conversation(string prompt) {
            Prompt = prompt;
        }

        public override string ToString()
        {
            return string.Format("Q: {0} A:{1}", Prompt, Response, Prompt.Length);
        }
    }
}
