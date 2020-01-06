using System;

namespace MCClone.Client
{
    class CliUtil
    {
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
            return true;
            if (ch == null || ch.Blocks.Count<256) return false;
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
        public static bool ShouldLoadChunk(int cx, int cz)
        {

            return true;
        }
    }
}
