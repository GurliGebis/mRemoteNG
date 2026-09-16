using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

namespace mRemoteNGSpecs.Support
{
    /// <summary>
    /// Reads the top-level windows of another process, by class and title.
    ///
    /// UI Automation cannot answer this one. The windows that matter here belong to PuTTY, a
    /// separate process that mRemoteNG re-parents into a tab, and the question a scenario needs to
    /// settle is what Win32 sees — the same class and caption the product itself matches on when it
    /// decides which dialog it may click. Asking through a different API would be testing a
    /// different thing.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public static class Win32Windows
    {
        private const int BufferLength = 512;

        public static List<(string ClassName, string Title)> TopLevelWindowsOf(int processId) =>
            [.. WindowsOf(processId).Select(w => (w.ClassName, w.Title))];

        /// <summary>
        /// The same windows, with their handles. A PuTTY dialog is re-parented into an mRemoteNG
        /// tab, so it is not a child of the desktop root and UI Automation's usual entry point
        /// does not reach it. Going in by handle does.
        /// </summary>
        public static List<(IntPtr Handle, string ClassName, string Title)> WindowsOf(int processId)
        {
            List<(IntPtr, string, string)> windows = [];

            EnumWindows((handle, _) =>
            {
                _ = GetWindowThreadProcessId(handle, out uint owner);
                // ReSharper disable once RedundantCast
                if (owner != (uint)processId)
                    return true;

                StringBuilder className = new(BufferLength);
                _ = GetClassName(handle, className, className.Capacity);

                StringBuilder title = new(BufferLength);
                _ = GetWindowText(handle, title, title.Capacity);

                windows.Add((handle, className.ToString(), title.ToString()));
                return true;
            }, IntPtr.Zero);

            return windows;
        }

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool EnumWindows(EnumWindowsProc callback, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder className, int maxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder title, int maxCount);
    }
}
