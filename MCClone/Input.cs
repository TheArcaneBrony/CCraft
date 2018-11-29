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
            var player = MainWindow.world.Player;

            var keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Key.Escape))
            {
                MainWindow.running = false;
                DiscordRpc.ClearPresence();
                DiscordRpc.Shutdown();
                Thread.Sleep(100);
                Environment.Exit(0);
            }

                if (keyState.IsKeyDown(Key.W))
                {
                    player.X += Math.Cos(Util.DegToRad(player.LX));
                    player.Z += Math.Sin(Util.DegToRad(player.LX));
                if (!keyState.IsKeyDown(Key.LControl))
                {
                    player.X += Math.Cos(Util.DegToRad(player.LX));
                    player.Z += Math.Sin(Util.DegToRad(player.LX));
                }
                }
                if (keyState.IsKeyDown(Key.A))
                {
                    player.X -= Math.Cos(Util.DegToRad(player.LX + 90));
                    player.Z -= Math.Sin(Util.DegToRad(player.LX + 90));
                }
                if (keyState.IsKeyDown(Key.S))
                {
                    player.X -= Math.Cos(Util.DegToRad(player.LX));
                    player.Z -= Math.Sin(Util.DegToRad(player.LX));
                }
                if (keyState.IsKeyDown(Key.D))
                {
                    player.X += Math.Cos(Util.DegToRad(player.LX + 90));
                    player.Z += Math.Sin(Util.DegToRad(player.LX + 90));
                }
                if (keyState.IsKeyDown(Key.ShiftLeft)) player.Y -= 0.1;
                if (keyState.IsKeyDown(Key.Space)) if (player.Flying) player.Y += 0.1; else player.YV = 0.1;
                if (keyState.IsKeyDown(Key.Q)) MainWindow.brightness -= 0.01f;
                if (keyState.IsKeyDown(Key.E)) MainWindow.brightness += 0.01f;
                if (keyState.IsKeyDown(Key.F)) player.Flying = true;
                if (keyState.IsKeyDown(Key.F)&&keyState.IsKeyDown(Key.LControl)) player.Flying = false;



            if (MainWindow.focussed && !(Mouse.GetCursorState().X == MainWindow.centerX || Mouse.GetCursorState().Y == MainWindow.centerY))
            {
                //Console.WriteLine($"{Mouse.GetCursorState().Y - MainWindow.centerY}");
                double x = Mouse.GetCursorState().X - MainWindow.centerX;
                double y = -(Mouse.GetCursorState().Y - MainWindow.centerY);
                //Point center = new Point(MainWindow.centerX, MainWindow.centerY);
               // Point mousePos = PointToScreen(center);

                OpenTK.Input.Mouse.SetPosition(MainWindow.centerX, MainWindow.centerY);
                MainWindow.world.Player.LY += y * MainWindow.sensitivity;
                MainWindow.world.Player.LX += x * MainWindow.sensitivity;

                if (MainWindow.world.Player.LY > 90) MainWindow.world.Player.LY = 90;
                if (MainWindow.world.Player.LY < -90) MainWindow.world.Player.LY = -90;

                //Logger.LogQueue.Add($"MV_MOUSE: X={x},Y={y}");
            }
        }
        public static void Tick()
        {
            var player = MainWindow.world.Player;
            player.Xa = (int)player.X;
            player.Ya = (int)player.Y;
            player.Za = (int)player.Z;
            if (!player.Flying && player.InAir) player.YV -= 0.005;
            if (player.YV < -0.45) player.YV = -0.45;

            //  if (bsarr.Contains(new Block((int)cx, (int)cy, (int)cz)) && cyv < 0) cyv = 0;
            //if()
            //if (MainWindow.world.Chunks)
            if (!player.Flying) player.Y += player.YV;

            if (MainWindow.brightness > 90) MainWindow.brightness = 90;
            if (MainWindow.brightness < 0.1f) MainWindow.brightness = 0.1f;
            Console.Title = $"{MainWindow.brightness}";
            player.CPos = new Vector3((float)player.X, (float)player.Y + 2, (float)player.Z);
            player.CFPt = new Vector3((float)(player.X + Math.Cos(Util.DegToRad(player.LX))), (float)(player.Y + 2 + Math.Sin(Util.DegToRad(player.LY))), (float)(player.Z + Math.Sin(Util.DegToRad(player.LX))));
        }
    }
}
