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

enum SuggestType {
    Application,
    Command,
    InternalCommand,
    Expression,
    Url
}

struct Suggest {
    public SuggestType Type;
    public TableLayoutPanel Control;
    public Action Invoke;
}

class PowerSearchWindow : Form {
    TableLayoutPanel tlpMain = new TableLayoutPanel();
    TableLayoutPanel tlpSuggests = new TableLayoutPanel();
    TextBox tbSearchBox = new TextBox();

    IEnumerable<IApplicationListEntry> applicationList = null; 
    List<string> pathDirs = new List<string>();
    string[] pathExt = null;

    List<Suggest> suggests = new List<Suggest>();
    int selectedSuggest = -1;

    Regex expressionRegex = new Regex(@"^[0-9+\-*/\(\)\s\.]+$", RegexOptions.Compiled);
    Regex urlRegex = new Regex(@"^\w+:", RegexOptions.Compiled);

    Bitmap cmdIcon = Icon.FromHandle(NativeHelper.GetIconFromLocation(Environment.ExpandEnvironmentVariables("%SYSTEMROOT%\\System32\\shell32.dll,-16767"))).ToBitmap();
    Bitmap calcIcon = Icon.FromHandle(NativeHelper.GetIconFromLocation(Environment.ExpandEnvironmentVariables("%SYSTEMROOT%\\System32\\calc.exe,0"))).ToBitmap();
    Bitmap urlIcon = Icon.FromHandle(NativeHelper.GetIconFromLocation(Environment.ExpandEnvironmentVariables("%SYSTEMROOT%\\System32\\inetcpl.cpl,-4460"))).ToBitmap();

    public PowerSearchWindow(ComponentManager manager, Dictionary<string, string> config){
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
            tlpSuggests.MaximumSize = new Size(tbSearchBox.Width, screenHeight - Location.X);
        };

        Shown += (s, e) => {
            NativeHelper.SetWindowPos(Handle, NativeHelper.HWND_TOPMOST, 0, 0, 0, 0, NativeHelper.SWP_NOMOVE | NativeHelper.SWP_NOSIZE);
            NativeHelper.ForceSetForegroundWindow(Handle);            
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

        tlpSuggests.AutoSize = true;
        tlpSuggests.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        tlpSuggests.HorizontalScroll.Maximum = 0;
        tlpSuggests.AutoScroll = true;

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

                    if(suggests.Count > 0){
                        if(selectedSuggest < 0) selectedSuggest = 0;
                        
                        try{
                            suggests[selectedSuggest].Invoke();
                        }catch{}
                    }

                    Close();
                    break;
                }

