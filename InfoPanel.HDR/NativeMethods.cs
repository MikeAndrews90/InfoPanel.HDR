using System.Runtime.InteropServices;

namespace InfoPanel.HDR
{
    internal static class NativeMethods
    {
        public const uint QDC_ONLY_ACTIVE_PATHS = 0x00000002;

        [DllImport("user32.dll")]
        public static extern int GetDisplayConfigBufferSizes(
            uint flags,
            ref uint numPathArrayElements,
            ref uint numModeInfoArrayElements);

        [DllImport("user32.dll")]
        public static extern int QueryDisplayConfig(
            uint flags,
            ref uint numPathArrayElements,
            [Out] DISPLAYCONFIG_PATH_INFO[] pathArray,
            ref uint numModeInfoArrayElements,
            [Out] DISPLAYCONFIG_MODE_INFO[] modeInfoArray,
            IntPtr currentTopologyId);

        [DllImport("user32.dll")]
        public static extern int DisplayConfigGetDeviceInfo(ref DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO requestPacket);

        [DllImport("user32.dll")]
        public static extern int DisplayConfigGetDeviceInfo(ref DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO_2 requestPacket);

        public enum DISPLAYCONFIG_DEVICE_INFO_TYPE : uint
        {
            DISPLAYCONFIG_DEVICE_INFO_GET_ADVANCED_COLOR_INFO = 9,
            DISPLAYCONFIG_DEVICE_INFO_GET_ADVANCED_COLOR_INFO_2 = 14,
        }

        /// <summary>
        /// Active color mode reported by DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO_2 (Windows 11 22H2+).
        /// </summary>
        public enum DISPLAYCONFIG_ADVANCED_COLOR_MODE : uint
        {
            SDR = 0,
            WCG = 1,
            HDR = 2,
            AdvancedSDR = 3,  // Auto HDR
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LUID
        {
            public uint LowPart;
            public int HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_PATH_SOURCE_INFO
        {
            public LUID adapterId;
            public uint id;
            public uint modeInfoIdx;
            public uint statusFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_PATH_TARGET_INFO
        {
            public LUID adapterId;
            public uint id;
            public uint modeInfoIdx;
            public uint outputTechnology;
            public uint rotation;
            public uint scaling;
            public DISPLAYCONFIG_RATIONAL refreshRate;
            public uint scanLineOrdering;
            [MarshalAs(UnmanagedType.Bool)]
            public bool targetAvailable;
            public uint statusFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_RATIONAL
        {
            public uint Numerator;
            public uint Denominator;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_PATH_INFO
        {
            public DISPLAYCONFIG_PATH_SOURCE_INFO sourceInfo;
            public DISPLAYCONFIG_PATH_TARGET_INFO targetInfo;
            public uint flags;
        }

        // The union in DISPLAYCONFIG_MODE_INFO is 48 bytes (DISPLAYCONFIG_TARGET_MODE is the largest member).
        // Total struct = 4 + 4 + 8 + 48 = 64 bytes.
        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_MODE_INFO
        {
            public uint infoType;
            public uint id;
            public LUID adapterId;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
            public byte[] modeInfoData;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_DEVICE_INFO_HEADER
        {
            public DISPLAYCONFIG_DEVICE_INFO_TYPE type;
            public uint size;
            public LUID adapterId;
            public uint id;
        }

        /// <summary>
        /// Legacy advanced color info (type 9). On Windows 11 22H2+, advancedColorEnabled may
        /// be true even when "Use HDR" is OFF (e.g. Auto HDR). Prefer type 14 when available.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO
        {
            public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
            private uint _value;

            public bool advancedColorSupported => (_value & 0x1) != 0;
            public bool advancedColorEnabled => (_value & 0x2) != 0;
            public bool wideColorEnforced => (_value & 0x4) != 0;
            public bool advancedColorForceDisabled => (_value & 0x8) != 0;

            public uint colorEncoding;
            public uint bitsPerColorChannel;
        }

        /// <summary>
        /// New advanced color info (type 14, Windows 11 22H2+). Provides an explicit activeColorMode
        /// that distinguishes SDR / WCG / HDR / Auto HDR, making HDR detection reliable.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO_2
        {
            public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
            private uint _value;

            public bool advancedColorSupported => (_value & 0x1) != 0;
            public bool advancedColorActive => (_value & 0x2) != 0;

            public DISPLAYCONFIG_ADVANCED_COLOR_MODE activeColorMode;
        }
    }
}
