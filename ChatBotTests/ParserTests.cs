using ChatBot.Helpers;
using ChatBot.Models.Data;
using ChatBot.Models.Data.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBotTests
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod]
        public void AssertReadTokensWorks()
        {
            List<string> tokens = new List<string>() { "\"", "make", "claim", "yes", "\""};
            string result = tokens.ReadTokensUntill(1, out int offset, out _, "\"");

            Assert.IsTrue(result == "make claim yes");
            Assert.IsTrue(offset == 3);
        }

        [TestMethod]
        public void AssertListOfStringToStringsWithOrBetweenWorks()
        {
            List<string> strings = new List<string>() { "thing1", "thing2", "anotherThing" };
            Assert.IsTrue(strings.GetStringsWithOrBetween() == "thing1 or thing2 or anotherThing");
        }

        [TestMethod]
        public void AssertUnfinishedFileResultsInComprehensibleError()
        {
            string input = "states{";

            ParseResult parseResult = Parser.ParseTrainingData(input);

            Assert.IsTrue(parseResult.Error != null);
            Assert.IsTrue(parseResult.Error.Expected.First() == "}");
        }

        [TestMethod]
        public void AssertUnfinishedFileResultsInComprehensibleError2()
        {
            string input = "states{default{";

            ParseResult parseResult = Parser.ParseTrainingData(input);

            Assert.IsTrue(parseResult.Error != null);
            Assert.IsTrue(parseResult.Error.Expected.First() == "}");
        }

        [TestMethod]
        public void AssertUnfinishedStateStringReturnsComprehensibleError()
        {
            string input = "states{default{enter{0:\"hi how are";

            ParseResult parseResult = Parser.ParseTrainingData(input);

            Assert.IsTrue(parseResult.Error != null);
            Assert.IsTrue(parseResult.Error.Expected.First() == "\"");
        }

        [TestMethod]
        public void AssertEnterMessageWithoutProperVisitsCountReturnsComprehensibleError()
        {
            string input = "states{default{enter{:\"hi how are\"}}";

            ParseResult parseResult = Parser.ParseTrainingData(input);

            Assert.IsTrue(parseResult.Error != null);
            Assert.IsTrue(parseResult.Error.Expected.First() == "integer");
        }

        [TestMethod]
        public void AssertStateWithoutRouteReturnsComprehensibleError()
        {
            string input = "states{default{enter{0:\"hi how are\"}}";

            ParseResult parseResult = Parser.ParseTrainingData(input);

            Assert.IsTrue(parseResult.Error != null);
            Assert.IsTrue(parseResult.Error.Expected.First() == "route for state");
            Assert.IsTrue(parseResult.Error.Got == "no route");
        }

        [TestMethod]
        public void AssertRouteToNonExistantErrerGivesComprehensibleError()
        {
            string input = "states{default{enter{0:\"hi how are\"}}routes{default{cancel insurance {\"hello\"}}}";

            ParseResult parseResult = Parser.ParseTrainingData(input);

            Assert.IsTrue(parseResult.Error != null);
            Assert.IsTrue(parseResult.Error.Expected.First() == "valid state name");
            Assert.IsTrue(parseResult.Error.Got == "routes");
        }

        [TestMethod]
        public void AssertParsingWorks()
        {
            string input = "states\r\n{\r\n    default\r\n    {\r\n        enter\r\n        {\r\n            0: \"Hi, how can I help you?\"\r\n            1: \"Is there anything else you need help with?\"\r\n        }\r\n    }\r\n    cancel insurance : default\r\n    {\r\n        enter\r\n        {\r\n            0: \"If you want to cancel your insurance you can go to the start menu and cancel it\"\r\n            0: \"Sure, just go to the start page and cancel it\"\r\n            1: \"As I said earlier, you can go to the start page and cancel it\"\r\n        }\r\n        exit\r\n        {\r\n            0: \"I'm glad I could help you with cancelling your insurance\"\r\n        }\r\n    }\r\n    make claim\r\n    {\r\n        enter\r\n        {\r\n            0: \"I will guide you through the process of making a claim. Have you had the insurance for more than 3 months?\"\r\n        }\r\n    }\r\n    make claim yes : default\r\n    {\r\n        enter\r\n        {\r\n            0: \"Then you will have to call the call center to cancel the insurance\"\r\n        }\r\n    }\r\n    make claim no : default\r\n    {\r\n        enter\r\n        {\r\n            0: \"Ok, then you can cancel it right away by clicking cancel\"\r\n        }\r\n    }\r\n    make claim cancel : default\r\n    {\r\n        enter\r\n        {\r\n            0: \"Oh, okay, I take it you don't want to cancel your claim then\"\r\n        }\r\n    }\r\n}\r\nroutes\r\n{\r\n    default\r\n    {\r\n        cancel insurance \"I want to cancel my insurance\"\r\n        cancel insurance \"Cancel insurance\"\r\n        cancel insurance \"I would like help with cancelling insurance\"\r\n        cancel insurance \"I was wondering how I could cancel my insurance\"\r\n        cancel insurance \"Can you help me with cancelling my insurance?\"\r\n        make claim \"I want to make a claim\"\r\n        make claim \"make a claim\"\r\n        make claim \"make claim\"\r\n        make claim \"My phone broke\"\r\n        make claim \"My computer is dead\"\r\n        make claim \"My computer broke\"\r\n        make claim \"I was riding my bike and suddenly I hit a rock\"\r\n        make claim \"I dropped my phone\"\r\n        make claim \"I want to make an insurance claim\"\r\n        make claim \"I want to make an insurance claim for my phone\"\r\n    }\r\n    make claim\r\n    {\r\n        make claim yes \"yes\"\r\n        make claim yes \"yes I have\"\r\n        make claim yes \"I have\"\r\n        make claim yes \"indeed\"\r\n        make claim no \"no\"\r\n        make claim no \"I don't think so\"\r\n        make claim yes \"I don't know\"\r\n        make claim cancel \"I don't want to make a claim\"\r\n        make claim cancel \"I want to cancel my insurance\"\r\n        make claim cancel \"cancel\"\r\n        make claim cancel \"don't make the claim\"\r\n    }\r\n}";

            ParseResult parseResult = Parser.ParseTrainingData(input);

            Assert.IsTrue(parseResult.Error == null);
            Assert.IsTrue(parseResult.Data != null);
            Assert.IsTrue(parseResult.Data!.States.Where(x => x.Name == "default").FirstOrDefault() != null);

            TrainingData result = parseResult.Data;
            State? makeClaim = result.States.Where(x => x.Name == "make claim").FirstOrDefault();
            Assert.IsTrue(makeClaim != null);
            Assert.IsTrue(makeClaim.Routes.Where(x => x.Prompt == "I don't know").Count() > 0);
        }

        [TestMethod]
        public void AssertMultipleRoutesInBracesWorks()
        {
            string input = "states{default{enter{0:\"hi\"}}cancel{enter{0:\"ok I will cancel then\"}exit{0:\"goodbye\"}}}routes{default{cancel{\"I want to cancel\"\"I don't want to go on\"}}cancel{default{\"take me home\"\"home\"\"enough\"}cancel{\"stay here\"}}}";

            ParseResult parseResult = Parser.ParseTrainingData(input);

            Assert.IsTrue(parseResult.Error == null);
            Assert.IsTrue(parseResult.Data != null);

            TrainingData data = parseResult.Data;
            State? defaultState = data.GetState("default");

            Assert.IsTrue(defaultState != null);
            Assert.IsTrue(defaultState.Routes.Count == 2);

            State? cancelState = data.GetState("cancel");

            Assert.IsTrue(cancelState != null);
            Assert.IsTrue(cancelState.Routes.Count == 4);
        }

        [TestMethod]
        public void AssertMissingRoutesLeadsToError()
        {
            string input = "states\r\n{\r\n    default\r\n    {\r\n        enter\r\n        {\r\n            0: \"Hi, how can I help you?\"\r\n            1: \"Is there anything else you need help with?\"\r\n        }\r\n    }\r\n    cancel insurance\r\n    {\r\n        enter\r\n        {\r\n            0: \"If you want to cancel your insurance you can go to the start menu and cancel it\"\r\n            0: \"Sure, just go to the start page and cancel it\"\r\n            1: \"As I said earlier, you can go to the start page and cancel it\"\r\n        }\r\n        exit\r\n        {\r\n            0: \"I'm glad I could help you with cancelling your insurance\"\r\n        }\r\n    }\r\n    make claim\r\n    {\r\n        enter\r\n        {\r\n            0: \"I will guide you through the process of making a claim. Have you had the insurance for more than 3 months?\"\r\n        }\r\n    }\r\n    make claim yes : default\r\n    {\r\n        enter\r\n        {\r\n            0: \"Then you will have to call the call center to cancel the insurance\"\r\n        }\r\n    }\r\n    make claim no : default\r\n    {\r\n        enter\r\n        {\r\n            0: \"Ok, then you can cancel it right away by clicking cancel\"\r\n        }\r\n    }\r\n    make claim cancel : default\r\n    {\r\n        enter\r\n        {\r\n            0: \"Oh, okay, I take it you don't want to cancel your claim then\"\r\n        }\r\n    }\r\n}\r\nroutes\r\n{\r\n    default\r\n    {\r\n        cancel insurance \"I want to cancel my insurance\"\r\n        cancel insurance \"Cancel insurance\"\r\n        cancel insurance \"I would like help with cancelling insurance\"\r\n        cancel insurance \"I was wondering how I could cancel my insurance\"\r\n        cancel insurance \"Can you help me with cancelling my insurance?\"\r\n        make claim \"I want to make a claim\"\r\n        make claim \"make a claim\"\r\n        make claim \"make claim\"\r\n        make claim \"My phone broke\"\r\n        make claim \"My computer is dead\"\r\n        make claim \"My computer broke\"\r\n        make claim \"I was riding my bike and suddenly I hit a rock\"\r\n        make claim \"I dropped my phone\"\r\n        make claim \"I want to make an insurance claim\"\r\n        make claim \"I want to make an insurance claim for my phone\"\r\n    }\r\n    make claim\r\n    {\r\n        make claim yes \"yes\"\r\n        make claim yes \"yes I have\"\r\n        make claim yes \"I have\"\r\n        make claim yes \"indeed\"\r\n        make claim no \"no\"\r\n        make claim no \"I don't think so\"\r\n        make claim yes \"I don't know\"\r\n        make claim cancel \"I don't want to make a claim\"\r\n        make claim cancel \"I want to cancel my insurance\"\r\n        make claim cancel \"cancel\"\r\n        make claim cancel \"don't make the claim\"\r\n    }\r\n}";

            ParseResult parseResult = Parser.ParseTrainingData(input);

            Assert.IsTrue(parseResult.Error != null);
        }
    }
}
