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
    struct SuggestionWrapper {
        public ISuggestion Suggestion;
        public Control Control;
    }

    class PowerSearchWindow : Form {
        TableLayoutPanel tlpMain = new TableLayoutPanel();
        TableLayoutPanel tlpSuggestions = new TableLayoutPanel();
        TextBox tbSearchBox = new TextBox();

        IEnumerable<IApplicationListEntry> applicationList = null; 

        List<SuggestionWrapper> suggestions = new List<SuggestionWrapper>();
        int selectedSuggestion = -1;

        public PowerSearchWindow(ComponentManager manager, Dictionary<string, string> config){
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
            
            Deactivate += (s, e) => {
                Close();
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
                                suggestions[selectedSuggestion].Suggestion.Invoke();
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

        void AddSuggestion(ISuggestion suggestion){
            try{
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

                pictureBox.Image = suggestion.GetIcon();
                label.Text = suggestion.GetText();

                suggestions.Add(new SuggestionWrapper(){
                    Suggestion = suggestion,
                    Control = tlp
                });

                tlpSuggestions.Controls.Add(tlp);
                tlpSuggestions.SetCellPosition(tlp, new TableLayoutPanelCellPosition(0, tlpSuggestions.RowCount++));

                tlpSuggestions.HorizontalScroll.Visible = false;
                tlpSuggestions.VerticalScroll.Visible = false;
            }catch(Exception){}
        }

        void ProcessSearch(string content){
            tlpMain.SuspendLayout();
            ClearSuggestions();

            foreach(var suggestion in SuggestionManager.GetSuggestions(content)){
                AddSuggestion(suggestion);
            }

            tlpMain.ResumeLayout();
        }

        protected override CreateParams CreateParams {
            get {
                var @params = base.CreateParams;
                @params.ExStyle |= (int)NativeHelper.WS_EX_TOOLWINDOW;
                return @params;
            }
        }
    }
}
