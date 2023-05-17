using System.Runtime.InteropServices;

namespace MouseJiggler;

class Program
{
    // Import the required functions from user32.dll
    [DllImport("user32.dll")]
    static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    static extern bool SetCursorPos(int x, int y);

    const int MOUSEEVENTF_MOVE = 0x0001;

    static void Main()
    {
        Console.WriteLine("Mouse Jiggler");
        Console.WriteLine("Press any key to exit...");
        var skipCiclesCount = 10;
        var skippedCyclesCount = 0;
        var deltaX = 1;
        while (!Console.KeyAvailable)
        {
            // Sleep for a short duration to simulate mouse movement
            Thread.Sleep(600);
            if (skippedCyclesCount < skipCiclesCount)
            {
                skippedCyclesCount++;
                continue;
            }

            skippedCyclesCount = 0;
            // Move the mouse cursor by 1 pixel horizontally and vertically on each monitor
            foreach (var monitor in MonitorHelper.GetMonitors())
            {
                MoveMouseOnMonitor(monitor, deltaX, 0);
                deltaX = -deltaX;
            }
        }
    }

    static void MoveMouseOnMonitor(MonitorInfo monitor, int deltaX, int deltaY)
    {
        // Get the current mouse position
        POINT currentPosition;
        GetCursorPos(out currentPosition);

        // Check if the mouse is within the bounds of the current monitor
        if (currentPosition.X >= monitor.Left && currentPosition.X <= monitor.Right &&
            currentPosition.Y >= monitor.Top && currentPosition.Y <= monitor.Bottom)
        {
            // Calculate the new mouse position
            int newX = currentPosition.X + deltaX;
            int newY = currentPosition.Y + deltaY;

            // Check if the new position exceeds the monitor bounds
            if (newX < monitor.Left) newX = monitor.Left;
            if (newX > monitor.Right) newX = monitor.Right;
            if (newY < monitor.Top) newY = monitor.Top;
            if (newY > monitor.Bottom) newY = monitor.Bottom;

            // Move the mouse to the new position
            SetCursorPos(newX, newY);
        }
    }

    // Structure representing a point
    struct POINT
    {
        public int X;
        public int Y;
    }

    // Structure representing monitor information
    struct MonitorInfo
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    // Helper class to retrieve monitor information
    static class MonitorHelper
    {
        [DllImport("user32.dll")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData);

        private delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public static MonitorInfo[] GetMonitors()
        {
            var monitors = new System.Collections.Generic.List<MonitorInfo>();
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData) =>
            {
                var info = new MonitorInfo
                {
                    Left = lprcMonitor.Left,
                    Top = lprcMonitor.Top,
                    Right = lprcMonitor.Right,
                    Bottom = lprcMonitor.Bottom
                };
                monitors.Add(info);
                return true;
            }, IntPtr.Zero);

            return monitors.ToArray();
        }
    }
}