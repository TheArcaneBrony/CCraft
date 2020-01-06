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
                world.Player.LY += y * sensitivity;
                world.Player.LX += x * sensitivity;
                if (world.Player.LX <= -180)
                {
                    world.Player.LX = 179.99;
                }
                world.Player.LX += 180;
                world.Player.LX %= 360;
                world.Player.LX -= 180;
                if (world.Player.LY > 90)
                {
                    world.Player.LY = 90;
                }
                if (world.Player.LY < -90)
                {
                    world.Player.LY = -90;
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
                    Process.GetCurrentProcess().Kill();
                }
                if (keyState.IsKeyDown(Key.W))
                {
                    world.Player.X += Math.Cos(Util.DegToRad(world.Player.LX));
                    world.Player.Z += Math.Sin(Util.DegToRad(world.Player.LX));
                    if (keyState.IsKeyDown(Key.LControl))
                    {
                        world.Player.X += Math.Cos(Util.DegToRad(world.Player.LX));
                        world.Player.Z += Math.Sin(Util.DegToRad(world.Player.LX));
                    }
                }
                if (keyState.IsKeyDown(Key.A))
                {
                    world.Player.X -= Math.Cos(Util.DegToRad(world.Player.LX + 90));
                    world.Player.Z -= Math.Sin(Util.DegToRad(world.Player.LX + 90));
                }
                if (keyState.IsKeyDown(Key.S))
                {
                    world.Player.X -= Math.Cos(Util.DegToRad(world.Player.LX));
                    world.Player.Z -= Math.Sin(Util.DegToRad(world.Player.LX));
                }
                if (keyState.IsKeyDown(Key.D))
                {
                    world.Player.X += Math.Cos(Util.DegToRad(world.Player.LX + 90));
                    world.Player.Z += Math.Sin(Util.DegToRad(world.Player.LX + 90));
                }
                if (keyState.IsKeyDown(Key.ShiftLeft))
                {
                    world.Player.Y -= 0.1;
                }
                if (keyState.IsKeyDown(Key.Space))
                {
                    if (world.Player.Flying)
                    {
                        world.Player.Y += 0.1;
                    }
                    else
                    {
                        world.Player.YV = 0.1;
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
                if (keyState.IsKeyDown(Key.F))
                {
                    world.Player.Flying = true;
                }
                if (keyState.IsKeyDown(Key.F) && keyState.IsKeyDown(Key.LControl))
                {
                    world.Player.Flying = false;
                }
                if (keyState.IsKeyDown(Key.Z))
                {
                    renderDistance--;
                }
                if (keyState.IsKeyDown(Key.C))
                {
                    renderDistance++;
                }
            }
        }
        public static void Tick()
        {
            int ty = 5;
            (int X, int Y) cpos = ((int)(Math.Truncate(world.Player.X / 16)), (int)(Math.Truncate(world.Player.Z / 16)));
            if (world.Chunks.TryGetValue(cpos, out Chunk chunk))
            {
                chunk.Blocks.TryGetValue(((int)world.Player.X % 16, (int)world.Player.Y - 1, (int)world.Player.Z % 16), out Block blockBelowPlayer);
                if (blockBelowPlayer != null)
                {
                    ty = blockBelowPlayer.Y;
                }
            }
            if (!world.Player.Flying)
            {
                if (world.Player.InAir)
                {
                    world.Player.YV -= 0.005;
                }
                if (world.Player.YV < -0.45)
                {
                    world.Player.YV = -0.45;
                }
                if (ty >= world.Player.Y - 1 && world.Player.YV < 0)
                {
                    world.Player.YV = 0;
                }
                world.Player.Y += world.Player.YV;
            }
            if (brightness > 1f)
            {
                brightness = 1f;
            }
            if (brightness < 0.1f)
            {
                brightness = 0.1f;
            }
            world.Player.CPos = new Vector3((float)world.Player.X, (float)world.Player.Y + 1.7f, (float)world.Player.Z);
            world.Player.CFPt = new Vector3((float)(world.Player.X + Math.Cos(Util.DegToRad(world.Player.LX))), (float)(world.Player.Y + 1.7f + Math.Sin(Util.DegToRad(world.Player.LY))), (float)(world.Player.Z + Math.Sin(Util.DegToRad(world.Player.LX))));
        }
    }
}