using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MCClone
{
    class TerrainGen
    {
        public static Random random = new Random();
        public static void GenTerrain(List<Chunk> chunkList)
        {
            Thread initialGen = new Thread(() =>
            {
                for (int x = -8; x < 8; x++)
                {
                    for (int z = -8; z < 8; z++)
                    {
                        GetChunk(chunkList, x, z);
                        Thread.Sleep(250);
                    }
                }
            });
            initialGen.Start();
        }
        public static Chunk GenChunk(List<Chunk> chunkList,int X, int Z)
        {
            Chunk chunk = new Chunk(X, Z, new List<Block>());
            chunkList.Add(chunk);
            Task.Run(() =>
            {
                for (int x2 = 0; x2 < 16; x2++)
                {
                    for (int z2 = 0; z2 < 16; z2++)
                    {
                        int by = GetHeight(X * 16 + x2, Z * 16 + z2);
                        by = Math.Max(by, 1);
                        // int by = 2;
                        //Thread.Sleep(1);
                        for (int y = by - 1; y < by; y++)
                        {
                            chunk.Blocks.Add(new Block(x2, (UInt16)y, z2));
                        }
                    }
                }
                try
                {
                    Task.Run(() => { File.WriteAllText($"Worlds/{MainWindow.world.Name}/ChunkData/{chunk.X}.{chunk.Z}.json", JsonConvert.SerializeObject(chunk)); });
                }
                catch (IOException)
                {
                    Console.WriteLine($"Failed to save chunk: {X}/{Z}");
                }
                catch
                {
                    throw;
                }
            });
            return chunk;
        }
        public static Chunk GetChunk(List<Chunk> chunkList, int X, int Z)
        {
            if (File.Exists($"Worlds/{MainWindow.world.Name}/ChunkData/{X}.{Z}.json"))
            {
                Chunk ch = JsonConvert.DeserializeObject<Chunk>(File.ReadAllText($"Worlds/{MainWindow.world.Name}/ChunkData/{X}.{Z}.json"));
                chunkList.Add(ch);
                Logger.LogQueue.Add($"Loaded chunk {X}/{Z}");
                return ch;
            }
            else
            {
                Logger.LogQueue.Add($"Generating chunk {X}/{Z}");
                return GenChunk(chunkList, X, Z);
            }
        }
        public static int GetHeight(int x, int z) => (int)Math.Abs(((Math.Sin(Util.DegToRad(x)) * 25 + Math.Sin(Util.DegToRad(z)) * 10) * 1.2));// 2;
    }
}