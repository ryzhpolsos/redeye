using System;
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

        public string EvaluateExpression(string expression, IVariableStorage<string> variables = null){
            if(!expression.Contains("(")){
                if(expression.Length > 2 && (expression[0] == '\'' || expression[0] == '"')) return ParseVariables(expression.Substring(1, expression.Length - 2), variables);
                return ParseVariables(expression, variables);
            }

            var match = expressionRegex.Match(expression);

            if(match.Groups.Count > 1){
                var function = match.Groups[1].Value;
                var args = new List<string>();

                foreach(Capture capture in match.Groups[2].Captures){
                    args.Add(EvaluateExpression(capture.Value, variables));
                }

                var exportedFunctions = pluginManager.GetExportedFunctions();

                // Console.WriteLine(function);

                if(exportedFunctions.ContainsKey(function)){
                    // Console.WriteLine(string.Join(", ", args));
                    return exportedFunctions[function].Invoke(args).ToString();
                }

                logger.LogFatal($"No function with name {function} was found");
            }

            return string.Empty;
        }

        public string ParseVariables(string value, IVariableStorage<string> variables){
            return variableRegex.Replace(value, (match) => {
                return match.Groups.Count > 1 ? variables.GetVariable(match.Groups[1].Value) : string.Empty;
            });
        }
    }
}
