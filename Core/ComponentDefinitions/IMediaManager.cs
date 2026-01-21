namespace RedEye.Core {
    public interface IMediaManager : IComponent {
        public int GetBrightness();
        public void SetBrightness(int level);
        public void IncreaseBrightness(int amount = 10);
        public void DecreaseBrightness(int amount = 10);
        public int GetVolume();
        public void SetVolume(int level);
        public int GetBatteryLevel();
    }
}
