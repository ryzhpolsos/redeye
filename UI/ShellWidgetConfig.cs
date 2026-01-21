namespace RedEye.UI {
    public class ShellWidgetEvent {
        public int X = 0;
        public int Y = 0;
        public string Name = string.Empty;
        public string KeyName = string.Empty;
    }

    public class ShellWidgetConfig {
        public string Id = string.Empty;
        public int X = 0;
        public int Y = 0;
        public int Width = 0;
        public int Height = 0;
        public bool AutoSize = false;
        public string Dock = string.Empty;
        public string Color = string.Empty;
        public string BackgroundColor = string.Empty;
        public string Padding = string.Empty;
        public string Margin = string.Empty;
        public string Font = string.Empty;
        public int UpdateInterval = 0;
    }
}
