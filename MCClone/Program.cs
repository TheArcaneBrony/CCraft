using System;
using System.Linq;
using System.Threading;

namespace MCClone
{
    class Program
    {
        public static string[] args;
        [MTAThread]
        static void Main(string[] args)
        {
            Program.args = args;
            /*for (int i = 0; i < 2000; i++)
            {
                Console.Write("M1N3CR4F7! ");
            }
            Console.Clear();*/
            Console.Write("This testing version of the game logs certain private data, such as your Windows username and pc name to identify whoever the data belongs to.\nThis is sent alongside more diagnostic info like:\n - .NET version,\n - FPS,\n - Game version\nIf you are not okay with this, press N to disable logging, although this will have negative impact on debugging if some problems arise.\n Thank you for understanding! :)\n[Press any key to continue, N to disable logging] ");
            if(!args.Contains("-nodisclaimer")) if (Console.ReadKey(true).Key == ConsoleKey.N) MainWindow.logger = false;
            Console.Clear();
            Console.WriteLine("Controls:\nWASD: Move around\nQ: Brightness down\nE: Brightness up\nSpace: Jump/fly\nShift: Descend\n[F: Enable fly]\n[CTRL + F: Disable fly]");
            for (int i = 100; i > 0; i--)
            {
                Thread.Sleep(5);
                Console.Write("\r" + new string(' ', i + 5) + "\r" + new string('.', i));
            }
           /* var player = new PCMPlayer();

            var rand = new Random();
            var bytes = new byte[32000];
            List<Byte> bytes2 = new List<Byte>();
            rand.NextBytes(bytes);
         /*   for (int i = 0; i < bytes.Length; i++)
            {
               // byte byte = 0x100;
                //bytes2.Add(BitConverter.GetBytes(i%255)[1]);
                bytes[i] = BitConverter.GetBytes(i)[0];

            }
            bytes = bytes2.ToArray();
            player.AddSamples(bytes);*/
            Console.Write("\r \n");
            Thread thread = new Thread(() => {
                var EH = new DiscordRpc.EventHandlers();
                DiscordRpc.Initialize("333608929575698442", ref EH, true, "");
                var PR = new DiscordRpc.RichPresence() { details = "Developing", state = "Just debugging ;)", startTimestamp = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds, smallImageKey = "test1", smallImageText = "testing" };
                DiscordRpc.UpdatePresence(ref PR);
            });
            thread.Start();

            new MainWindow().Run(60);
        }
    }
}