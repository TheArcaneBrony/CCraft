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
                        //Thread.Sleep(5);
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
                Logger.PostLog($"Loaded chunk {X}/{Z}");
                return ch;
            }
            else
            {
                Logger.PostLog($"Generating chunk {X}/{Z}");
                return GenChunk(chunkList, X, Z);
            }
        }
        public static int GetHeight(int x, int z) => (int)Math.Abs(((Math.Sin(Util.DegToRad(x)) * 25 + Math.Sin(Util.DegToRad(z)) * 10) * 1.725));// 2;
        public static double FinalNoise(double x, double z, double py) => Cuberp(py, py + RandomNoise() * (RandomNoise()+0.1), py + RandomNoise(), py + RandomNoise() * 2, RandomNoise());
        public static float RandomNoise()
        {
            return (float) random.NextDouble();
        }
        public static double Lerp(double v0, double v1, float t) => v0+(v1-v0)*t;
        public static double Cosinerp(double v0, double v1, float t)
        {
            double ft = t * 3.1415927;

            double f = (1 - Math.Cos(ft)) * .5;

            return v0 * (1 - f) + v1 * f;
        }
        public static double Cuberp(double y0, double y1, double y2, double y3, float mu)
        {
           double a0,a1,a2,a3,mu2;

           mu2 = mu * mu;

           a0 = y3 - y2 - y0 + y1;

           a1 = y0 - y1 - a0;

           a2 = y2 - y0;

           a3 = y1;
           return (a0 * mu * mu2 + a1 * mu2 + a2 * mu + a3);

        }
    }
}