using System.Drawing;
using System.Windows.Forms;

using RedEye.Core;

namespace RedEye.UI.BuiltInWidgets {
    public class Image : BaseShellWidget {
        PictureBox pictureBox = new();

        public override void Initialize(){
            Control = pictureBox;

            base.Initialize();
        }

        protected override void UpdateControlInternal(){
            pictureBox.SizeMode = ParseHelper.ParseEnum<PictureBoxSizeMode>(Node.GetAttribute("sizeMode", "stretchImage"));

            var image = ComponentManager.GetComponent<IResourceManager>().GetResource<System.Drawing.Image>(Node.GetAttribute("src"));
            Size size = new();

            UtilHelper.IfNotEmpty(Node.GetAttribute("imageWidth"), imageWidth => {
                size.Width = ParseHelper.ParseInt(imageWidth);
            });
            
            UtilHelper.IfNotEmpty(Node.GetAttribute("imageHeight"), imageWidth => {
                size.Height = ParseHelper.ParseInt(imageWidth);
            });

            if(!size.IsEmpty) image = new Bitmap(image, size);
            pictureBox.Image = image; 

            base.UpdateControlInternal();
        }
    }
}
