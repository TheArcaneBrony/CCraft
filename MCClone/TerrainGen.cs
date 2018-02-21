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
            Thread TGThread = new Thread(() =>
            {
                for (double x = -16; x < 16; x++)
                {
                    for (double z = -16; z < 16; z++)
                    {
                    Chunk chunk = new Chunk(x, z, new List<Block>());
                        chunkList.Add(chunk);
                    }
                }
                for (int i = 0; i < chunkList.Count -1; i++)
                {
                    
                        Thread childThread = new Thread(() =>
                        {
                            int ti = i;
                            //lock (chunkList)
                                for (double x2 = 0; x2 < 16; x2++)
                            {
                                for (double z2 = 0; z2 < 16; z2++)
                                {
                                    
                                        double by = Math.Abs((int)((Math.Sin(Util.DegToRad(chunkList[ti].X * 16 + x2)) * 25 + Math.Sin(Util.DegToRad(chunkList[ti].Z * 16 + z2)) * 10) * 1.25));
                                        by = Math.Max(by, 1);
                                        for (int y = (int)by-1; y < by; y++)
                                    {
                                        try
                                        {
                                            chunkList[ti].Blocks.Add(new Block(x2, y, z2));
                                        }
                                        catch (Exception)
                                        {
                                            Thread.Sleep(1000);

                                        }
                                        Thread.Sleep(0);
                                    }
                                }
                                }
                        });
                    
                        childThread.Start();
                    Thread.Sleep(50);
                }
            }); TGThread.Start();
        }
    }
}