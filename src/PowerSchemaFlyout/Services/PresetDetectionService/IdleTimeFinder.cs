using System;
using System.Runtime.InteropServices;


namespace PowerSchemaFlyout.Services
{
    internal struct Lastinputinfo
    {
        public uint CbSize;
        public uint DwTime;
    }

    /// <summary>
    /// Helps to find the idle time, (in milliseconds) spent since the last user input
    /// </summary>
    public class IdleTimeFinder
    {
        [DllImport("User32.dll")]
        private static extern bool GetLastInputInfo(ref Lastinputinfo plii);

        public static uint GetIdleTime()
        {
            Lastinputinfo lastInPut = new Lastinputinfo();
            lastInPut.CbSize = (uint)Marshal.SizeOf(lastInPut);
            GetLastInputInfo(ref lastInPut);

            return ((uint)Environment.TickCount - lastInPut.DwTime);
        }
    }
}
