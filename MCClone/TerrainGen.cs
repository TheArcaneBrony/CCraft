using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MCClone
{
    class TerrainGen
    {
        public static void GenTerrain(List<Chunk> chunkList)
        {
         /*   Thread worldGen = new Thread(() =>
            {
                
            });
            worldGen.Start();*/
            for (double x = -16; x < 16; x++)
            {
                /*Thread worldGen2 = new Thread(() =>
                {

                });
                while (threadCount > 9) Thread.Sleep(100);
                worldGen2.Start();
                worldGen2.Join(130);
                Thread.Sleep(1000);*/
                for (double z = -16; z < 16; z++)
                {
                    Thread.Sleep(10);
                    /*var success = false;
                   // while (!success)
                    {
                        Thread childThread = new Thread(() => {
                            threadCount++;
                            Chunk tmp = new Chunk(x, z);
                            tmp.Generate();
                            chunkList.Add(tmp);
                            threadCount--;
                            Console.WriteLine($"{x} | {z}");
                        });
                        childThread.Name = x + " - " + z; childThread.Start();
                     //   success = childThread.Join(2000);
                    }*/
                    Console.Write($"\n{x} | {z}");
                    Chunk tmp = new Chunk(x, z);
                    for (double x2 = 0; x2 < 16; x2++)
                    {
                        for (double z2 = 0; z2 < 16; z2++)
                        {
                       //     Console.Write($".");
                            double by = /*(int)*/(Math.Sin(Util.DegToRad(x * x2)) * 10 + Math.Sin(Util.DegToRad(z * z2)) * 10);
                            Block blk = new Block(x, by, x);
                            //if (!Blocks.Contains(blk))
                            // {
                            lock (tmp) 
                            tmp.Blocks.Add(blk);
                            // }
                            //Thread.Sleep(10);
                        }
                    }
                    lock(chunkList)
                    chunkList.Add(tmp);

                }
            }

        }
    }
}
