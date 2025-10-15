using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace RedEye {
    public class Logger {
        public enum MessageType {
            Critical,
            Error,
            Warning,
            Information
        }

        public delegate void LoggerCallback(MessageType type, string message);

        public static LoggerCallback Callback;

        public static StreamWriter Writer;

        public static void Log(MessageType type, string message){
            if(Callback != null){
                Callback(type, message);
            }

            if(Writer == null){
                Writer = new StreamWriter(Path.Combine(Config.AppDir, "log.txt"), true);
            }

            if(type == MessageType.Error || type == MessageType.Critical){
                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }else if(type == MessageType.Warning){
                MessageBox.Show(message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            Writer.WriteLine($"{DateTime.Now.ToString()} [{type.ToString().ToUpper()}] {message}");
            Writer.Flush();

            if(type == MessageType.Critical){
                Writer.Flush();
                Writer.Close();
                Environment.Exit(1);
            }
        }
    }
}
