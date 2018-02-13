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
                        List<Block> blocks = new List<Block>();
                        var success = false;
                        while (!success)
                        {
                            Thread childThread = new Thread(() =>
                            {
                                
                                for (double x2 = 0; x2 < 16; x2++)
                                {
                                    for (double z2 = 0; z2 < 16; z2++)
                                    {
                                        //  double by = /*(int)*/(Math.Sin(Util.DegToRad(x * x2)) * 10 + Math.Sin(Util.DegToRad(z * z2)) * 10);
                                       // Block blk = new Block(x2, 10, z2);
                                            blocks.Add(new Block(x2, 10, z2));
                                    }
                                }
                                
                            });
                            childThread.Name = x + " - " + z; childThread.Start();
                            success = childThread.Join(500);
                        }
                            chunkList.Add(new Chunk(x,z,blocks));
                        // Console.Write($"\n{x} | {z}");

                    }
                }
            }); TGThread.Start();
        }

    }
}