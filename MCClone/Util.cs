//using MCClone.TerrainGenTest;
using System;
using System.Runtime.InteropServices;

namespace MCClone
{
    internal class Util
    {
        public static string[] args;
        public static double DegToRad(double deg)
        {
            return (Math.PI * deg) / 180;
        }
        public static (int, int) PlayerPosToChunkPos((double, double) pos)
        {
            return ((int)(Math.Truncate(pos.Item1 / 16)), (int)(Math.Truncate(pos.Item2 / 16)));
        }
        public static int TruncateHeight(int y)
        {
            return Math.Min(Math.Max(0, y), 255);
        }
        public static string GetGameArg(string key)
        {
            int ArgIndex = Array.FindIndex(args, (string s) =>
            {
                if (s == $"-{key}")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            });
            if (ArgIndex != -1)
            {
                DataStore.GameArgs.Add(key, args[ArgIndex + 1]);
                return args[ArgIndex + 1];
            }
            else
            {
                if (DataStore.GameArgs.ContainsKey(key)) return DataStore.GameArgs[key];
                return "null";
            }
        }
    }
    public static class SystemUtils
    {
        const int SWP_NOZORDER = 0x4;
        const int SWP_NOACTIVATE = 0x10;
        [DllImport("kernel32")]
        private static extern IntPtr GetConsoleWindow();
        [DllImport("user32")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int x, int y, int cx, int cy, int flags);
        public static void SetWindowPosition(int x, int y, int width, int height)
        {
            SetWindowPos(Handle, IntPtr.Zero, x, y, width, height, SWP_NOZORDER | SWP_NOACTIVATE);
        }
        public static IntPtr Handle
        {
            get
            {
                return GetConsoleWindow();
            }
        }
    }
}
