using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MCClone.TerrainGenTest
{
    class Program
    {
        public static string[] args;
        static void Main(string[] largs)
        {
            args = largs;
            Stopwatch sw = new Stopwatch();
            World world = new World("test");
            foreach(Dimension d in world.Dimensions.Values)
            {
                sw.Restart();
                Console.Write($"Generating dimension {d.Name} @ 2...");
                TerrainGen.GenerateAround(d, 0, 0, 2);
                Console.WriteLine($"\rGenerated dimension {d.Name} @ 2 in {sw.Elapsed}");
            }
            /*foreach (Dimension d in world.Dimensions.Values)
            {
                sw.Restart();
                Console.Write($"Generating dimension {d.Name} @ 16...");
                TerrainGen.GenerateAround(d, 0, 0, 16);
                Console.WriteLine($"\rGenerated dimension {d.Name} @ 16 in {sw.Elapsed}");
            }
            foreach (Dimension d in world.Dimensions.Values)
            {
                sw.Restart();
                Console.Write($"Generating dimension {d.Name} @ 32...");
                TerrainGen.GenerateAround(d, 0, 0, 32);
                Console.WriteLine($"\rGenerated dimension {d.Name} @ 32 in {sw.Elapsed}");
            }*/
            sw.Restart();
            Console.Write($"Saving world!");
            world.Save();
            Console.WriteLine("\rSaved world in " + sw.Elapsed);
            Console.ReadLine();
        }
    }
}
