using System.Runtime.InteropServices;

namespace ConsoleBoost.Helpers;

public static class NativeMethods
{
    // Power Schemes
    public static readonly Guid GUID_MAX_POWER_SAVINGS = new("a1841308-3541-4fab-bc81-f71556f20b4a");
    public static readonly Guid GUID_TYPICAL_POWER_SAVINGS = new("381b4222-f694-41f0-9685-ff5bb260df2e");
    public static readonly Guid GUID_MIN_POWER_SAVINGS = new("8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c");
    public static readonly Guid GUID_ULTIMATE_PERFORMANCE = new("e9a42b02-d5df-448d-aa00-03f14749eb61");

    [DllImport("powrprof.dll", EntryPoint = "PowerGetActiveScheme", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern uint PowerGetActiveScheme(IntPtr UserRootPowerKey, out IntPtr ActivePolicyGuid);

    [DllImport("powrprof.dll", EntryPoint = "PowerSetActiveScheme", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern uint PowerSetActiveScheme(IntPtr UserRootPowerKey, ref Guid SchemeGuid);

    [DllImport("powrprof.dll", EntryPoint = "PowerReadFriendlyName", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern uint PowerReadFriendlyName(
        IntPtr RootPowerKey,
        ref Guid SchemeGuid,
        IntPtr SubGroupOfPowerSettingsGuid,
        IntPtr PowerSettingGuid,
        IntPtr Buffer,
        ref uint BufferSize);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;

        public MEMORYSTATUSEX()
        {
            dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
        }
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetSystemTimes(out FILETIME lpIdleTime, out FILETIME lpKernelTime, out FILETIME lpUserTime);

    [StructLayout(LayoutKind.Sequential)]
    public struct FILETIME
    {
        public uint dwLowDateTime;
        public uint dwHighDateTime;

        public ulong ToTicks() => ((ulong)dwHighDateTime << 32) | dwLowDateTime;
    }
}
