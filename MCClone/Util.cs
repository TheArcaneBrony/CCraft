using System;
using System.Runtime.InteropServices;

namespace MCClone
{
    internal class Util
    {
        public static double DegToRad(double deg)
        {
            return (Math.PI * deg) / 180;
        }
        public static string GetGameArg(string key)
        {
            int ArgIndex = Array.FindIndex(Program.args, (string s) =>
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
                return Program.args[ArgIndex + 1];
            }
            else
            {
                return "null";
            }
        }
        public static bool ShouldRenderChunk(Chunk ch)
        {
            if (ch == null) return false;
            if (!(MainWindow.world.Player.X / 16 + MainWindow.renderDistance > ch.X && MainWindow.world.Player.X / 16 - MainWindow.renderDistance < ch.X))
            {
                return false;
            }

            if (!(MainWindow.world.Player.Z / 16 + MainWindow.renderDistance > ch.Z && MainWindow.world.Player.Z / 16 - MainWindow.renderDistance < ch.Z))
            {
                return false;
            }

            if (-90 - 45 <= MainWindow.world.Player.LX && MainWindow.world.Player.LX <= 90 + 45 && ch.X >= MainWindow.world.Player.X / 16)
            {
                return true;
            }
            else if (MainWindow.world.Player.LX >= 90 - 45 && MainWindow.world.Player.LX >= -180 + 45 && ch.Z >= MainWindow.world.Player.Z / 16)
            {
                return true;
            }
            else if (MainWindow.world.Player.LX <= 180 - 45 && MainWindow.world.Player.LX <= -90 + 45 && ch.X <= MainWindow.world.Player.X / 16)
            {
                return true;
            }
            else if (MainWindow.world.Player.LX >= -90 && MainWindow.world.Player.LX <= 0 && ch.X >= MainWindow.world.Player.X / 16)
            {
                // return true;
            }
            return false;
        }
    }
    public class SystemUtils
    {
        [DllImport("ntdll.dll", EntryPoint = "NtSetTimerResolution")]
        public static extern void NtSetTimerResolution(uint DesiredResolution, bool SetResolution, ref uint CurrentResolution);
        const int SWP_NOZORDER = 0x4;
        const int SWP_NOACTIVATE = 0x10;
        [DllImport("kernel32")]
        public static extern IntPtr GetConsoleWindow();
        [DllImport("user32")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
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
