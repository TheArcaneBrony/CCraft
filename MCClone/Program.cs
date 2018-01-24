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
            for (int i = 30; i > 0; i--)
            {
                Thread.Sleep(50);
                Console.Write("\r" + new string(' ',i+5) +"\r"+new string('.',i));
            }
            new MainWindow().Run(30);
        }
    }
}
