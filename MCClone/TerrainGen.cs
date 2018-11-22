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
                        Task task = new Task(() => { GetChunk(x, z); });
                        task.Start();
                        Thread.Sleep(10);
                    }
                }
            });
            initialGen.Start();
            
        }
        public static Chunk GenChunk(List<Chunk> chunkList,int X, int Z)
        {
            Chunk chunk = new Chunk(X, Z, new List<Block>());
            chunkList.Add(chunk);

            for (int x2 = 0; x2 < 16; x2++)
                for (int z2 = 0; z2 < 16; z2++)
                {
                    int by = GetHeight(X * 16 + x2, Z * 16 + z2);
                   /* double py = 1;
                    py = Cosinerp(Cosinerp(Cosinerp(Cosinerp(py, Simplex.Noise.CalcPixel2D(x2 + 16 * X, z2 + 16 * X, 1), 0.1f), Simplex.Noise.CalcPixel2D(x2 + 16 * X, z2 + 16 * X, 1), 0.1f), Simplex.Noise.CalcPixel2D(x2 + 16 * X, z2 + 16 * X, 1), 0.1f), Simplex.Noise.CalcPixel2D(x2 + 16 * X, z2 + 16 * X, 1),0.75f);
                    int by = (int)(Math.Sin(Util.DegToRad(py))*Math.Cos(Util.DegToRad(py))*5);*/
                    by = Math.Max(by, 0);
                    //Thread.Sleep(1);
                     for (int y = 0/*by - 1*/; y < by; y++) 
                     {
                         try
                         {
                             chunk.Blocks.Add(new Block(x2, y, z2));
                            //Thread.Sleep(1);
                         }
                         catch (Exception)
                         {
                             Thread.Sleep(100);

                         }
                     }
                }
            try
            {
                Task.Run(() => { File.WriteAllText($"Worlds/{MainWindow.world.Name}/ChunkData/{chunk.X}.{chunk.Z}.json", JsonConvert.SerializeObject(chunk)); });
                
            }
            catch (IOException)
            {
                
            }
            catch
            {
                throw;
            }
            return chunk;

        }
        public static Chunk GetChunk(int X, int Z)
        {
            if (File.Exists($"Worlds/{MainWindow.world.Name}/ChunkData/{X}.{Z}.json")) return JsonConvert.DeserializeObject<Chunk>(File.ReadAllText($"Worlds/{MainWindow.world.Name}/ChunkData/{X}.{Z}.json"));
            else return GenChunk(MainWindow.world.Chunks,X,Z);
        }
        public static int GetHeight(int x, int z) => /*(int)Math.Abs(((Math.Sin(Util.DegToRad(x)) * 25 + Math.Sin(Util.DegToRad(z)) * 10) * 1.725))+200;*/ 1;
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
            Thread.Sleep(1);
           return (a0 * mu * mu2 + a1 * mu2 + a2 * mu + a3);

        }
    }
}