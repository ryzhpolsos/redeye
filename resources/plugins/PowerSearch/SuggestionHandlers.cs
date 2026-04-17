using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Microsoft.Win32;

using RedEye.Core;

namespace PowerSearch {
    static class SuggestionHandlers {
        public static ComponentManager ComponentManager = null;

        static IEnumerable<IApplicationListEntry> applicationList = null;

        static List<string> pathDirs = new List<string>();
        static string[] pathExt = null;

        static Regex expressionRegex = new Regex(@"^[0-9+\-*/\(\)\s\.]+$", RegexOptions.Compiled);

        static Bitmap cmdIcon = Icon.FromHandle(NativeHelper.GetIconFromLocation(Environment.ExpandEnvironmentVariables("%SYSTEMROOT%\\System32\\shell32.dll,-16767"))).ToBitmap();
        static Bitmap calcIcon = Icon.FromHandle(NativeHelper.GetIconFromLocation(Environment.ExpandEnvironmentVariables("%SYSTEMROOT%\\System32\\calc.exe,0"))).ToBitmap();
        static Bitmap urlIcon = Icon.FromHandle(NativeHelper.GetIconFromLocation(Environment.ExpandEnvironmentVariables("%SYSTEMROOT%\\System32\\inetcpl.cpl,-4460"))).ToBitmap();
        static Bitmap intCmdIcon = Icon.FromHandle(NativeHelper.GetIconFromLocation(Environment.ExpandEnvironmentVariables("%SYSTEMROOT%\\System32\\imageres.dll,-5342"))).ToBitmap();

        public static void Register(ComponentManager manager){
            ComponentManager = manager;

            SuggestionManager.RegisterHandler(ApplicationHandler);
            SuggestionManager.RegisterHandler(CommandHandler);
            SuggestionManager.RegisterHandler(ExpressionHandler);
            SuggestionManager.RegisterHandler(UrlHandler);
            SuggestionManager.RegisterHandler(InternalCommandHandler);
            
            applicationList = ComponentManager.GetComponent<ISpecialFolderWrapper>().GetApplicationList();

            pathDirs.AddRange(Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Process).Split(';'));
            pathDirs.AddRange(Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User).Split(';'));
            pathDirs.AddRange(Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Machine).Split(';'));
            pathDirs = pathDirs.Distinct().ToList();
            pathExt = Environment.GetEnvironmentVariable("PathExt").Split(';');
        }

        static IEnumerable<ISuggestion> ApplicationHandler(string input){
            var lowerInput = input.ToLower();

            foreach(var app in applicationList.Where(x => x.GetName().ToLower().StartsWith(lowerInput))){
                yield return new ApplicationSuggestion(app);
            }
        }

        static IEnumerable<ISuggestion> CommandHandler(string input){
            var splitted = input.Split(' ');
            var firstPart = splitted[0];

            if(firstPart.Contains('.')){
                if(pathDirs.Any(d => File.Exists(Path.Combine(d, firstPart)))){
                    yield return new CommandSuggestion(input, cmdIcon);
                }
            }else{
                foreach(var ext in pathExt){
                    if(pathDirs.Any(d => File.Exists(Path.Combine(d, firstPart + ext)))){
                        yield return new CommandSuggestion(input, cmdIcon);
                    }
                }
            }
        }

        static IEnumerable<ISuggestion> ExpressionHandler(string input){
            if(expressionRegex.IsMatch(input)){
                yield return new ExpressionSuggestion(input, calcIcon);
            }
        }

        static IEnumerable<ISuggestion> UrlHandler(string input){
            var splitted = input.Split(':');
            var firstPart = splitted[0];
            var regKey = Registry.ClassesRoot.OpenSubKey(firstPart);

            if(regKey != null){
                if(regKey.GetValue("URL Protocol") != null){
                    regKey.Close();
                    yield return new UrlSuggestion(input, urlIcon);
                }
            }
        }

        static IEnumerable<ISuggestion> InternalCommandHandler(string input){
            if(input[0] == '~'){
                yield return new InternalCommandSuggestion(input.Substring(1), intCmdIcon, ComponentManager.GetComponent<IExpressionParser>());
            }
        }
    }

    class ApplicationSuggestion : ISuggestion {
        IApplicationListEntry app;

        public ApplicationSuggestion(IApplicationListEntry app){
            this.app = app;
        }

        public string GetText(){
            return "Start application: " + app.GetName();
        }

        public Image GetIcon(){
            return app.GetIcon().ToBitmap();
        }

        public void Invoke(){
            app.Invoke();
        }
    }

    class CommandSuggestion : ISuggestion {
        string command;
        Image icon;

        public CommandSuggestion(string command, Image icon){
            this.command = command;
            this.icon = icon;
        }

        public string GetText(){
            return "Execute command: " + command;
        }

        public Image GetIcon(){
            return icon;
        }

        public void Invoke(){
            try{
                Process.Start("cmd.exe", "/c " + command);
            }catch(Exception){}
        }
    }

    class ExpressionSuggestion : ISuggestion {
        string expression;
        string result = string.Empty;
        Image icon;

        public ExpressionSuggestion(string expression, Image icon){
            this.expression = expression;
            this.icon = icon;
        }

        public string GetText(){
            result = "<error>";

            try{
                result = EvalHelper.Eval(expression);
            }catch(Exception){}

            return expression + " = " + result + " (Enter to copy)";
        }

        public Image GetIcon(){
            return icon;
        }

        public void Invoke(){
            Clipboard.SetText(result);
        }
    }

    class UrlSuggestion : ISuggestion {
        string url;
        Image icon;

        public UrlSuggestion(string url, Image icon){
            this.url = url;
            this.icon = icon;
        }

        public string GetText(){
            return "Open URL: " + url;
        }

        public Image GetIcon(){
            return icon;
        }

        public void Invoke(){
            try{
                Process.Start(url);
            }catch(Exception){}
        }
    }

    class InternalCommandSuggestion : ISuggestion {
        string command;
        Image icon;
        IExpressionParser expressionParser;

        public InternalCommandSuggestion(string command, Image icon, IExpressionParser expressionParser){
            this.command = command;
            this.icon = icon;
            this.expressionParser = expressionParser;
        }

        public string GetText(){
            return "Execute RedEye command: " + command;
        }

        public Image GetIcon(){
            return icon;
        }

        public void Invoke(){
            expressionParser.EvaluateExpression(command, EmptyVariableStorage.EmptyStringStorage);
        }
    }
}
