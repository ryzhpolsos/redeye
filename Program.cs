using System;
using System.Threading;
using System.Globalization;

namespace RedEye {
    public class Program {

        [STAThread]
        public static void Main(){
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            new Bootstrap().StartApplication();
        }
    }
}
