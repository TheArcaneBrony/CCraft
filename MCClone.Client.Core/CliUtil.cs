﻿using System;

namespace MCClone.Client
{
    class CliUtil
    {
        
        public static bool ShouldRenderChunk(Chunk ch)
        {
            return true;
            if (ch == null || ch.Blocks.Count < 1) return false;
            if (!(DataStore.Player.X / 16 + MainWindow.renderDistance > ch.X && DataStore.Player.X / 16 - MainWindow.renderDistance < ch.X))
            {
                return false;
            }

            if (!(DataStore.Player.Z / 16 + MainWindow.renderDistance > ch.Z && DataStore.Player.Z / 16 - MainWindow.renderDistance < ch.Z))
            {
                return false;
            }

            if (-90 - 45 <= DataStore.Player.LX && DataStore.Player.LX <= 90 + 45 && ch.X >= DataStore.Player.X / 16)
            {
                return true;
            }
            else if (DataStore.Player.LX >= 90 - 45 && DataStore.Player.LX >= -180 + 45 && ch.Z >= DataStore.Player.Z / 16)
            {
                return true;
            }
            else if (DataStore.Player.LX <= 180 - 45 && DataStore.Player.LX <= -90 + 45 && ch.X <= DataStore.Player.X / 16)
            {
                return true;
            }
            else if (DataStore.Player.LX >= -90 && DataStore.Player.LX <= 0 && ch.X >= DataStore.Player.X / 16)
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
