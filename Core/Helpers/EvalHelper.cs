using System;
using System.Data;

namespace RedEye.Core {
    public static class EvalHelper {
        static DataTable dataTable = new();

        public static string Eval(string expression){
            try{
                var result = dataTable.Compute(expression, null).ToString();
                return result;
            }catch(EvaluateException ex){
                Console.WriteLine($"{expression} failed: {ex.Message}\n{ex.StackTrace}");
                return string.Empty;
            }
        }
    }
}