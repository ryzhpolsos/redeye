using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RedEye.Core {
    class RwmlExpressionParser {
        public RwmlExpressionParser(IPluginManager pluginManager, IVariableStorage<string> variableStorage){
            Shared.PluginManager = pluginManager;
            Shared.VariableStorage = variableStorage;
        }

        static class Shared {
            public static IPluginManager PluginManager;
            public static IVariableStorage<string> VariableStorage;
            public static Regex VariableRegex = new Regex(@"\$\{([\w.]+)\}", RegexOptions.Compiled);
        }

        enum TokenType {
            None,
            LParen,
            RParen,
            String,
            RawString,
            Ident,
            Comma
        }

        struct Token {
            public Token(TokenType type, string value = ""){
                Type = type;
                Value = value;
            }

            public TokenType Type;
            public string Value;
        }

        enum State {
            None,
            Expression,
            String,
            RawString,
            Escape
        }

        class SpecialCharacters {
            public const char LParen = '(';
            public const char RParen = ')';
            public const char Comma = ',';
            public const char String = '\'';
            public const char RawStringStart = '{';
            public const char RawStringEnd = '}';
            public const char Escape = '`';
        }

        class BaseOperation {
            public string Value;
            public List<BaseOperation> Arguments;

            public BaseOperation(string value, params BaseOperation[] arguments){
                Value = value;
                Arguments = arguments.ToList();
            }

            public virtual string Invoke(){
                throw new NotImplementedException();   
            }

            public override string ToString(){
                return $"{GetType().Name} ['{Value}'] ('{string.Join("', '", Arguments.Select(x => x.ToString()))}')";
            }
        }

        class LiteralValueOperation : BaseOperation {
            public LiteralValueOperation(string value, params BaseOperation[] arguments) : base(value, arguments){}

            public override string Invoke(){
                return Value;
            }
        }

        class FirstArgumentValueOperation : BaseOperation {
            public FirstArgumentValueOperation(string value, params BaseOperation[] arguments) : base(value, arguments){}

            public override string Invoke(){
                return Arguments[0].Invoke();
            }
        }

        class FunctionCallOperation : BaseOperation {
            public FunctionCallOperation(string value, params BaseOperation[] arguments) : base(value, arguments){}

            public override string Invoke(){
                return Shared.PluginManager.GetExportedFunction(Value).Invoke(Arguments.Select(arg => arg.Invoke()), Shared.VariableStorage).ToString();
            }
        }

        class OperationStackItem {
            public Type Type = typeof(BaseOperation);
            public string Value = string.Empty;
            public List<BaseOperation> Arguments = new List<BaseOperation>();
        }

        string ParseVars(string value){
            if(!value.Contains("$")) return value;
            return Shared.VariableRegex.Replace(value, match => {
                return match.Groups.Count > 1 ? Shared.VariableStorage.GetVariable(match.Groups[1].Value) : string.Empty;
            });

        }

        public string Evaluate(string expression){
            var tokens = Tokenize(expression);
            // Console.WriteLine();

            var emptyToken = new Token(TokenType.None);

            var value = string.Empty;
            var argList = new List<string>();
            var operationStack = new Stack<BaseOperation>();

            operationStack.Push(new FirstArgumentValueOperation(""));

            for(int i = 0; i < tokens.Count; i++){
                var token = tokens[i];
                var nextToken = (i + 1 >= tokens.Count) ? emptyToken : tokens[i + 1];

                switch(token.Type){
                    case TokenType.Ident: {
                        if(nextToken.Type == TokenType.LParen){
                            var op = new FunctionCallOperation(token.Value);
                            operationStack.Peek().Arguments.Add(op);
                            operationStack.Push(op);
                        }else{
                            operationStack.Peek().Arguments.Add(new LiteralValueOperation(ParseVars(token.Value)));
                        }

                        break;
                    }

                    case TokenType.RParen: {
                        operationStack.Pop();
                        break;
                    }

                    case TokenType.String: {
                        operationStack.Peek().Arguments.Add(new LiteralValueOperation(ParseVars(token.Value)));
                        break;
                    }

                    case TokenType.RawString: {
                        operationStack.Peek().Arguments.Add(new LiteralValueOperation(token.Value));
                        break;
                    }
                }
            }

            void PrintOp(BaseOperation op, int tabAmount = 0){
                Console.WriteLine($"{new string(' ', tabAmount * 4)}[{op.GetType().Name}] '{op.Value}'" + (op.Arguments.Any() ? " =>" : ""));
                foreach(var arg in op.Arguments) PrintOp(arg, tabAmount + 1);
            }

            var rop = operationStack.Pop();
            // PrintOp(rop);

            return rop.Invoke();
        }

        bool IsValidExpressionChar(char ch){
            return char.IsLetter(ch) || char.IsDigit(ch) || ch == '$' || ch == '{' || ch == '}' || ch == '.' || ch == '-';
        }

        List<Token> Tokenize(string expression){
            var state = State.None;
            var pState = State.None;
            var tokens = new List<Token>();
            var buffer = new StringBuilder();

            void SetState(State st){
                pState = state;
                state = st;
            }

            foreach(var ch in expression){
                switch(state){
                    case State.Escape: {
                        buffer.Append(ch);
                        SetState(pState);
                        break;
                    }

                    case State.None: {
                        switch(ch){
                            case SpecialCharacters.String: {
                                SetState(State.String);
                                break;
                            }

                            case SpecialCharacters.RawStringStart: {
                                SetState(State.RawString);
                                break;
                            }

                            default: {
                                if(IsValidExpressionChar(ch)){
                                    SetState(State.Expression);
                                    buffer.Append(ch);
                                }

                                break;
                            }
                        }

                        break;
                    }

                    case State.Expression: {
                        if(IsValidExpressionChar(ch)){
                            buffer.Append(ch);
                        }else{
                            SetState(State.None);
                            tokens.Add(new Token(TokenType.Ident, buffer.ToString()));
                            buffer.Clear();
                        }

                        break;
                    }

                    case State.String: {
                        if(ch == SpecialCharacters.Escape){
                            SetState(State.Escape);
                        }else if(ch == SpecialCharacters.String){
                            tokens.Add(new Token(TokenType.String, buffer.ToString()));

                            buffer.Clear();
                            SetState(State.None);
                        }else{
                            buffer.Append(ch);
                        }

                        break;
                    }

                    case State.RawString: {
                        if(ch == SpecialCharacters.Escape){
                            SetState(State.Escape);
                        }else if(ch == SpecialCharacters.RawStringEnd){
                            tokens.Add(new Token(TokenType.RawString, buffer.ToString()));
                           
                            buffer.Clear();
                            SetState(State.None);
                        }else{
                            buffer.Append(ch);
                        }

                        break;
                    }
                }
                
                if(state != State.String && state != State.RawString){
                    var tokType = TokenType.None;

                    if(ch == SpecialCharacters.LParen){
                        tokType = TokenType.LParen;
                    }else if(ch == SpecialCharacters.RParen){
                        tokType = TokenType.RParen;
                    }else if(ch == SpecialCharacters.Comma){
                        tokType = TokenType.Comma;
                    }

                    if(tokType != TokenType.None){
                        tokens.Add(new Token(tokType));
                    }
                }

            }

            // foreach(var token in tokens){
            //     Console.WriteLine($"{token.Type}: '{token.Value}'");
            // }

            return tokens;
        }
    }
}
