using NAudio.Wave;
using OpenTK.Graphics.ES20;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MCClone
{
    class Util
    {
        public static double DegToRad(double deg) => (Math.PI * deg) / 180;
        public static string GetGameArg(string key)
        {
            int test = Array.FindIndex(Program.args, (String s) => { if (s == $"-{key}") return true; else return false; });
            if (test != -1)
                return Program.args[test + 1];
            else return "null";
        }
        public static bool ShouldRenderChunk(Chunk ch)
        {
            if (!(MainWindow.world.Player.X / 16 + MainWindow.renderDistance > ch.X && MainWindow.world.Player.X / 16 - MainWindow.renderDistance < ch.X)) return false;
            if (!(MainWindow.world.Player.Z / 16 + MainWindow.renderDistance > ch.Z && MainWindow.world.Player.Z / 16 - MainWindow.renderDistance < ch.Z)) return false;
            if (MainWindow.world.Player.LX >= -90 - 45 && MainWindow.world.Player.LX <= 90 + 45 && ch.X >= MainWindow.world.Player.X / 16)
            {
                return true;
            }
            else if (MainWindow.world.Player.LX >= 90 - 45 && MainWindow.world.Player.LX >= -180 + 45 && ch.Z >= MainWindow.world.Player.Z / 16)
            {
                return true;
            }
            else if (MainWindow.world.Player.LX >= -180 && MainWindow.world.Player.LX <= -90 && ch.X <= MainWindow.world.Player.X / 16)
            {
                return true;
            }
            else if (MainWindow.world.Player.LX >= -90 && MainWindow.world.Player.LX <= 0 && ch.X >= MainWindow.world.Player.X / 16)
            {
                return true;
            }

            return false;
        }

    }
    public class SystemUtils
    {
        [DllImport("ntdll.dll", EntryPoint = "NtSetTimerResolution")]
        public static extern void NtSetTimerResolution(uint DesiredResolution, bool SetResolution, ref uint CurrentResolution);
    }
}
