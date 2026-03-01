using System.Windows.Forms;
using System.Collections.Generic;

using RedEye.Core;

namespace RedEye.UI.BuiltInWidgets {
    public class TablePanel : BaseContainerWidget {
        TableLayoutPanel tableLayoutPanel = new();
        ILogger logger = null;
        List<IShellWidget> pendingWidgets = new();

        void ProcessWidget(IShellWidget widget){
            var control = widget.GetControl(false);
            var node = widget.GetNode();
            
            UtilHelper.IfNotEmpty(node.GetAttribute("table.position"), pos => {
                var spl = pos.Split(',');
                
                if(spl.Length != 2){
                    logger.LogError($"Invalid value for table.position parameter in widget#{widget.GetConfig().Id}: {pos}");
                    return;
                }

                int column = ParseHelper.ParseInt(spl[0]);
                int row = ParseHelper.ParseInt(spl[1]);
                tableLayoutPanel.SetCellPosition(control, new(column, row));
            });

            UtilHelper.IfNotEmpty(node.GetAttribute("table.rowSpan"), rowSpan => {
                tableLayoutPanel.SetRowSpan(control, ParseHelper.ParseInt(rowSpan));
            });

            UtilHelper.IfNotEmpty(node.GetAttribute("table.columnSpan"), columnSpan => {
                tableLayoutPanel.SetColumnSpan(control, ParseHelper.ParseInt(columnSpan));
            });

        }

        public override void Initialize(){
            Control = tableLayoutPanel;
            
            logger = ComponentManager.GetComponent<ILogger>(); 
            base.Initialize();
        }

        public override void PostInitialize(){
            tableLayoutPanel.SuspendLayout();

            UtilHelper.IfNotEmpty(Node.GetAttribute("rowCount"), rowCount => {
                var prevCount = tableLayoutPanel.RowCount * 1;
                tableLayoutPanel.RowCount = ParseHelper.ParseInt(rowCount);
            });

            UtilHelper.IfNotEmpty(Node.GetAttribute("columnCount"), columnCount => {
                tableLayoutPanel.ColumnCount = ParseHelper.ParseInt(columnCount);
            });

            for(int i = 0; i < tableLayoutPanel.RowCount; i++){
                tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / tableLayoutPanel.RowCount));
            }

            for(int i = 0; i < tableLayoutPanel.ColumnCount; i++){
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / tableLayoutPanel.ColumnCount));
            }

            foreach(var widget in pendingWidgets){
                base.AddWidgetInternal(widget);
                ProcessWidget(widget);
            }

            tableLayoutPanel.ResumeLayout();
        }

        protected override void AddWidgetInternal(IShellWidget widget){
            pendingWidgets.Add(widget);
        }
       
        protected override void UpdateControlInternal(){
            base.UpdateControlInternal();

            UtilHelper.IfNotEmpty(Node.GetAttribute("growStyle"), growStyle => {
                tableLayoutPanel.GrowStyle = ParseHelper.ParseEnum<TableLayoutPanelGrowStyle>(growStyle);
            });
        }
    }
}
