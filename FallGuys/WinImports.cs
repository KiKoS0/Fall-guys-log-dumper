using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace FallGuys
{
    public static class WinImports
    {
        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(String lpClassName, String lpWindowName);

        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("USER32.DLL")]
        public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("USER32.DLL")]
        public static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("User32.DLL")]
        public static extern bool AttachThreadInput(int CurrentForegroundThread, int MakeThisThreadForegrouond, bool boolAttach);

        [DllImport("User32.DLL")]
        public static extern int SetFocus(int hwnd);

        [DllImport("User32.DLL")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr lpdwProcessId);

        [DllImport("Kernel32.DLL")]
        public static extern int GetCurrentThreadID();

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("kernel32.dll")]
        public static extern int GetCurrentThreadId();

        [DllImport("User32")]
        public static extern int ShowWindow(IntPtr hwnd, int nCmdShow);

        [DllImport("user32.dll", EntryPoint = "keybd_event")]
        public static extern void keybd_event(
            byte bVk,
            byte bScan,
            int dwFlags,
            int dwExtraInfo
        );
    }
}
