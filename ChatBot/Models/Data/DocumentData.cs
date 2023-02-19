using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Models.Data
{
    public class DocumentData
    {
        public string ConversationData { get; set; }
        public string ResponseData { get; set; }

        public List<Conversation> Conversations { get { if (_conversations == null) _conversations = ParseConversations(); return _conversations; } }
        private List<Conversation> _conversations;

        public Dictionary<string, List<string>> Responses { get { if (_responses == null) _responses = ParseResponses(); return _responses; } }
        private Dictionary<string, List<string>> _responses;

        private Random random;

        public DocumentData(string fullDocument)
        {
            if (fullDocument.Contains("**Responses**"))
            {
                string[] parts = fullDocument.Split("**Responses**");
                ConversationData = parts[0];
                ResponseData = parts[1];
            }
            else
            {
                ConversationData = fullDocument;
            }

            random = new Random();
        }

        private List<Conversation> ParseConversations()
        {
            List<Conversation> result = new List<Conversation>();

            string[] lines = ConversationData.Split("\n");

            Conversation conversation = null;

            int type = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrEmpty(lines[i].Trim()))
                    continue;

                if (type == 0)
                {
                    string line = lines[i].ToLower();

                    if (!line.StartsWith("q:"))
                        throw new Exception("Error in training data! Expected the beggning of an answer with \"Q:\" but instead got: " + line);
                    conversation = new Conversation(line.Substring(2));
                }
                if (type == 1)
                {
                    string line = lines[i].ToLower();

                    if (!line.StartsWith("a:"))
                        throw new Exception("Error in training data! Expected the beggning of an answer with \"A:\" but instead got: " + line);

                    conversation.Response = line.Substring(2);
                    result.Add(conversation);
                }

                type++;
                if (type == 2)
                    type = 0;
            }

            return result;
        }

        private Dictionary<string, List<string>> ParseResponses()
        {
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();

            string[] lines = ResponseData.Split("\n");

            string currentClass = "";
            List<string> currentResponses = new List<string>();

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line))
                    continue;

                if (line.ToLower().StartsWith("p:"))
                {
                    if (currentResponses.Count > 0)
                    {
                        result.Add(currentClass, currentResponses);
                    }

                    currentClass = line.ToLower().Substring(2).Trim();
                    currentResponses = new List<string>();
                }
                else if (line.ToLower().StartsWith("r:"))
                {
                    currentResponses.Add(line.Substring(2).Trim());
                }
                else throw new Exception("Unexpected string when parsing responses: " + lines[i]);
            }

            if (currentResponses.Count > 0)
            {
                if (!result.ContainsKey(currentClass))
                    result.Add(currentClass, currentResponses);
            }

            return result;
        }

        public string GetResponse(string responseClass)
        {
            if (Responses.TryGetValue(responseClass.Trim(), out List<string> responses))
            {
                return responses[random.Next(responses.Count)];
            }
            else
            {
                return "Error, no such response class found: " + responseClass;
            }
        }
    }
}
