﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using PowerSchemaFlyout.Services.Events;

namespace PowerSchemaFlyout.Services.Native
{
    public class ForegroundWindowWatcher
    {
        private WinEventDelegate winEventDelegate = null;

        private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        private const uint WINEVENT_OUTOFCONTEXT = 0;
        private const uint EVENT_SYSTEM_FOREGROUND = 3;

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        public static Process GetProcessByWindowHandler(IntPtr hWnd)
        {
            uint processID = 0;
            uint threadID = GetWindowThreadProcessId(hWnd, out processID); // Get PID from window handle
            Process fgProc = Process.GetProcessById(Convert.ToInt32(processID)); // Get it as a C# obj.
            // NOTE: In some rare cases ProcessID will be NULL. Handle this how you want. 
            return fgProc;
        }

        public void Start()
        {
            winEventDelegate = new WinEventDelegate(WinEventProc);
            m_hhook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, winEventDelegate, 0, 0, WINEVENT_OUTOFCONTEXT);
        }

        private IntPtr m_hhook;

        public void Stop()
        {
            UnhookWinEvent(m_hhook);
            winEventDelegate = null;
        }

        public void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            ForegroundProcessChanged?.Invoke(this,new ProcessWatch(hwnd));
        }

        public event EventHandler<ProcessWatch> ForegroundProcessChanged;

        public static string GetWindowTitle(IntPtr handle)
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString().Trim();
            }
            return string.Empty;
        }
    }

    public class ProcessWatch
    {
        public Process Process { get; set; }
        public IntPtr Handler { get; set; }
        public string Title { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }

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
    }
}
