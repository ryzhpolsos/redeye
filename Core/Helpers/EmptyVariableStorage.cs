using System.Collections.Generic;

namespace RedEye.Core {
    public class EmptyVariableStorage<T> : IVariableStorage<T> {
        public T GetVariable(string _) => default(T);
        public IEnumerable<string> GetVariables() => new string[0];
        public void SetVariable(string _, T __){}
    }

    public class EmptyVariableStorage : EmptyVariableStorage<object> {
        public static readonly EmptyVariableStorage EmptyObjectStorage = new EmptyVariableStorage();
        public static readonly EmptyVariableStorage<string> EmptyStringStorage = new EmptyVariableStorage<string>();
    }
}
