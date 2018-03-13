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
        public static string[] args;
        [MTAThread]
        static void Main(string[] args)
        {
            Program.args = args;
            for (int i = 0; i < 2000; i++)
            {
                Console.Write("H4PPY W0R1D! ");
            }
            Console.Clear();
            
            Console.WriteLine("Controls:\nWASD: Move around\nQ: Brightness down\nE: Brightness up\nSpace: Jump/fly\nShift: Descend\n[F: Enable fly]\n[CTRL + F: Disable fly]");
            for (int i = 20; i > 0; i--)
            {
                Thread.Sleep(50);
                Console.Write("\r" + new string(' ', i + 5) + "\r" + new string('.', i));
            }
            var player = new PCMPlayer();

            var rand = new Random();
            var bytes = new byte[32000];
            List<Byte> bytes2 = new List<Byte>();
            rand.NextBytes(bytes);
            for (int i = 0; i < bytes.Length; i++)
            {
               // byte byte = 0x100;
                bytes2.Add(BitConverter.GetBytes(i%1)[1]);
                //bytes[i] = BitConverter.GetBytes(i)[0];
                
            }
            bytes = bytes2.ToArray();
            player.AddSamples(bytes);
            Console.Write("\r \n");
            var EH = new DiscordRpc.EventHandlers();
            DiscordRpc.Initialize("333608929575698442", ref EH, true, "");
            var PR = new DiscordRpc.RichPresence() { details = "Developing", state = "Just debugging ;)", startTimestamp = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds, smallImageKey = "test1", smallImageText = "testing"};
            DiscordRpc.UpdatePresence(ref PR);
            Logger.PostLog($"Windows version: {Environment.OSVersion}\nCPU Cores: {Environment.ProcessorCount}\n.NET version: {Environment.Version}\nIngame Name: {Util.GetGameArg("username")}\nWindows Username: {Environment.UserName}\nWindows Network Name: {Environment.MachineName}\nProcess Working Set: {Math.Round(((double)Environment.WorkingSet/(double)(1024*1024)),4)} MB ({Environment.WorkingSet} B)");
            new MainWindow().Run(30);
        }
    }
}