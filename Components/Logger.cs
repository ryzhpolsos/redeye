using System;
using System.IO;
using System.Windows.Forms;

using RedEye.Core;

namespace RedEye.Components {
    public class LoggerComponent : ILogger {
        ComponentManager manager;
        StreamWriter writer;

        public void SetManager(ComponentManager manager){
            this.manager = manager;
        }

        public void Initialize(){}

        public void Log(LogType type, string message){
            if(writer == null){
                writer = new StreamWriter(Path.Combine(manager.GetComponent<IConfig>().GetAppDirectory(), "log.txt"), true);
            }

            writer.WriteLine($"{DateTime.Now.ToString()} [{type.ToString().ToUpper()}] {message}");
            writer.Flush();

            if(type == LogType.Fatal){
                writer.Close();
                Environment.Exit(1);
            }
        }

        public void LogDebug(string message) => Log(LogType.Debug, message);
        public void LogInformation(string message) => Log(LogType.Information, message);
        public void LogWarning(string message) => Log(LogType.Warning, message);

        public void LogError(string message){
            MessageBox.Show($"Error: {message}", "RedEye", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Log(LogType.Error, message);
        }

        public void LogFatal(string message){
            MessageBox.Show($"Fatal Error: {message}", "RedEye", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Log(LogType.Fatal, message);
        }
    }
}