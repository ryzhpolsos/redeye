using System;
using System.Threading;
using System.Globalization;

// [assembly: ComVisible(false)]

namespace RedEye {
    public class Program {

        [STAThread]
        public static void Main(string[] args){
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            new Bootstrap().StartApplication(args);
        }
    }
}
