using RedEye.UI;

namespace RedEye.Core {
    public enum ShellWindowType {
        Normal,
        TopMost,
        Background
    }

    public enum ShellWindowBorderType {
        None,
        Normal,
        FixedDialog,
        FixedSingle,
        FixedToolWindow,
        SizableToolWindow
    }

    public class ShellWindowConfig {
        public string Id = string.Empty;
        public ShellWindowType Type = ShellWindowType.Normal;
        public ShellWindowBorderType BorderType = ShellWindowBorderType.Normal;
        public bool IsTransparent = false;
        public string Title = string.Empty;
        public int X = 0;
        public int Y = 0;
        public int Width = 0;
        public int Height = 0;
        public bool AutoShow = true;
        public bool MinimizeButton = true;
        public bool MaximizeButton = true;
        public bool AllowClose = true;
        public bool AutoSize = false;
        public string Color = null;
        public string BackgroundColor = null;
        public string Padding = null;
    }

    public interface IShellWindow : IComponent, IWidgetContainer {
        public void InitWindow();
        public void ShowWindow();
        public void HideWindow();
        public void ToggleWindow();
        public string GetTitle();
        public void SetTitle(string newTitle);
        public ShellWindowConfig GetConfig();
        public void SetConfig(ShellWindowConfig newConfig);
    }
}
