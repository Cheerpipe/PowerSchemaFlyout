using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace PowerSchemaFlyout.Services.Native
{
    public class ForegroundWindowWatcher
    {
        // ReSharper disable once NotNullMemberIsNotInitialized
        private WinEventDelegate _winEventDelegate;

        private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        private const uint WineventOutofcontext = 0;
        private const uint EventSystemForeground = 3;

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        public static Process GetProcessByWindowHandler(IntPtr hWnd)
        {
            GetWindowThreadProcessId(hWnd, out var processId);
            Process fgProc = Process.GetProcessById(Convert.ToInt32(processId)); // Get it as a C# obj.
            // NOTE: In some rare cases ProcessID will be NULL. Handle this how you want. 
            return fgProc;
        }

        public void Start()
        {
            _winEventDelegate = WinEventProc;
            _mHhook = SetWinEventHook(EventSystemForeground, EventSystemForeground, IntPtr.Zero, _winEventDelegate, 0, 0, WineventOutofcontext);
        }

        private IntPtr _mHhook;

        public void Stop()
        {
            UnhookWinEvent(_mHhook);
            _winEventDelegate = null!;
        }

        public void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            ForegroundProcessChanged?.Invoke(this, new ProcessWatch(hwnd));
        }

        public event EventHandler<ProcessWatch>? ForegroundProcessChanged;

        public static string GetWindowTitle(IntPtr handle)
        {
            const int nChars = 256;
            StringBuilder buff = new StringBuilder(nChars);

            if (GetWindowText(handle, buff, nChars) > 0)
            {
                return buff.ToString().Trim();
            }
            return string.Empty;
        }
    }

    public class ProcessWatch
    {
        public Process? Process { get; set; }
        public IntPtr Handler { get; set; }
        public string Title { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }

        // ReSharper disable once NotNullMemberIsNotInitialized
        private ProcessWatch() { }

        public ProcessWatch(IntPtr hWnd)
        {
            try
            {
                Process process = ForegroundWindowWatcher.GetProcessByWindowHandler(hWnd);

                Process = process;
                Handler = hWnd;
                Title = ForegroundWindowWatcher.GetWindowTitle(hWnd);
                FilePath = process.MainModule!.FileName!.ToLower();
                FileName = System.IO.Path.GetFileName(process.MainModule!.FileName!.ToLower());
            }
            catch
            {
                Process process = ForegroundWindowWatcher.GetProcessByWindowHandler(hWnd);
                Process = process;
                Handler = IntPtr.Zero;
                Title = String.Empty;
                FilePath = String.Empty;
                FileName = String.Empty;
            }
        }

        public static ProcessWatch Empty
        {
            get
            {
                return new ProcessWatch
                {
                    Process = null,
                    Handler = IntPtr.Zero,
                    Title = String.Empty,
                    FileName = String.Empty,
                    FilePath = String.Empty
                };
            }
        }
    }
}
