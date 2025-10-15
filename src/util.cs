using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace RedEye {
    public class Util {
        static JavaScriptSerializer jsSer = new JavaScriptSerializer();

        public static string ReplaceTemplate(string text, params string[] replacers){
            if(replacers.Length % 2 != 0) throw new ArgumentException();

            for(int i = 0; i < replacers.Length; i += 2){
                text = text.Replace("$("+replacers[i]+")", replacers[i+1]);
            }

            return text;
        }

        public static string GetPath(string pathData){
            return Path.IsPathRooted(pathData) ? pathData : Path.Combine(Config.AppDir, pathData);
        }

        public static string ReadFile(string relativePath, bool isCritical){
            try{
                return File.ReadAllText(GetPath(relativePath));
            }catch(Exception e){
                Logger.Log(isCritical ? Logger.MessageType.Critical : Logger.MessageType.Error, $"Failed to read \"{relativePath}\": {e.Message}");
                return null;
            }
        }

        public static string ReadFile(string relativePath){
            return ReadFile(relativePath, true);
        }

        public static T FromJson<T>(string json){
            return jsSer.Deserialize<T>(json);
        }

        public static string ToJson(object obj){
            return jsSer.Serialize(obj);
        }

        public static T[] ParseJsArray<T>(string jsonArray){
            return jsSer.Deserialize<T[]>(jsonArray);
        }

        public static object[] ParseJsArray(string jsonArray){
            return ParseJsArray<object>(jsonArray);
        }

        public static string InsertAfter(string s, string tf, string data){
            int index = s.IndexOf(tf) + tf.Length;
            return s.Insert(index, data);
        }

        public static string ToJsString(string str){
            return "\"" + str.Replace("\\", "\\\\").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\"", "\\\"") + "\"";
        }

        public static string GetFullTypeName(string shortTypeName){
            switch(shortTypeName){
                case "int": {
                    return "System.Int32";
                }
                case "long": {
                    return "System.Int64";
                }
                case "float": {
                    return "System.Single";
                }
                case "double": {
                    return "System.Double";
                }
                case "ptr":
                case "intptr": {
                    return "System.IntPtr";
                }
                case "char": {
                    return "System.Char";
                }
                case "string": {
                    return "System.String";
                }
                default: {
                    return null;
                }
            }
        }

        public static string ParseJsCode(string code){
            if(Config.CurrentConfig.core.parseJsSyntax.Contains("letconst")){
                code = Regex.Replace(code, @"(((?<!['""][^\r\n\)\};]*)(?<=[\s\)\};])let)|^let)\s([^\s]+)", new MatchEvaluator((m)=>{
                    return "var " + m.Groups[3].Value;
                }));

                code = Regex.Replace(code, @"(((?<!['""][^\r\n\)\};]*)(?<=[\s\)\};])const)|^const)\s([^\s]+)", new MatchEvaluator((m)=>{
                    return "var " + m.Groups[3].Value;
                }));
            }

            if(Config.CurrentConfig.core.parseJsSyntax.Contains("arrowfunc")){
                code = Regex.Replace(code, @"(?<!['""][^\r\n\)\};]*)\((.*)\)=>(?={)", new MatchEvaluator((m)=>{
                    return $"function({m.Groups[1]})";
                }));

                code = Regex.Replace(code, @"(?<!['""][^\r\n\)\};]*)([\w$]+)=>", new MatchEvaluator((m)=>{
                    return $"function({m.Groups[1]})";
                }));
            }

            return code;
        }
    }
}