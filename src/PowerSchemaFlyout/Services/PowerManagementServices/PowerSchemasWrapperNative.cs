using System;
using System.Runtime.InteropServices;

namespace PowerSchemaFlyout.Services
{
    public static class PowerSchemasWrapperNative
    {
        #region DLL imports

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr LocalFree(IntPtr hMem);

        [DllImport("powrprof.dll", EntryPoint = "PowerSetActiveScheme")]
        internal static extern uint PowerSetActiveScheme(IntPtr userPowerKey, ref Guid activePolicyGuid);

        [DllImport("powrprof.dll", EntryPoint = "PowerGetActiveScheme")]
        internal static extern uint PowerGetActiveScheme(IntPtr userPowerKey, out IntPtr activePolicyGuid);

        [DllImport("powrprof.dll", EntryPoint = "PowerReadFriendlyName")]
        internal static extern uint PowerReadFriendlyName(IntPtr rootPowerKey, ref Guid schemeGuid, IntPtr subGroupOfPowerSettingsGuid, IntPtr powerSettingGuid, IntPtr bufferPtr, ref uint bufferSize);

        [DllImport("PowrProf.dll")]
        internal static extern UInt32 PowerEnumerate(IntPtr rootPowerKey, IntPtr schemeGuid, IntPtr subGroupOfPowerSettingGuid, UInt32 acessFlags, UInt32 index, ref Guid buffer, ref UInt32 bufferSize);

        #endregion

        #region EnumerationEnums

        internal enum AccessFlags : uint
        {
            AccessScheme = 16
        }

        #endregion

        #region Constants

        internal const int ErrorNoMoreItems = 259;

        #endregion
    }
}
