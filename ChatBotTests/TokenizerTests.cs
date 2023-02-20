using ChatBot.Models.Data.Parsing;

namespace ChatBotTests
{
    [TestClass]
    public class TokenizerTests
    {
        [TestMethod]
        public void AssertTokenCreationWorks()
        {
            string input = "states{default{enter{0:\"Hi, how can I help you\"1:\"Is there anything else?\"}}}";
            string[] expectedOutput = { "states", "{", "default", "{", "enter", "{", "0", ":", "\"", "Hi,", "how", "can", "I", "help", "you", "\"", "1", ":", "\"", "Is", "there", "anything", "else?", "\"", "}", "}", "}" };
            string[] actualOutput = Tokenizer.GetTokens(input).ToArray();

            Assert.IsTrue(expectedOutput.Length == actualOutput.Length);

            for (int i = 0; i < expectedOutput.Length; i++)
            {
                Assert.IsTrue(expectedOutput[i] == actualOutput[i]);
            }
        }
    }
}