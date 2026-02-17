 
namespace RedEye.Core {
    public enum LogType {
        Debug,
        Information,
        Warning,
        Error,
        Fatal
    }

    public interface ILogger : IComponent {
        public void Log(LogType logType, string message);
        public void LogDebug(string message);
        public void LogInformation(string message);
        public void LogWarning(string message);
        public void LogError(string message);
        public void LogFatal(string message);
    }
}