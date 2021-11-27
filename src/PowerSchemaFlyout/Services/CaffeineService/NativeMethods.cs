using System;
using System.Runtime.InteropServices;

namespace PowerSchemaFlyout.Services
{
    public static class NativeMethods
    {
        #region Native Methods
        public static void PreventSleep()
        {
            SetThreadExecutionState(ExecutionState.EsContinuous | ExecutionState.EsSystemRequired);
        }

        public static void AllowSleep()
        {
            SetThreadExecutionState(ExecutionState.EsContinuous);
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern ExecutionState SetThreadExecutionState(ExecutionState esFlags);

        [Flags]
        private enum ExecutionState : uint
        {
            EsContinuous = 0x80000000,
            EsSystemRequired = 0x00000001
        }
        #endregion
    }
}
