using System;

namespace RedEye.Core {
    public static class UtilHelper {
        public static void IfNotEmpty(string str, Action action){
            if(!string.IsNullOrEmpty(str)) action.Invoke();
        }

        public static T IfNotEmpty<T>(string str, Func<T> func){
            if(!string.IsNullOrEmpty(str)) return func.Invoke();
            return default;
        }

        public static void IfNotEmpty(string str, Action<string> action){
            if(!string.IsNullOrEmpty(str)) action.Invoke(str);
        }

        public static T IfNotEmpty<T>(string str, Func<string, T> func){
            if(!string.IsNullOrEmpty(str)) return func.Invoke(str);
            return default;
        }

        public static string ToFirstLower(string str){
            return str[0].ToString().ToLower() + str.Substring(1);
        }

        public static string ToFirstUpper(string str){
            return str[0].ToString().ToUpper() + str.Substring(1);
        }
    }
}