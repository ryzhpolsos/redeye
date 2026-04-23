using System.Collections.Generic;
using System.Text.RegularExpressions;

using RedEye.Core;

namespace RedEye.Components {
    public class ExpressionParserComponent : IExpressionParser {
        ComponentManager manager = null;

        ILogger logger = null;
        IPluginManager pluginManager = null;

        Regex isSingleWord = new Regex(@"^\w+$", RegexOptions.Compiled);
        Regex variableRegex = new Regex(@"\$\{([\w.]+)\}", RegexOptions.Compiled);
        Regex expressionRegex = new Regex(@"^([\w.]+)\((?:(\w+\(.*?\)|(?<quote>[""'`]).*?(?<!\\)\<quote>|[^,\s]+)[\s,]*)*\)$", RegexOptions.Compiled);

        public void SetManager(ComponentManager manager){
            this.manager = manager;
        }

        public void Initialize(){
            logger = manager.GetComponent<ILogger>();
            pluginManager = manager.GetComponent<IPluginManager>();
        }

        public ExpressionParseResult ParseExpression(string expression, IVariableStorage<string> variables = null){
            ExpressionParseResult result = new();

            if(expression.Length > 0 && expression[0] =='~'){
                if(expression.Length == 1){
                    result.Value = expression;
                    return result;
                }

                result.Value = expression.Substring(1);
                return result;
            }

            if(!expression.Contains("(")){
                if(expression.Length > 2 && (expression[0] == '\'' || expression[0] == '"')){
                    result.Value = ParseVariables(expression.Substring(1, expression.Length - 2), variables);
                    return result;
                }

                result.Value = ParseVariables(expression, variables);
                return result;
            }

            var match = expressionRegex.Match(expression);

            if(match.Groups.Count > 1){
                var function = match.Groups[1].Value;
                var args = new List<string>();

                foreach(Capture capture in match.Groups[2].Captures){
                    args.Add(EvaluateExpression(capture.Value, variables));
                }

                result.FunctionName = function;
                result.Arguments = args;
            }

            return result;
        } 

        public string EvaluateExpression(string expression, IVariableStorage<string> variables = null){
            // System.Diagnostics.Debugger.Break();
            // System.Console.WriteLine(expression);
            if(expression.Length > 0 && expression[0] == '~'){
                return expression.Substring(1);
            }

            if(!expression.Contains("(")){
                return ParseVariables(expression, variables);
            }
            
            return new RwmlExpressionParser(pluginManager, variables ?? EmptyVariableStorage.EmptyStringStorage).Evaluate(expression);
        }

        public string ParseVariables(string value, IVariableStorage<string> variables){
            if(!value.Contains("$")) return value;
            return variableRegex.Replace(value, (match) => {
                return match.Groups.Count > 1 ? variables.GetVariable(match.Groups[1].Value) : string.Empty;
            });
        }
    }
}
