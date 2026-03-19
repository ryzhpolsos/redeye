using System.Collections.Generic;

namespace RedEye.Core {
    public interface IExpressionParser : IComponent {
        public ExpressionParseResult ParseExpression(string expression, IVariableStorage<string> variables = null);
        public string EvaluateExpression(string expression, IVariableStorage<string> variables = null);
    }

    public interface IVariableStorage<T> {
        public T GetVariable(string name);
        public IEnumerable<string> GetVariables();
        public void SetVariable(string name, T value);
    }

    public struct ExpressionParseResult {
        public ExpressionParseResult(){}
        public string Value = null;
        public string FunctionName = null;
        public IEnumerable<string> Arguments = null;
    }
}
