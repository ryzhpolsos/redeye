using System;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;

using RedEye.Core;
using static RedEye.Core.NativeHelper;

namespace RedEye.Components {
    public class MediaManagerComponent : IMediaManager {
        ComponentManager manager = null;

        public void SetManager(ComponentManager manager){
            this.manager = manager;
        }

        public void Initialize(){}

        public int GetBrightness(){
            using(ManagementObjectSearcher searcher = new(@"Root\wmi", "SELECT CurrentBrightness FROM WmiMonitorBrightness")){
                using(var results = searcher.Get()){
                    foreach(var result in results){
                        return ParseHelper.ParseInt(result["CurrentBrightness"].ToString());
                    }
                }
            }

            return -1;
        }

        public void SetBrightness(int level){
            using(ManagementObjectSearcher searcher = new(new ManagementScope(@"Root\wmi"), new SelectQuery("WmiMonitorBrightnessMethods"))){
                using(var results = searcher.Get()){
                    foreach(var result in results){
                        ((ManagementObject)result).InvokeMethod("WmiSetBrightness", new object[]{ 0, (byte)level });
                        return;
                    }

                    throw new Exception("Failed to set monitor brightness");
                }
            }
        }

        public int GetVolume(){
            return VolumeController.Volume;
        }

        public void SetVolume(int level){
            VolumeController.Volume = level;
        }

        public void IncreaseBrightness(int amount = 10){
            SetBrightness(GetBrightness() + amount);
        }

        public void DecreaseBrightness(int amount = 10){
            SetBrightness(GetBrightness() - amount);
        }

        public int GetBatteryLevel(){
            using(ManagementObjectSearcher searcher = new("SELECT EstimatedChargeRemaining FROM Win32_Battery")){
                using(var results = searcher.Get()){
                    foreach(var result in results){
                        return ParseHelper.ParseInt(result["EstimatedChargeRemaining"].ToString());
                    }
                }
            }

            return -1;
        }
    }

    [Guid("5CDF2C82-841E-4546-9722-0CF74078229A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IAudioEndpointVolume {
        int _0(); int _1(); int _2(); int _3();
        int SetMasterVolumeLevelScalar(float fLevel, Guid pguidEventContext);
        int _5();
        int GetMasterVolumeLevelScalar(out float pfLevel);
        int _7(); int _8(); int _9(); int _10(); int _11(); int _12();
    }

    [Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IMMDevice {
        int Activate(ref System.Guid id, int clsCtx, int activationParams, out IAudioEndpointVolume aev);
    }

    [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IMMDeviceEnumerator {
        int _0();
        int GetDefaultAudioEndpoint(int dataFlow, int role, out IMMDevice endpoint);
    }

    [ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")] class MMDeviceEnumeratorComObject { }

    internal static class VolumeController {
        static IAudioEndpointVolume mmVolume = null;

        static VolumeController(){
            var enumerator = (IMMDeviceEnumerator)(new MMDeviceEnumeratorComObject());
            enumerator.GetDefaultAudioEndpoint(0, 1, out IMMDevice dev);
            var aevGuid = typeof(IAudioEndpointVolume).GUID;
            dev.Activate(ref aevGuid, 1, 0, out mmVolume);
        }

        public static int Volume {
            get {
                mmVolume.GetMasterVolumeLevelScalar(out float level);
                return (int)(level * 100);
            }
            set {
                mmVolume.SetMasterVolumeLevelScalar((float)value / 100, default);
            }
        }
    }

}
