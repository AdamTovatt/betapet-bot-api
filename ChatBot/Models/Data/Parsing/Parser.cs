using ChatBot.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Models.Data.Parsing
{
    /// <summary>
    /// Class containing the Parse method that can be used to convert from a training data file to a training data object that can be used with a bot
    /// </summary>
    public static class Parser
    {
        private enum ParseState
        {
            States, SingleState, EnterMessages, ExitMessages, Routes, SingleRoute
        }

        /// <summary>
        /// Function for parsing a training data file to a training data object that can be used with the a bot
        /// </summary>
        /// <param name="input">The training data file</param>
        /// <returns>A ParseResult object that will contain information about the parsed data or any errors that occured during the parsing</returns>
        /// <exception cref="Exception"></exception>
        public static ParseResult Parse(string input)
        {
            List<State> parsedStates = new List<State>();

            List<string> tokens = Tokenizer.GetTokens(input);

            Stack<ParseState> stateStack = new Stack<ParseState>();

            State? currentState = null;
            List<PromptResponsePair>? promptResponsePairs = null;
            string? currentStateName = null;

            for (int i = 0; i < tokens.Count; i++)
            {
                string token = tokens.Get(i);

                if (stateStack.Count == 0)
                {
                    if (token == "states")
                    {
                        if (tokens.Get(i + 1) == "{")
                        {
                            i++;
                            stateStack.Push(ParseState.States);
                        }
                        else
                        {
                            return ParseResult.CreateError(token, i, tokens.Get(i + 1), "{");
                        }
                    }
                    else if (token == "routes")
                    {
                        if (tokens.Get(i + 1) == "{")
                        {
                            i++;
                            stateStack.Push(ParseState.Routes);
                        }
                        else
                        {
                            return ParseResult.CreateError(token, i, tokens.Get(i + 1), "{");
                        }
                    }
                    else
                    {
                        return ParseResult.CreateError(token, i, tokens.Get(i + 1), "states", "routes").AddErrorDescription("Invalid start of file");
                    }
                }
                else
                {
                    ParseState parserState = stateStack.Peek();
                    if (parserState == ParseState.States) //is parsing states
                    {
                        if (token == "}")
                        {
                            stateStack.Pop();
                        }
                        else
                        {
                            string stateName = tokens.ReadTokensUntill(i, out int stateNameOffset, out string? _, "{", ":");
                            i += stateNameOffset;

                            if (stateName == "routes")
                                return ParseResult.CreateError(tokens.Get(i), i, stateName, "valid state name").AddErrorDescription("A state can not be called \"routes\". If you did not intend on having a state called routes, maybe you didn't close the states section properly before trying to declare routes");

                            string forwardName = tokens.ReadTokensUntill(i, out int forwardNameOffset, out string? _, "{");
                            i += forwardNameOffset;

                            currentState = new State(stateName);

                            if (!string.IsNullOrEmpty(forwardName))
                                currentState.ForwardState = forwardName.Substring(1).Trim();

                            if (tokens.Get(i) != "{")
                            {
                                return ParseResult.CreateError(tokens.Get(i), i, tokens.Get(i + 1), "{").AddErrorDescription("Missing { after state name declaration");
                            }

                            stateStack.Push(ParseState.SingleState);
                        }
                    }
                    else if (parserState == ParseState.SingleState)
                    {
                        if (tokens.Get(i) == "enter")
                        {
                            if (tokens.Get(i + 1) == "{")
                            {
                                i++;
                                stateStack.Push(ParseState.EnterMessages);
                            }
                            else
                            {
                                return ParseResult.CreateError(tokens.Get(i), i, tokens.Get(i + 1), "{").AddErrorDescription("Missing { after enter keyword");
                            }
                        }
                        else if (tokens.Get(i) == "exit")
                        {
                            if (tokens.Get(i + 1) == "{")
                            {
                                i++;
                                stateStack.Push(ParseState.ExitMessages);
                            }
                            else
                            {
                                return ParseResult.CreateError(tokens.Get(i), i, tokens.Get(i + 1), "{").AddErrorDescription("Missing { after exit keyword");
                            }
                        }
                        else if (tokens.Get(i) == "}")
                        {
                            if (currentState != null)
                            {
                                parsedStates.Add(currentState);
                                currentState = null;
                            }
                            stateStack.Pop();
                        }
                        else
                        {
                            return ParseResult.CreateError(tokens.Get(i), i, tokens.Get(i), "enter", "exit").AddErrorDescription("Invalid state definition, expected enter or exit keywords");
                        }
                    }
                    else if (parserState == ParseState.EnterMessages || parserState == ParseState.ExitMessages)
                    {
                        if (tokens.Get(i) == "}")
                        {
                            stateStack.Pop();
                        }
                        else
                        {
                            if (int.TryParse(token, out int recentVisits))
                            {
                                if (tokens.Get(i + 1) == ":")
                                {
                                    i += 2;

                                    if (tokens.Get(i) == "\"")
                                    {
                                        i++;
                                        string answerText = tokens.ReadTokensUntill(i, out int answerTextOffset, out string? _, "\"");
                                        i += answerTextOffset;

                                        if (i >= tokens.Count)
                                            return ParseResult.CreateError(tokens.Get(i), i, "untermintated string", "\"").AddErrorDescription("String starting at specified token is not terminated correctly");

                                        currentState!.AddResponse(parserState == ParseState.EnterMessages ? State.ResponseType.Enter : State.ResponseType.Exit, recentVisits, answerText);
                                    }
                                    else
                                    {
                                        return ParseResult.CreateError(token, i, tokens.Get(i), "\"").AddErrorDescription("Should be start of string that is response");
                                    }
                                }
                                else
                                {
                                    return ParseResult.CreateError(tokens.Get(i), i, tokens.Get(i + 1), ":").AddErrorDescription("Missing : after recentVisits integer declaration in enter/exit section");
                                }
                            }
                            else
                            {
                                return ParseResult.CreateError(tokens.Get(i), i, tokens.Get(i), "integer").AddErrorDescription("Expected integer for recentVisits value but instead found non integer value");
                            }
                        }
                    }
                    else if (parserState == ParseState.Routes)
                    {
                        if (token == "}")
                        {
                            stateStack.Pop();
                        }
                        else
                        {
                            currentStateName = tokens.ReadTokensUntill(i, out int offset, out string? _, "{");
                            i += offset;

                            if (parsedStates.Where(x => x.Name == currentStateName).Count() == 0)
                            {
                                return ParseResult.CreateError(token, i, token, currentStateName).AddErrorDescription("Route with name: \"" + currentStateName + "\" is missing a state");
                            }

                            promptResponsePairs = new List<PromptResponsePair>();
                            stateStack.Push(ParseState.SingleRoute);
                        }
                    }
                    else if (parserState == ParseState.SingleRoute)
                    {
                        if (token == "}")
                        {
                            if (promptResponsePairs != null && promptResponsePairs.Count > 0)
                            {
                                State? targetState = parsedStates.Where(x => x.Name == currentStateName).FirstOrDefault();

                                if (targetState != null)
                                {
                                    targetState.AddRoutes(promptResponsePairs);
                                }
                            }

                            promptResponsePairs = null;
                            stateStack.Pop();
                        }
                        else
                        {
                            string response = tokens.ReadTokensUntill(i, out int responseOffset, out string? stoppingToken, "\"", "{");
                            i += responseOffset + 1;

                            if (stoppingToken == "\"")
                            {
                                string prompt = tokens.ReadTokensUntill(i, out int promptOffset, out string? _, "\"");
                                i += promptOffset;

                                if (promptResponsePairs == null)
                                    throw new Exception("Has entered single route without creating instance of promptResponsePairs, this should not be possible");

                                promptResponsePairs.Add(new PromptResponsePair(prompt) { Response = response });
                            }
                            else
                            {
                                while (tokens.Get(i) == "\"")
                                {
                                    string prompt = tokens.ReadTokensUntill(i + 1, out int promptOffset, out string? _, "\"");
                                    i += promptOffset + 2;

                                    if (promptResponsePairs == null)
                                        throw new Exception("Has entered single route without creating instance of promptResponsePairs, this should not be possible");

                                    promptResponsePairs.Add(new PromptResponsePair(prompt) { Response = response });
                                }

                                if (tokens.Get(i) != "}")
                                    return ParseResult.CreateError(tokens.Get(i), i, tokens.Get(i), "}").AddErrorDescription("A collection of possible prompts for a route should end with \"}\"");
                            }
                        }
                    }
                }
            }

            foreach (State state in parsedStates.Where(x => x.ForwardState == null && x.Routes.Count == 0))
            {
                return ParseResult.CreateError(state.Name, -1, "no route", "route for state").AddErrorDescription(state.Name + " is missing routes. Please provide routes for it or use the forward operator \":\" and then the state to forward to");
            }

            foreach (State state in parsedStates)
            {
                foreach (PromptResponsePair pair in state.Routes)
                {
                    if (parsedStates.Where(x => x.Name == pair.Response).Count() == 0)
                    {
                        return ParseResult.CreateError("route: " + state.Name + " / " + pair.Response, -1, pair.Prompt, parsedStates.GetNames().ToArray()).AddErrorDescription("The state \"" + pair.Response + "\" does not exist");
                    }
                }
            }

            if (stateStack.Count > 0)
            {
                ParseState state = stateStack.Pop();

                switch (state)
                {
                    case ParseState.States:
                        return ParseResult.CreateError(tokens.Get(tokens.Count - 1), tokens.Count - 1, "end of file", "}").AddErrorDescription("States section not closed properly. Should be closed with }");
                    case ParseState.SingleState:
                        return ParseResult.CreateError(tokens.Get(tokens.Count - 1), tokens.Count - 1, "end of file", "}").AddErrorDescription("State section not closed properly. Should be closed with }");
                    case ParseState.EnterMessages:
                        return ParseResult.CreateError(tokens.Get(tokens.Count - 1), tokens.Count - 1, "end of file", "}").AddErrorDescription("Enter messages section not closed properly. Should be closed with }");
                    case ParseState.ExitMessages:
                        return ParseResult.CreateError(tokens.Get(tokens.Count - 1), tokens.Count - 1, "end of file", "}").AddErrorDescription("Exit messages section not closed properly. Should be closed with }");
                    case ParseState.Routes:
                        return ParseResult.CreateError(tokens.Get(tokens.Count - 1), tokens.Count - 1, "end of file", "}").AddErrorDescription("Routes section not closed properly. Should be closed with }");
                    case ParseState.SingleRoute:
                        return ParseResult.CreateError(tokens.Get(tokens.Count - 1), tokens.Count - 1, "end of file", "}").AddErrorDescription("Route section not closed properly. Should be closed with }");
                    default:
                        return ParseResult.CreateError(tokens.Get(tokens.Count - 1), tokens.Count - 1, "end of file", "}").AddErrorDescription("File not closed correctly. Should be closed with }");
                }
            }

            return new ParseResult(new TrainingData(parsedStates));
        }
    }
}