                default: {
                    if(e.KeyCode == Keys.Up || e.KeyCode == Keys.Down) break;

                    long time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                    if(true || lastTime + 300 <= time){
                        lastTime = time;

                        if(tbSearchBox.Text.Length > 0){
                            ProcessSearch(tbSearchBox.Text.ToLower());
                        }else{
                            ClearSuggests();
                        }
                    }

                    break;
                }

            }
        };

        tbSearchBox.KeyDown += (s, e) => {
            switch(e.KeyCode){
                
                case Keys.Up: {
                    e.Handled = true;
                    e.SuppressKeyPress = true;

                    tlpSuggests.SuspendLayout();
                    if(selectedSuggest >= 0 && selectedSuggest < suggests.Count) suggests[selectedSuggest].Control.BackColor = BackColor;

                    selectedSuggest--;

                    if(selectedSuggest < 0){
                        selectedSuggest = suggests.Count - 1;
                    }

                    suggests[selectedSuggest].Control.BackColor = SystemColors.Highlight;
                    tlpSuggests.ScrollControlIntoView(suggests[selectedSuggest].Control);

                    tlpSuggests.ResumeLayout();

                    break;
                }

                case Keys.Down: {
                    e.Handled = true;
                    e.SuppressKeyPress = true;

                    tlpSuggests.SuspendLayout();
                    if(selectedSuggest >= 0 && selectedSuggest < suggests.Count) suggests[selectedSuggest].Control.BackColor = BackColor;

                    selectedSuggest++;

                    if(selectedSuggest >= suggests.Count){
                        selectedSuggest = 0;
                    }

                    suggests[selectedSuggest].Control.BackColor = SystemColors.Highlight;
                    tlpSuggests.ScrollControlIntoView(suggests[selectedSuggest].Control);

                    tlpSuggests.ResumeLayout();
                    break;
                }

            }
        };

        tlpMain.Controls.Add(tbSearchBox);
        tlpMain.SetColumnSpan(tbSearchBox, 5);
        tlpMain.SetCellPosition(tbSearchBox, new TableLayoutPanelCellPosition(0, 0));

        tlpMain.Controls.Add(tlpSuggests);
        tlpMain.SetCellPosition(tlpSuggests, new TableLayoutPanelCellPosition(0, 1));

        Controls.Add(tlpMain);
    }

    void ClearSuggests(){
        selectedSuggest = -1;

        foreach(var suggest in suggests.ToArray()){
            tlpSuggests.Controls.Remove(suggest.Control);
            tlpSuggests.RowCount--;
            suggests.Remove(suggest);
        }
    }

    void AddSuggest(SuggestType type, object data){
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

        var suggest = new Suggest(){
            Type = type,
            Control = tlp
        };

        switch(type){
            case SuggestType.Application: {
                var ale = (IApplicationListEntry)data;
                pictureBox.Image = ale.GetIcon().ToBitmap();
                label.Text = "Start application: " + ale.GetName();
                suggest.Invoke = () => { ale.Invoke(); };
                break;
            }

            case SuggestType.Command: {
                var cmd = (string)data;
                pictureBox.Image = cmdIcon;
                label.Text = "Execute command: " + cmd;
                
                suggest.Invoke = () => {
                    var splitted = cmd.Split(' ');
                    var psi = new ProcessStartInfo();
                    psi.FileName = splitted[0];
                    if(splitted.Length > 1) psi.Arguments = string.Join(" ", splitted.Skip(1));
                    Process.Start(psi);
                };
            
                break;
            }

            case SuggestType.Expression: {
                var expr = (string)data;
                var result = "<error>";
                
                try{
                    result = EvalHelper.Eval(expr);
                }catch(Exception){}

                label.Text = expr + " = " + result + " (Enter to copy)"; 
                pictureBox.Image = calcIcon;
                suggest.Invoke = () => { Clipboard.SetText(result); };

                break;
            }

            case SuggestType.Url: {
                var url = (string)data;
                label.Text = "Open URL: " + url;
                pictureBox.Image = urlIcon;
                suggest.Invoke = () => { Process.Start(url); };
            
                break;
            }
        }

        suggests.Add(suggest);
        tlpSuggests.Controls.Add(tlp);
        tlpSuggests.SetCellPosition(tlp, new TableLayoutPanelCellPosition(0, tlpSuggests.RowCount++));

        tlpSuggests.HorizontalScroll.Visible = false;
        tlpSuggests.VerticalScroll.Visible = false;
    }

    void ProcessSearch(string content){
        tlpMain.SuspendLayout();

        ClearSuggests();

        foreach(var app in applicationList.Where(x => x.GetName().ToLower().StartsWith(content))){
            AddSuggest(SuggestType.Application, app);
        }

        var splitted = content.Split(' ');
        var firstPart = splitted[0];

        if(firstPart.Contains('.')){
            if(pathDirs.Any(d => File.Exists(Path.Combine(d, firstPart)))){
                AddSuggest(SuggestType.Command, content);
            }
        }else{
            foreach(var ext in pathExt){
                if(pathDirs.Any(d => File.Exists(Path.Combine(d, firstPart + ext)))){
                    AddSuggest(SuggestType.Command, content);
                }
            }
        }

        if(content.Contains(':')){
            var first = content.Split(':')[0];
            var regKey = Registry.ClassesRoot.OpenSubKey(first, false);

            if(regKey != null){
                if(regKey.GetValue("URL Protocol") != null){
                    AddSuggest(SuggestType.Url, content);
                }

                regKey.Close();
            }
        }

        if(expressionRegex.IsMatch(content)){
            AddSuggest(SuggestType.Expression, content);
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
