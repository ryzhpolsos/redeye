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
    enum SuggestionType {
        Application,
        Command,
        InternalCommand,
        Expression,
        Url
    }

    struct Suggestion {
        public SuggestionType Type;
        public TableLayoutPanel Control;
        public Action Invoke;
    }

    class PowerSearchWindow : Form {
        IExpressionParser expressionParser = null;

        TableLayoutPanel tlpMain = new TableLayoutPanel();
        TableLayoutPanel tlpSuggestions = new TableLayoutPanel();
        TextBox tbSearchBox = new TextBox();

        IEnumerable<IApplicationListEntry> applicationList = null; 
        List<string> pathDirs = new List<string>();
        string[] pathExt = null;

        List<Suggestion> suggestions = new List<Suggestion>();
        int selectedSuggestion = -1;

        Regex expressionRegex = new Regex(@"^[0-9+\-*/\(\)\s\.]+$", RegexOptions.Compiled);
        Regex urlRegex = new Regex(@"^\w+:", RegexOptions.Compiled);

        Bitmap cmdIcon = Icon.FromHandle(NativeHelper.GetIconFromLocation(Environment.ExpandEnvironmentVariables("%SYSTEMROOT%\\System32\\shell32.dll,-16767"))).ToBitmap();
        Bitmap calcIcon = Icon.FromHandle(NativeHelper.GetIconFromLocation(Environment.ExpandEnvironmentVariables("%SYSTEMROOT%\\System32\\calc.exe,0"))).ToBitmap();
        Bitmap urlIcon = Icon.FromHandle(NativeHelper.GetIconFromLocation(Environment.ExpandEnvironmentVariables("%SYSTEMROOT%\\System32\\inetcpl.cpl,-4460"))).ToBitmap();
        Bitmap intCmdIcon = Icon.FromHandle(NativeHelper.GetIconFromLocation(Environment.ExpandEnvironmentVariables("%SYSTEMROOT%\\System32\\imageres.dll,-5342"))).ToBitmap();

        public PowerSearchWindow(ComponentManager manager, Dictionary<string, string> config){
            expressionParser = manager.GetComponent<IExpressionParser>();
            applicationList = manager.GetComponent<ISpecialFolderWrapper>().GetApplicationList();

            pathDirs.AddRange(Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Process).Split(';'));
            pathDirs.AddRange(Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User).Split(';'));
            pathDirs.AddRange(Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Machine).Split(';'));
            pathDirs = pathDirs.Distinct().ToList();
            pathExt = Environment.GetEnvironmentVariable("PathExt").Split(';');

            HandleCreated += (s, e) => {
                manager.GetComponent<IShellEventListener>().AddIgnoredHandle(Handle);
            };

            Load += (s, e) => {
                var screenWidth = Screen.AllScreens.Select(x => x.Bounds.Width).Sum();
                var screenHeight = Screen.AllScreens.Select(x => x.Bounds.Height).Sum();
                Location = new Point(screenWidth / 2 - Width / 2, screenHeight / 2 - Height / 2);
                tlpSuggestions.MaximumSize = new Size(tbSearchBox.Width, screenHeight - Location.X);
            };

            Shown += (s, e) => {
                NativeHelper.SetWindowPos(Handle, NativeHelper.HWND_TOPMOST, 0, 0, 0, 0, NativeHelper.SWP_NOMOVE | NativeHelper.SWP_NOSIZE);
                NativeHelper.ForceSetForegroundWindow(Handle);            
            };

            FormClosing += (s, e) => {
                PowerSearchWindowState.Opened = false;
            };
            
            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            UpdateStyles();

            Font = ParseHelper.ParseFont(config["font"]);
            ForeColor = ColorTranslator.FromHtml(config["fontColor"]);
            BackColor = ColorTranslator.FromHtml(config["background"]);

            KeyPreview = true;
            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.None;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;

            tlpMain.AutoSize = true;
            tlpMain.RowCount = 2;
            tlpMain.ColumnCount = 1;
            tlpMain.AutoSize = true;
            tlpMain.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            tlpSuggestions.AutoSize = true;
            tlpSuggestions.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlpSuggestions.HorizontalScroll.Maximum = 0;
            tlpSuggestions.AutoScroll = true;

            tbSearchBox.Width = ParseHelper.ParseInt(config["width"]);
            tbSearchBox.Dock = DockStyle.Fill;
            tbSearchBox.AcceptsReturn = true;
            tbSearchBox.BackColor = BackColor;
            tbSearchBox.ForeColor = ForeColor;
            tbSearchBox.BorderStyle = BorderStyle.None;

            long lastTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            tbSearchBox.KeyPress += (s, e) => {
                if(e.KeyChar == (char)Keys.Enter || e.KeyChar == (char)Keys.Escape){
                    e.Handled = true;
                }
            };

            tbSearchBox.KeyUp += (s, e) => {
                switch(e.KeyCode){
                    case Keys.Escape: {
                        e.Handled = true;
                        e.SuppressKeyPress = true;

                        Close();
                        break;
                    }

                    case Keys.Enter: {
                        e.Handled = true;
                        e.SuppressKeyPress = true;

                        if(suggestions.Count > 0){
                            if(selectedSuggestion < 0) selectedSuggestion = 0;
                            
                            try{
                                suggestions[selectedSuggestion].Invoke();
                            }catch{}
                        }

                        Close();
                        break;
                    }

                    default: {
                        if(e.KeyCode == Keys.Up || e.KeyCode == Keys.Down) break;
                        
                        if(tbSearchBox.Text.Length > 0){
                            ProcessSearch(tbSearchBox.Text);
                        }else{
                            ClearSuggestions();
                        }

                        // long time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                        // if(lastTime + 300 <= time){
                        //     lastTime = time;

                        //     if(tbSearchBox.Text.Length > 0){
                        //         ProcessSearch(tbSearchBox.Text);
                        //     }else{
                        //         ClearSuggestions();
                        //     }
                        // }

                        break;
                    }

                }
            };

            tbSearchBox.KeyDown += (s, e) => {
                switch(e.KeyCode){
                    
                    case Keys.Up: {
                        e.Handled = true;
                        e.SuppressKeyPress = true;

                        tlpSuggestions.SuspendLayout();
                        if(selectedSuggestion >= 0 && selectedSuggestion < suggestions.Count) suggestions[selectedSuggestion].Control.BackColor = BackColor;

                        selectedSuggestion--;

                        if(selectedSuggestion < 0){
                            selectedSuggestion = suggestions.Count - 1;
                        }

                        suggestions[selectedSuggestion].Control.BackColor = SystemColors.Highlight;
                        tlpSuggestions.ScrollControlIntoView(suggestions[selectedSuggestion].Control);

                        tlpSuggestions.ResumeLayout();

                        break;
                    }

                    case Keys.Down: {
                        e.Handled = true;
                        e.SuppressKeyPress = true;

                        tlpSuggestions.SuspendLayout();
                        if(selectedSuggestion >= 0 && selectedSuggestion < suggestions.Count) suggestions[selectedSuggestion].Control.BackColor = BackColor;

                        selectedSuggestion++;

                        if(selectedSuggestion >= suggestions.Count){
                            selectedSuggestion = 0;
                        }

                        suggestions[selectedSuggestion].Control.BackColor = SystemColors.Highlight;
                        tlpSuggestions.ScrollControlIntoView(suggestions[selectedSuggestion].Control);

                        tlpSuggestions.ResumeLayout();
                        break;
                    }

                }
            };

            tlpMain.Controls.Add(tbSearchBox);
            tlpMain.SetColumnSpan(tbSearchBox, 5);
            tlpMain.SetCellPosition(tbSearchBox, new TableLayoutPanelCellPosition(0, 0));

            tlpMain.Controls.Add(tlpSuggestions);
            tlpMain.SetCellPosition(tlpSuggestions, new TableLayoutPanelCellPosition(0, 1));

            Controls.Add(tlpMain);
        }

        void ClearSuggestions(){
            selectedSuggestion = -1;

            foreach(var suggestion in suggestions.ToArray()){
                tlpSuggestions.Controls.Remove(suggestion.Control);
                tlpSuggestions.RowCount--;
                suggestions.Remove(suggestion);
            }
        }

        void AddSuggestion(SuggestionType type, object data){
            var tlp = new TableLayoutPanel();
            tlp.RowCount = 1;
            tlp.ColumnCount = 5;
            tlp.Width = tbSearchBox.Width;
            tlp.Height = (int)(tbSearchBox.Height * 2);

            var pictureBox = new PictureBox();
            pictureBox.Dock = DockStyle.Fill;
            pictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
            tlp.Controls.Add(pictureBox);
            tlp.SetCellPosition(pictureBox, new TableLayoutPanelCellPosition(0, 0));

            var label = new Label();
            label.Dock = DockStyle.Fill;
            label.TextAlign = ContentAlignment.MiddleLeft;
            tlp.Controls.Add(label);
            tlp.SetCellPosition(label, new TableLayoutPanelCellPosition(1, 0));
            tlp.SetColumnSpan(label, 4);

            var suggestion = new Suggestion(){
                Type = type,
                Control = tlp
            };

            switch(type){
                case SuggestionType.Application: {
                    var ale = (IApplicationListEntry)data;
                    pictureBox.Image = ale.GetIcon().ToBitmap();
                    label.Text = "Start application: " + ale.GetName();
                    suggestion.Invoke = () => { ale.Invoke(); };
                    break;
                }

                case SuggestionType.Command: {
                    var cmd = (string)data;
                    pictureBox.Image = cmdIcon;
                    label.Text = "Execute command: " + cmd;
                    
                    suggestion.Invoke = () => {
                        var splitted = cmd.Split(' ');
                        var psi = new ProcessStartInfo();
                        psi.FileName = splitted[0];
                        if(splitted.Length > 1) psi.Arguments = string.Join(" ", splitted.Skip(1));
                        Process.Start(psi);
                    };
                
                    break;
                }

                case SuggestionType.Expression: {
                    var expr = (string)data;
                    var result = "<error>";
                    
                    try{
                        result = EvalHelper.Eval(expr);
                    }catch(Exception){}

                    label.Text = expr + " = " + result + " (Enter to copy)"; 
                    pictureBox.Image = calcIcon;
                    suggestion.Invoke = () => { Clipboard.SetText(result); };

                    break;
                }

                case SuggestionType.Url: {
                    var url = (string)data;
                    label.Text = "Open URL: " + url;
                    pictureBox.Image = urlIcon;
                    suggestion.Invoke = () => { Process.Start(url); };
                
                    break;
                }

                case SuggestionType.InternalCommand: {
                    var cmd = (string)data;
                    label.Text = "Execute RedEye command: " + cmd;
                    pictureBox.Image = intCmdIcon;
                    suggestion.Invoke = () => { expressionParser.EvaluateExpression(cmd, EmptyVariableStorage.EmptyStringStorage); };
                    break;
                }
            }

            suggestions.Add(suggestion);
            tlpSuggestions.Controls.Add(tlp);
            tlpSuggestions.SetCellPosition(tlp, new TableLayoutPanelCellPosition(0, tlpSuggestions.RowCount++));

            tlpSuggestions.HorizontalScroll.Visible = false;
            tlpSuggestions.VerticalScroll.Visible = false;
        }

        void ProcessSearch(string content){
            tlpMain.SuspendLayout();

            ClearSuggestions();

            var lowerContent = content.ToLower();
            foreach(var app in applicationList.Where(x => x.GetName().ToLower().StartsWith(lowerContent))){
                AddSuggestion(SuggestionType.Application, app);
            }

            var splitted = content.Split(' ');
            var firstPart = splitted[0];

            if(firstPart.Contains('.')){
                if(pathDirs.Any(d => File.Exists(Path.Combine(d, firstPart)))){
                    AddSuggestion(SuggestionType.Command, content);
                }
            }else{
                foreach(var ext in pathExt){
                    if(pathDirs.Any(d => File.Exists(Path.Combine(d, firstPart + ext)))){
                        AddSuggestion(SuggestionType.Command, content);
                    }
                }
            }

            if(content.Contains(':')){
                var first = content.Split(':')[0];
                var regKey = Registry.ClassesRoot.OpenSubKey(first, false);

                if(regKey != null){
                    if(regKey.GetValue("URL Protocol") != null){
                        AddSuggestion(SuggestionType.Url, content);
                    }

                    regKey.Close();
                }
            }

            if(expressionRegex.IsMatch(content)){
                AddSuggestion(SuggestionType.Expression, content);
            }

            if(content.StartsWith("~")){
                AddSuggestion(SuggestionType.InternalCommand, content.Substring(1));
            }

            tlpMain.ResumeLayout();
        }

        protected override CreateParams CreateParams {
            get {
                var @params = base.CreateParams;
                @params.ExStyle |= NativeHelper.WS_EX_TOOLWINDOW;
                return @params;
            }
        }
    }
}
