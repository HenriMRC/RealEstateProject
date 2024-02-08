using System;
using System.Runtime.InteropServices;

namespace RealEstateProject;

internal static class ConsoleUtils
{
    [DllImport("kernel32.dll")]
    public static extern bool AllocConsole();

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetConsoleWindow();

    [DllImport("kernel32.dll")]
    public static extern bool FreeConsole();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowInfo(IntPtr hwnd, ref WINDOWINFO pwi);


    [DllImport("user32.dll")]
    private static extern bool DeleteMenu(IntPtr hMenu, uint uPosition, uint uFlags);

    [DllImport("user32.dll")]
    private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
    
    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    const int GWL_STYLE = -16;
    const uint WS_SIZEBOX = 0x00040000;
    const uint MF_BYCOMMAND = 0x00000000;
    const uint SC_SIZE = 0xF000;

    /// <summary>
    /// Function to disable resizing of the console window
    /// </summary>
    internal static void DisableResizing()
    {
        IntPtr consoleHandle = GetConsoleWindow();
        if (consoleHandle != IntPtr.Zero)
        {
            WINDOWINFO info = new();
            info.cbSize = (uint)Marshal.SizeOf(info);
            GetWindowInfo(consoleHandle, ref info);
            uint style = info.dwStyle & ~WS_SIZEBOX;
            SetWindowLong(consoleHandle, GWL_STYLE, style);

            IntPtr sysMenuHandle = GetSystemMenu(consoleHandle, false);
            if (sysMenuHandle != IntPtr.Zero)
            {
                DeleteMenu(sysMenuHandle, SC_SIZE, MF_BYCOMMAND);
            }
        }

    }
    
    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WINDOWINFO
    {
        public uint cbSize;
        public RECT rcWindow;
        public RECT rcClient;
        public uint dwStyle;
        public uint dwExStyle;
        public uint dwWindowStatus;
        public uint cxWindowBorders;
        public uint cyWindowBorders;
        public ushort atomWindowType;
        public ushort wCreatorVersion;

        public WINDOWINFO(bool? filler) : this()
        {
            cbSize = (uint)Marshal.SizeOf(typeof(WINDOWINFO));
        }
    }
}
