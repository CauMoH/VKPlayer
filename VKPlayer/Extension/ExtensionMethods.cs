using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Application = System.Windows.Application;

namespace VKPlayer.Extension
{
    public static class ExtensionMethods
    {
        const string USER32 = "user32.dll";

        [DllImport(USER32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [DllImport(USER32)]
        public static extern int FlashWindow(IntPtr hwnd, bool bInvert);

        //Flash both the window caption and taskbar button.
        //This is equivalent to setting the FLASHW_CAPTION | FLASHW_TRAY flags. 
        private const uint FLASHW_TRAY = 2;

        // Flash continuously until the window comes to the foreground. 
        private const uint FLASHW_TIMERNOFG = 12;

        [StructLayout(LayoutKind.Sequential)]
        public struct FLASHWINFO
        {
            public uint cbSize;
            public IntPtr hwnd;
            public uint dwFlags;
            public uint uCount;
            public uint dwTimeout;
        }

        public static void Flash()
        {
            var hWnd = new WindowInteropHelper(Application.Current.MainWindow ?? throw new InvalidOperationException()).Handle;
            var fInfo = new FLASHWINFO();

            fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));
            fInfo.hwnd = hWnd;
            fInfo.dwFlags = FLASHW_TRAY | FLASHW_TIMERNOFG;
            fInfo.uCount = uint.MaxValue;
            fInfo.dwTimeout = 0;

            FlashWindow(hWnd, true);
        }
    }
}
