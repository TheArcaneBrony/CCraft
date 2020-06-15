using OpenTK;
using OpenTK.Input;
using System;
using System.Diagnostics;

namespace MCClone
{
    internal class Input : MainWindow
    {
        public static void HandleInput()
        {
            KeyboardState keyState = Keyboard.GetState();
            if (focussed && !(Mouse.GetCursorState().X == centerX || Mouse.GetCursorState().Y == centerY))
            {
                double x = Mouse.GetCursorState().X - centerX;
                double y = -(Mouse.GetCursorState().Y - centerY);
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
                Mouse.SetPosition(centerX, centerY);
            }
            if (keyState.IsAnyKeyDown)
            {
                if (keyState.IsKeyDown(Key.F1))
                {
                    focussed = !focussed;
                    while (Keyboard.GetState().IsKeyDown(Key.F1))
                    {
                    }
                }
                if (!focussed) return;
                if (keyState.IsKeyDown(Key.Escape))
                {
                    running = false;
                    MainWindow.world.Save();
                    Process.GetCurrentProcess().Kill();
                }
                if (keyState.IsKeyDown(Key.W))
                {
                    DataStore.Player.X += Math.Cos(Util.DegToRad(DataStore.Player.LX));
                    DataStore.Player.Z += Math.Sin(Util.DegToRad(DataStore.Player.LX));
                    if (keyState.IsKeyDown(Key.LControl))
                    {
                        DataStore.Player.X += Math.Cos(Util.DegToRad(DataStore.Player.LX));
                        DataStore.Player.Z += Math.Sin(Util.DegToRad(DataStore.Player.LX));
                    }
                }
                if (keyState.IsKeyDown(Key.A))
                {
                    DataStore.Player.X -= Math.Cos(Util.DegToRad(DataStore.Player.LX + 90));
                    DataStore.Player.Z -= Math.Sin(Util.DegToRad(DataStore.Player.LX + 90));
                }
                if (keyState.IsKeyDown(Key.S))
                {
                    DataStore.Player.X -= Math.Cos(Util.DegToRad(DataStore.Player.LX));
                    DataStore.Player.Z -= Math.Sin(Util.DegToRad(DataStore.Player.LX));
                }
                if (keyState.IsKeyDown(Key.D))
                {
                    DataStore.Player.X += Math.Cos(Util.DegToRad(DataStore.Player.LX + 90));
                    DataStore.Player.Z += Math.Sin(Util.DegToRad(DataStore.Player.LX + 90));
                }
                if (keyState.IsKeyDown(Key.ShiftLeft))
                {
                    DataStore.Player.Y -= 0.1;
                }
                if (keyState.IsKeyDown(Key.Space))
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
                if (keyState.IsKeyDown(Key.Q))
                {
                    brightness -= 0.01f;
                }
                if (keyState.IsKeyDown(Key.E))
                {
                    brightness += 0.01f;
                }
                /*if (keyState.IsKeyDown(Key.F))
                {
                    DataStore.Player.Flying = true;
                }
                if (keyState.IsKeyDown(Key.F) && keyState.IsKeyDown(Key.LControl))
                {
                    DataStore.Player.Flying = false;
                }*/
                if (keyState.IsKeyDown(Key.Z))
                {
                    renderDistance--;
                }
                if (keyState.IsKeyDown(Key.C))
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
            DataStore.Player.CFPt = new Vector3((float)(DataStore.Player.X + Math.Cos(Util.DegToRad(DataStore.Player.LX)) * Math.Sin(Util.DegToRad(DataStore.Player.LY*2))), cyp , (float)(DataStore.Player.Z + Math.Sin(Util.DegToRad(DataStore.Player.LX))* Math.Sin(Util.DegToRad(DataStore.Player.LY*2))));
        }
    }
}