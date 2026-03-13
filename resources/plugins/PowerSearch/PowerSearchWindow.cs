using System.Windows.Forms;

class PowerSearchWindow : Form {
    TableLayoutPanel tlpMain = new TableLayoutPanel();
    TextBox tbSearchBox = new TextBox(); 

    public PowerSearchWindow(){
        tlpMain.Dock = DockStyle.Fill;
        tlpMain.ColumnCount = 5;

        Controls.Add(tlpMain);
    }
}
