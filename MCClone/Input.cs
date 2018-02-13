using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MCClone
{
    class Input
    {
        public static void HandleKeyboard()
        {
            var player = MainWindow.player;

            var keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Key.Escape))
            {
                MainWindow.running = false;
                Thread.Sleep(100);
                Environment.Exit(0);
            }
            if (!keyState.IsKeyDown(Key.LControl))
            {
                if (keyState.IsKeyDown(Key.W))
                {
                    player.X += Math.Cos(Util.DegToRad(player.LX));
                    player.Z += Math.Sin(Util.DegToRad(player.LX));
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
                if (keyState.IsKeyDown(Key.R))
                {
                    player.X = 0; player.Y = 100 * 2; player.Z = 0;
                }
                if (keyState.IsKeyDown(Key.Q)) MainWindow.brightness -= 0.01;
                if (keyState.IsKeyDown(Key.E)) MainWindow.brightness += 0.01;
                if (keyState.IsKeyDown(Key.F)) player.Flying = true;
            }
            else
            {
                if (keyState.IsKeyDown(Key.F)) player.Flying = false;
                if (keyState.IsKeyDown(Key.W)) player.X += 0.1;
                if (keyState.IsKeyDown(Key.A)) player.Z -= 0.1;
                if (keyState.IsKeyDown(Key.S)) player.X -= 0.1;
                if (keyState.IsKeyDown(Key.D)) player.Z += 0.1;
            }
        }
        public static void Tick()
        {
            var player = MainWindow.player;
            if (!player.Flying) player.YV -= 0.005;
            if (player.YV < -0.45) player.YV = -0.45;
            //  if (bsarr.Contains(new Block((int)cx, (int)cy, (int)cz)) && cyv < 0) cyv = 0;
            if (!player.Flying) player.Y += player.YV;

            player.CPos = new Vector3((float)player.X, (float)player.Y + 2, (float)player.Z);
            player.CFPt = new Vector3((float)(player.X + Math.Cos(Util.DegToRad(player.LX)) * 360), (float)(player.Y + 2 + Math.Sin(Util.DegToRad(player.LY)) * 360) * 2, (float)(player.Z + Math.Sin(Util.DegToRad(player.LX)) * 360));
        }
    }
}
