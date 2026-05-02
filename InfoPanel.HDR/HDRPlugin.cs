using System.Runtime.InteropServices;
using InfoPanel.Plugins;

namespace InfoPanel.HDR
{
    public class HDRPlugin : BasePlugin
    {
        private readonly PluginText _hdrStatus = new("hdr-status", "HDR Status", "Unknown");
        private readonly PluginSensor _hdrEnabled = new("hdr-enabled", "HDR Enabled", 0f);

        public HDRPlugin() : base("hdr-plugin", "HDR Status", "Displays whether HDR is enabled on any connected display")
        {
        }

#pragma warning disable CS0672, CS0618
        public override string? ConfigFilePath => null;
#pragma warning restore CS0672, CS0618

        public override TimeSpan UpdateInterval => TimeSpan.FromSeconds(2);

        public override void Initialize() { }

        public override void Load(List<IPluginContainer> containers)
        {
            var container = new PluginContainer("hdr", "HDR");
            container.Entries.Add(_hdrStatus);
            container.Entries.Add(_hdrEnabled);
            containers.Add(container);
        }

        public override Task UpdateAsync(CancellationToken cancellationToken)
        {
            bool hdrOn = IsHdrEnabled();
            _hdrStatus.Value = hdrOn ? "On" : "Off";
            _hdrEnabled.Value = hdrOn ? 1f : 0f;
            return Task.CompletedTask;
        }

        public override void Update() => throw new NotImplementedException();

        public override void Close() { }

        /// <summary>
        /// Checks if HDR is enabled on any display. Uses the Windows 11 22H2+ type-14 API
        /// (DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO_2) which correctly distinguishes HDR from
        /// Auto HDR / WCG. Falls back to the legacy type-9 API on older Windows versions.
        /// </summary>
        private static bool IsHdrEnabled()
        {
            try
            {
                uint pathCount = 0, modeCount = 0;

                int result = NativeMethods.GetDisplayConfigBufferSizes(
                    NativeMethods.QDC_ONLY_ACTIVE_PATHS, ref pathCount, ref modeCount);

                if (result != 0)
                    return false;

                var paths = new NativeMethods.DISPLAYCONFIG_PATH_INFO[pathCount];
                var modes = new NativeMethods.DISPLAYCONFIG_MODE_INFO[modeCount];

                result = NativeMethods.QueryDisplayConfig(
                    NativeMethods.QDC_ONLY_ACTIVE_PATHS, ref pathCount, paths, ref modeCount, modes, IntPtr.Zero);

                if (result != 0)
                    return false;

                foreach (var path in paths)
                {
                    // Try the Windows 11 22H2+ API first — it has an explicit activeColorMode
                    // that distinguishes real HDR from Auto HDR / WCG.
                    var req2 = new NativeMethods.DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO_2
                    {
                        header = new NativeMethods.DISPLAYCONFIG_DEVICE_INFO_HEADER
                        {
                            type = NativeMethods.DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_ADVANCED_COLOR_INFO_2,
                            size = (uint)Marshal.SizeOf<NativeMethods.DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO_2>(),
                            adapterId = path.targetInfo.adapterId,
                            id = path.targetInfo.id
                        }
                    };

                    result = NativeMethods.DisplayConfigGetDeviceInfo(ref req2);
                    if (result == 0)
                    {
                        // HDR mode (not Auto HDR which is AdvancedSDR)
                        if (req2.activeColorMode == NativeMethods.DISPLAYCONFIG_ADVANCED_COLOR_MODE.HDR)
                            return true;
                        continue;
                    }

                    // Fallback: legacy type-9 API for Windows 10 / older Windows 11 builds.
                    // advancedColorEnabled is true for HDR; wideColorEnforced being true means
                    // it's only WCG (not true HDR), so exclude that case.
                    var req = new NativeMethods.DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO
                    {
                        header = new NativeMethods.DISPLAYCONFIG_DEVICE_INFO_HEADER
                        {
                            type = NativeMethods.DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_ADVANCED_COLOR_INFO,
                            size = (uint)Marshal.SizeOf<NativeMethods.DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO>(),
                            adapterId = path.targetInfo.adapterId,
                            id = path.targetInfo.id
                        }
                    };

                    result = NativeMethods.DisplayConfigGetDeviceInfo(ref req);
                    if (result == 0 && req.advancedColorEnabled && !req.wideColorEnforced)
                        return true;
                }
            }
            catch
            {
                // Silently return false if anything fails
            }

            return false;
        }
    }
}
