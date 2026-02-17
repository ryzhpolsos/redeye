using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Security.Cryptography;

using RedEye.Core;

namespace RedEye.UI.BuiltInWidgets {
    public class Image : BaseShellWidget {
        PictureBox pictureBox = new();
        byte[] lastHash = null;
        SHA1 sha1 = SHA1.Create();

        public override void Initialize(){
            Control = pictureBox;

            base.Initialize();
        }

        protected override void UpdateControlInternal(){
            pictureBox.SizeMode = ParseHelper.ParseEnum<PictureBoxSizeMode>(Node.GetAttribute("sizeMode", "stretchImage"));

            var image = ComponentManager.GetComponent<IResourceManager>().GetResource<System.Drawing.Image>(Node.GetAttribute("src"));
            byte[] hash = null;

            using(MemoryStream ms = new()){
                image.Save(ms, ImageFormat.Bmp);
                hash = sha1.ComputeHash(ms);
            }

            if(lastHash is null || !hash.SequenceEqual(lastHash)){
                lastHash = hash;
                pictureBox.Image = image;    
            }

            base.UpdateControlInternal();
        }
    }
}
