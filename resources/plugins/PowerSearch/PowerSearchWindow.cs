using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

using RedEye.Core;

enum SuggestType {
    Application,
    Command,
    InternalCommand
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
    List<Suggest> suggests = new List<Suggest>();
    int selectedSuggest = -1;

    public PowerSearchWindow(ComponentManager manager, Dictionary<string, string> config){
        applicationList = manager.GetComponent<ISpecialFolderWrapper>().GetApplicationList();

        HandleCreated += (s, e) => {
            manager.GetComponent<IShellEventListener>().AddIgnoredHandle(Handle);
        };

        Load += (s, e) => {
            var screenWidth = Screen.AllScreens.Select(x => x.Bounds.Width).Sum();
            var screenHeight = Screen.AllScreens.Select(x => x.Bounds.Height).Sum();
            Location = new Point(screenWidth / 2 - Width / 2, screenHeight / 2 - Height / 2);
            tlpSuggests.MaximumSize = new Size(tbSearchBox.Width, screenHeight - Location.X);
        };

        // Deactivate += (s, e) => {
        //     Close();
        // };

        Font = ParseHelper.ParseFont(config["font"]);

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
        // tlpSuggests.HorizontalScroll.Visible = false;
        // tlpSuggests.VerticalScroll.Visible = false;

        tbSearchBox.Width = ParseHelper.ParseInt(config["width"]);
        tbSearchBox.Dock = DockStyle.Fill;
        tbSearchBox.AcceptsReturn = true;

        long lastTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        tbSearchBox.KeyUp += (s, e) => {
            switch(e.KeyCode){
                case Keys.Escape: {
                    Close();
                    break;
                }

                case Keys.Return: {
                    if(suggests.Count > 0){
                        suggests[selectedSuggest].Invoke();
                    }

                    Close();
                    // RunCommand(content);
                    break;
                }

                default: {
                    if(e.KeyCode == Keys.Up || e.KeyCode == Keys.Down) break;

                    long time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                    if(lastTime + 300 <= time){
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

                    tlpSuggests.SuspendLayout();
                    if(selectedSuggest >= 0 && selectedSuggest < suggests.Count) suggests[selectedSuggest].Control.BackColor = SystemColors.Control;

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

                    tlpSuggests.SuspendLayout();
                    if(selectedSuggest >= 0 && selectedSuggest < suggests.Count) suggests[selectedSuggest].Control.BackColor = SystemColors.Control;

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
        tlp.Height = (int)(tbSearchBox.Height * 1.5);

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
