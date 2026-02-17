using System.Collections.Generic;

namespace RedEye.Core {
    public interface IExpressionParser : IComponent {
        public string EvaluateExpression(string expression, IVariableStorage<string> variables = null);
    }

    public interface IVariableStorage<T> {
        public T GetVariable(string name);
        public IEnumerable<string> GetVariables();
        public void SetVariable(string name, T value);
    }
}
