using OpenTK;
using OpenTK.Input;
using System;
using System.Threading;

namespace MCClone
{
    class Input
    {
        public static void HandleInput()
        {

            var keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Key.Escape))
            {
                MainWindow.running = false;
                DiscordRpc.ClearPresence();
                DiscordRpc.Shutdown();
                Thread.Sleep(50);
                Environment.Exit(0);
            }

            if (keyState.IsKeyDown(Key.W))
            {
                MainWindow.world.Player.X += Math.Cos(Util.DegToRad(MainWindow.world.Player.LX));
                MainWindow.world.Player.Z += Math.Sin(Util.DegToRad(MainWindow.world.Player.LX));
                if (keyState.IsKeyDown(Key.LControl))
                {
                    MainWindow.world.Player.X += Math.Cos(Util.DegToRad(MainWindow.world.Player.LX));
                    MainWindow.world.Player.Z += Math.Sin(Util.DegToRad(MainWindow.world.Player.LX));
                }
            }
            if (keyState.IsKeyDown(Key.A))
            {
                MainWindow.world.Player.X -= Math.Cos(Util.DegToRad(MainWindow.world.Player.LX + 90));
                MainWindow.world.Player.Z -= Math.Sin(Util.DegToRad(MainWindow.world.Player.LX + 90));
            }
            if (keyState.IsKeyDown(Key.S))
            {
                MainWindow.world.Player.X -= Math.Cos(Util.DegToRad(MainWindow.world.Player.LX));
                MainWindow.world.Player.Z -= Math.Sin(Util.DegToRad(MainWindow.world.Player.LX));
            }
            if (keyState.IsKeyDown(Key.D))
            {
                MainWindow.world.Player.X += Math.Cos(Util.DegToRad(MainWindow.world.Player.LX + 90));
                MainWindow.world.Player.Z += Math.Sin(Util.DegToRad(MainWindow.world.Player.LX + 90));
            }
            if (keyState.IsKeyDown(Key.ShiftLeft)) MainWindow.world.Player.Y -= 0.1;
            if (keyState.IsKeyDown(Key.Space)) if (MainWindow.world.Player.Flying) MainWindow.world.Player.Y += 0.1; else MainWindow.world.Player.YV = 0.1;
            if (keyState.IsKeyDown(Key.Q)) MainWindow.brightness -= 0.01f;
            if (keyState.IsKeyDown(Key.E)) MainWindow.brightness += 0.01f;
            if (keyState.IsKeyDown(Key.F)) MainWindow.world.Player.Flying = true;
            if (keyState.IsKeyDown(Key.F) && keyState.IsKeyDown(Key.LControl)) MainWindow.world.Player.Flying = false;
            if (MainWindow.focussed && !(Mouse.GetCursorState().X == MainWindow.centerX || Mouse.GetCursorState().Y == MainWindow.centerY))
            {
                double x = Mouse.GetCursorState().X - MainWindow.centerX;
                double y = -(Mouse.GetCursorState().Y - MainWindow.centerY);

                OpenTK.Input.Mouse.SetPosition(MainWindow.centerX, MainWindow.centerY);
                MainWindow.world.Player.LY += y * MainWindow.sensitivity;
                MainWindow.world.Player.LX += x * MainWindow.sensitivity;


                if (MainWindow.world.Player.LX <= -180) MainWindow.world.Player.LX = 179.99;
                MainWindow.world.Player.LX += 180;
                //MainWindow.world.Player.LX = Math.Abs(MainWindow.world.Player.LX);
                MainWindow.world.Player.LX %= 360;
                MainWindow.world.Player.LX -= 180;

                if (MainWindow.world.Player.LY > 90) MainWindow.world.Player.LY = 90;
                if (MainWindow.world.Player.LY < -90) MainWindow.world.Player.LY = -90;
            }
        }
        public static void Tick()
        {
            if (!MainWindow.world.Player.Flying && MainWindow.world.Player.InAir) MainWindow.world.Player.YV -= 0.005;
            if (MainWindow.world.Player.YV < -0.45) MainWindow.world.Player.YV = -0.45;

            //  if (bsarr.Contains(new Block((int)cx, (int)cy, (int)cz)) && cyv < 0) cyv = 0;
            //if()
            //if (MainWindow.world.Chunks)
            if (!MainWindow.world.Player.Flying) MainWindow.world.Player.Y += MainWindow.world.Player.YV;

            if (MainWindow.brightness > 1f) MainWindow.brightness = 1f;
            if (MainWindow.brightness < 0.1f) MainWindow.brightness = 0.1f;
            Console.Title = $"{MainWindow.brightness}";
            MainWindow.world.Player.CPos = new Vector3((float)MainWindow.world.Player.X, (float)MainWindow.world.Player.Y + 1.7f, (float)MainWindow.world.Player.Z);
            MainWindow.world.Player.CFPt = new Vector3((float)(MainWindow.world.Player.X + Math.Cos(Util.DegToRad(MainWindow.world.Player.LX))), (float)(MainWindow.world.Player.Y + 1.7f + Math.Sin(Util.DegToRad(MainWindow.world.Player.LY))), (float)(MainWindow.world.Player.Z + Math.Sin(Util.DegToRad(MainWindow.world.Player.LX))));
        }
    }
}