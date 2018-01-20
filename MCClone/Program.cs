using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Platform;

namespace MCClone
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Controls:\nW: X+\nS: X-\nD: Z+\nA: Z-\nQ: Brightness down\nE: Brightness up\nSpace: Jump/fly\nShift: Descend\nR: Go to 0/100/0");
            Console.ReadKey();
            Console.Write("\b ");
            new MainWindow().Run(120);
        }
    }
}
