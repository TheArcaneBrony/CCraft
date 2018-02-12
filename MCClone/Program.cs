using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Platform;

namespace MCClone
{
   
    class Program
    {
        [MTAThread]
        static void Main(string[] args)
        {

            Console.WriteLine("Controls:\nW: X+\nS: X-\nD: Z+\nA: Z-\nQ: Brightness down\nE: Brightness up\nSpace: Jump/fly\nShift: Descend\nR: Go to 0/200/0\nF: Enable fly\nCTRL + F: Disable fly");
            /*for (int i = 30; i > 0; i--)
            {
                Thread.Sleep(50);
                Console.Write("\r" + new string(' ', i + 5) + "\r" + new string('.', i));
            }
            /*Console.Clear();
            for (int i = 0; i < 32; i++)
            {
                Console.SetCursorPosition(16, i);
                Console.Write('|');
            }*/
            var EH = new DiscordRpc.EventHandlers();
            DiscordRpc.Initialize("333608929575698442", ref EH, true, "");
            var PR = new DiscordRpc.RichPresence() { details = "Developing", state = "Just debugging ;)", startTimestamp = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds, smallImageKey = "test1", smallImageText = "testing"};
            DiscordRpc.UpdatePresence(ref PR);
            
            new MainWindow().Run(30);
        }
    }
}
