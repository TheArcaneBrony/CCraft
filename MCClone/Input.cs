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
    }
}
