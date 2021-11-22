using System;
using System.Diagnostics;

namespace PowerSchemaFlyout.GameDetection.Events
{
    public class ForegroundProcessChangedEventArgs : EventArgs
    {
        public Process Process { get; set; }

        public ForegroundProcessChangedEventArgs(Process process)
        {
            Process = process;
        }

    }
}
