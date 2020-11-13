//using MCClone.Client.UI;
using System;
using System.IO;
using System.Threading;
//using System.Windows.Forms;
//using System.Windows.Threading;

namespace MCClone
{
    internal class Program
    {
        public static string[] args;
        [STAThread]
        private static void ShowActivityViewer()
        {
            Thread thr = new Thread(() =>
            {
                //ActivityViewer activityViewer = DataStore.activityViewer = new ActivityViewer();
                //activityViewer.Show();
                //Dispatcher.Run();
            });
            thr.SetApartmentState(ApartmentState.STA);
            thr.IsBackground = true;
            thr.Start();
        }
        [MTAThread]
        private static void Main(string[] args)
        {
            Program.args = args;
            Util.args = args;
            SystemUtils.SetWindowPosition(0, 0, 990, 512); //990, 512);
            Directory.CreateDirectory($"Worlds");
            Directory.CreateDirectory($"Mods");
            string[] HQuote = new string[] { "MOMIJI INUBASHIRI!", "SQUID GIRL!", "MINECRAFT ROCKS!" };
            int rnd = new Random().Next(HQuote.Length);
            for (int i = 0; i < 200; i++)
            {
                Console.Out.WriteAsync(HQuote[rnd] + " ");
            }
            Console.Clear();
            /*Console.Write("This testing version of the game may log certain private data, such as your Windows username and pc name to identify whoever the data belongs to.\nThis is sent alongside more diagnostic info like:\n - .NET version,\n - FPS,\n - Game version\nIf you are not okay with this, press N to disable logging, although this will have negative impact on debugging if some problems arise.\n Thank you for understanding! :)\n[Press any key to continue, N to disable logging] ");
            if (!args.Contains("-nodisclaimer") && Console.ReadKey(true).Key == ConsoleKey.N)
            {
                MainWindow.logger = false;
            }*/
            Console.Clear();
            Console.WriteLine(@"Controls:
WASD: Move around
Q -   Brightness    + E
Z - Render Distance + C
Space: Jump/fly
Shift: Descend
F: Enable fly
CTRL + F: Disable fly
F1: Toggle mouse capture
ESC: Exit game");
            for (int i = 10; i > 0; i--)
            {
                Thread.Sleep(5);
                Console.Write("\r" + new string(' ', i + 1) + " \r" + new string('.', i));
            }
            Console.Write("\r \n");
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);

            ShowActivityViewer();
            new MainWindow().Run();
        }
    }
}
