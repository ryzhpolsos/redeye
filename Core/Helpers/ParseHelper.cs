using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace RedEye.Core {
    public static class ParseHelper {
        static JavaScriptSerializer serializer = new();

        public static T ParseEnum<T>(string name) where T : struct, Enum {
            name = name.Trim();

            name = name[0].ToString().ToUpper() + name.Substring(1);
            return (T)Enum.Parse(typeof(T), name);
        }

        public static T ParseEnum<T>(string name, T defaultValue) where T : struct, Enum {
            if(name.Length == 0) return defaultValue;
            name = name[0].ToString().ToUpper() + name.Substring(1);

            if(Enum.TryParse<T>(name, out T result)){
                return result;
            }

            return defaultValue;
        }

        public static bool ParseBool(string value){
            value = value.Trim();
            return (value.Length > 0 && value.ToLower() != "false" && value != "0");
        }

        public static int ParseInt(string value){
            value = value.Trim();
            if(value.Length == 0) return 0;

            value = value.Split('.')[0];

            if(int.TryParse(value, out var result)){
                return result;
            }

            return 0;
        }

        public static double ParseDouble(string value){
            value = value.Trim();
            if(value.Length == 0) return 0;

            if(double.TryParse(value, out var result)){
                return result;
            }

            return 0;
        }

        public static T ParseJson<T>(string json){
            return serializer.Deserialize<T>(json);
        }

        public static string ToJson(object obj){
            return serializer.Serialize(obj);
        }

        public static Padding ParsePadding(string val){
            if(val.Contains(",")){
                var split = val.Split(',');
                if(split.Length != 4) throw new ArgumentException("Invalid padding value: " + val);
                return new Padding(ParseInt(split[0].Trim()), ParseInt(split[1].Trim()), ParseInt(split[2].Trim()), ParseInt(split[3].Trim()));
            }

            return new Padding(ParseInt(val));
        }

        public static Font ParseFont(string val){
            var split = val.Split(',');
            if(split.Length != 2) throw new ArgumentException("Invalid font value: " + val);

            return new Font(split[0], ParseInt(split[1]));
        }

        // public static string GetPath(string pathData){
        //     return Path.IsPathRooted(pathData) ? pathData : Path.Combine(Config.AppDirectory, pathData);
        // }

        // public static string ReadFile(string relativePath, bool isCritical){
        //     try{
        //         return File.ReadAllText(GetPath(relativePath));
        //     }catch(Exception e){
        //         Logger.Log(isCritical ? LogType.Fatal : LogType.Error, $"Failed to read \"{relativePath}\": {e.Message}");
        //         return null;
        //     }
        // }

        // public static string ReadFile(string relativePath){
        //     return ReadFile(relativePath, true);
        // }
        public static string[] ParseArguments(string args){
            List<string> arguments = new();

            var matches = Regex.Matches(args, @"(?:([\w.]+)|[""'`{(](.*?)[""'`})])(?=\s*,\s*|$)");
            foreach(Match match in matches){
                arguments.Add(match.Groups.Count > 1 && !string.IsNullOrEmpty(match.Groups[2].Value) ? match.Groups[2].Value : match.Groups[1].Value);
            }

            return arguments.ToArray();
        }

        public static string InsertAfter(string s, string tf, string data){
            int index = s.IndexOf(tf) + tf.Length;
            return s.Insert(index, data);
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
    }
}
