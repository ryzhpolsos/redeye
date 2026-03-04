using System;
using System.Windows.Forms;

using RedEye.Core;

namespace RedEye.UI.BuiltInWidgets {
    public class FlowPanel : BaseContainerWidget {
        FlowLayoutPanel flowLayoutPanel = new();
        ILogger logger = null;

//         public FlowPanel() : base() {
//             flowLayoutPanel = new();
//         }

        public FlowPanel() : base(){
            Control = flowLayoutPanel;
        }

        public override void Initialize(){
            logger = ComponentManager.GetComponent<ILogger>();
            base.Initialize();
        }

        protected override void UpdateControlInternal(){
            flowLayoutPanel.WrapContents = ParseHelper.ParseBool(Node.GetAttribute("wrap", "false"));
            try{
                flowLayoutPanel.FlowDirection = ParseHelper.ParseEnum<FlowDirection>(Node.GetAttribute("direction", "leftToRight"));
            }catch(Exception ex){
                if(logger is not null) logger.LogDebug(ExceptionHelper.FormatException(ex));
            }

            base.UpdateControlInternal();
        }
    }
}
