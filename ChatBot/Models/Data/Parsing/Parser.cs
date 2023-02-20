using ChatBot.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Models.Data.Parsing
{
    public static class Parser
    {
        private enum ParseState
        {
            States, SingleState, EnterMessages, ExitMessages, Routes, SingleRoute
        }

        public static ParseResult ParseTrainingData(string input)
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
                            string stateName = tokens.ReadTokensUntill(i, out int stateNameOffset, "{", ":");
                            i += stateNameOffset;

                            string forwardName = tokens.ReadTokensUntill(i, out int forwardNameOffset, "{");
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
                                        string answerText = tokens.ReadTokensUntill(i, out int answerTextOffset, "\"");
                                        i += answerTextOffset;

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
                            currentStateName = tokens.ReadTokensUntill(i, out int offset, "{");
                            i += offset;

                            if (parsedStates.Where(x => x.Name == currentStateName).Count() == 0)
                            {
                                return ParseResult.CreateError(token, i, token, currentStateName).AddErrorDescription("Route with name: \"" + currentStateName + "\" is missing a state");
                            }

                            promptResponsePairs = new List<PromptResponsePair>();
                            stateStack.Push(ParseState.SingleRoute);
                        }
                    }
                    else if(parserState == ParseState.SingleRoute)
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
                            string response = tokens.ReadTokensUntill(i, out int responseOffset, "\"");
                            i += responseOffset + 1;
                            string prompt = tokens.ReadTokensUntill(i, out int promptOffset, "\"");
                            i += promptOffset;

                            if (promptResponsePairs == null)
                                throw new Exception("Has entered single route without creating instance of promptResponsePairs, this should not be possible");

                            promptResponsePairs.Add(new PromptResponsePair(prompt) { Response = response });
                        }
                    }
                }
            }

            foreach(State state in parsedStates.Where(x => x.ForwardState == null && x.Routes.Count == 0))
            {
                return ParseResult.CreateError(state.Name, -1, "no route", "routes for state").AddErrorDescription(state.Name + " is missing routes. Please provide routes for it or use the forward operator \":\" and then the state to forward to");
            }

            foreach(State state in parsedStates)
            {
                foreach(PromptResponsePair pair in state.Routes)
                {
                    if(parsedStates.Where(x => x.Name == pair.Response).Count() == 0)
                    {
                        return ParseResult.CreateError("route: " + state.Name + " / " + pair.Response, -1, pair.Prompt, parsedStates.GetNames().ToArray()).AddErrorDescription("The state \"" + pair.Response + "\" does not exist");
                    }
                }
            }

            return new ParseResult(new TrainingData(parsedStates));
        }
    }
}
