using System;
using System.Data;

namespace RedEye.Core {
    public static class EvalHelper {
        public static ILogger Logger = null;
        static DataTable dataTable = new();

        public static string Eval(string expression){
            try{
                var result = dataTable.Compute(expression, null).ToString();
                return result;
            }catch(Exception ex){
                Logger.LogFatal($"Error in expression \"{expression}\": {ex.Message}\n{ex.StackTrace}");
                return string.Empty;
            }
        }
    }
}
