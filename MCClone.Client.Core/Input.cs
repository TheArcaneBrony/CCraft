using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

using System;
using System.Diagnostics;

namespace MCClone
{
    internal class Input : MainWindow
    {
        public static void HandleInput(GameWindow window)
        {
            KeyboardState keyState = window.KeyboardState.GetSnapshot();
            MouseState mouseState = window.MouseState.GetSnapshot();
            if (focussed && !(mouseState.Position.X == centerX || mouseState.Position.Y == centerY))
            {
                double x = mouseState.Delta.X;
                double y = -(mouseState.Delta.Y);
                DataStore.Player.LY += y * sensitivity;
                DataStore.Player.LX += x * sensitivity;
                if (DataStore.Player.LX <= -180)
                {
                    DataStore.Player.LX = 179.99;
                }
                DataStore.Player.LX += 180;
                DataStore.Player.LX %= 360;
                DataStore.Player.LX -= 180;
                if (DataStore.Player.LY > 90)
                {
                    DataStore.Player.LY = 90;
                }
                if (DataStore.Player.LY < -90)
                {
                    DataStore.Player.LY = -90;
                }

                //Mouse.SetPosition(centerX, centerY);
            }
            if (keyState.IsAnyKeyDown)
            {
                if (keyState.IsKeyDown(Keys.F1))
                {
                    focussed = !focussed;
                    while (keyState.IsKeyDown(Keys.F1))
                    {
                    }
                }
                if (!focussed) return;
                if (keyState.IsKeyDown(Keys.Escape))
                {
                    running = false;
                    MainWindow.world.Save();
                    Process.GetCurrentProcess().Kill();
                }
                if (keyState.IsKeyDown(Keys.W))
                {
                    DataStore.Player.X += Math.Cos(Util.DegToRad(DataStore.Player.LX));
                    DataStore.Player.Z += Math.Sin(Util.DegToRad(DataStore.Player.LX));
                    if (keyState.IsKeyDown(Keys.LeftControl))
                    {
                        DataStore.Player.X += Math.Cos(Util.DegToRad(DataStore.Player.LX));
                        DataStore.Player.Z += Math.Sin(Util.DegToRad(DataStore.Player.LX));
                    }
                }
                if (keyState.IsKeyDown(Keys.A))
                {
                    DataStore.Player.X -= Math.Cos(Util.DegToRad(DataStore.Player.LX + 90));
                    DataStore.Player.Z -= Math.Sin(Util.DegToRad(DataStore.Player.LX + 90));
                }
                if (keyState.IsKeyDown(Keys.S))
                {
                    DataStore.Player.X -= Math.Cos(Util.DegToRad(DataStore.Player.LX));
                    DataStore.Player.Z -= Math.Sin(Util.DegToRad(DataStore.Player.LX));
                }
                if (keyState.IsKeyDown(Keys.D))
                {
                    DataStore.Player.X += Math.Cos(Util.DegToRad(DataStore.Player.LX + 90));
                    DataStore.Player.Z += Math.Sin(Util.DegToRad(DataStore.Player.LX + 90));
                }
                if (keyState.IsKeyDown(Keys.LeftShift))
                {
                    DataStore.Player.Y -= 0.1;
                }
                if (keyState.IsKeyDown(Keys.Space))
                {
                    if (DataStore.Player.Flying)
                    {
                        DataStore.Player.Y += 0.1;
                    }
                    else
                    {
                        DataStore.Player.YV = 0.1;
                    }
                }
                if (keyState.IsKeyDown(Keys.Q))
                {
                    brightness -= 0.01f;
                }
                if (keyState.IsKeyDown(Keys.E))
                {
                    brightness += 0.01f;
                }
                /*if (keyState.IsKeyDown(Keys.F))
                {
                    DataStore.Player.Flying = true;
                }
                if (keyState.IsKeyDown(Keys.F) && keyState.IsKeyDown(Keys.LControl))
                {
                    DataStore.Player.Flying = false;
                }*/
                if (keyState.IsKeyDown(Keys.Z))
                {
                    renderDistance--;
                }
                if (keyState.IsKeyDown(Keys.C))
                {
                    renderDistance++;
                }
            }

            //tick
            /*int ty = 5;
            (int X, int Y) cpos = ((int)(Math.Truncate(DataStore.Player.X / 16)), (int)(Math.Truncate(DataStore.Player.Z / 16)));
            if (world.Chunks.TryGetValue(cpos, out Chunk chunk))
            {
                chunk.Blocks.TryGetValue(((int)DataStore.Player.X % 16, (int)DataStore.Player.Y - 1, (int)DataStore.Player.Z % 16), out Block blockBelowPlayer);
                if (blockBelowPlayer != null)
                {
                    ty = blockBelowPlayer.Y;
                }
            }*/

            if (brightness > 1f)
            {
                brightness = 1f;
            }
            if (brightness < 0.1f)
            {
                brightness = 0.1f;
            }
            DataStore.Player.CPos = new Vector3((float)DataStore.Player.X, (float)DataStore.Player.Y + 1.7f, (float)DataStore.Player.Z);
            //DataStore.Player.CFPt = new Vector3((float)(DataStore.Player.X + Math.Cos(Util.DegToRad(DataStore.Player.LX))), (float)(DataStore.Player.Y + 1.7f + Math.Sin(Util.DegToRad(DataStore.Player.LY))), (float)(DataStore.Player.Z + Math.Sin(Util.DegToRad(DataStore.Player.LX))));
            float cyp = (float)(DataStore.Player.Y + 1.7f + Math.Sin(Util.DegToRad(DataStore.Player.LY)));
            DataStore.Player.CFPt = new Vector3((float)(DataStore.Player.X + Math.Cos(Util.DegToRad(DataStore.Player.LX)) * Math.Sin(Util.DegToRad(DataStore.Player.LY * 2))), cyp, (float)(DataStore.Player.Z + Math.Sin(Util.DegToRad(DataStore.Player.LX)) * Math.Sin(Util.DegToRad(DataStore.Player.LY * 2))));
        }
    }
}
