using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;

using Newtonsoft.Json;

namespace MCClone
{
    internal class TerrainGen
    {
        private static bool ShouldLoadChunks = false;
        public static Stopwatch GenTime = new Stopwatch();
        public static int runningThreads = 0;
        public static int maxThreads = (int)Math.Pow(MainWindow.genDistance * MainWindow.renderDistance, 2) * Environment.ProcessorCount / 1;
        public static void GenTerrain()
        {
            for (int x = 0; x < MainWindow.renderDistance * MainWindow.genDistance; x++)
            {
                for (double y = 0; y < 360; y++)
                {
                    (int, int) pos = ((int)(MainWindow.world.Player.X / 16 + x * Math.Sin(Util.DegToRad(y))), (int)(MainWindow.world.Player.Z / 16 + x * Math.Cos(Util.DegToRad(y))));
                    if (!MainWindow.world.Chunks.ContainsKey(pos))
                        GetChunk(pos.Item1, pos.Item2);
                }
            }
        }
        public static Random rnd = new Random();
        public static Chunk GenChunk(int X, int Z)
        {
            Chunk chunk = new Chunk(X, Z);
            if (MainWindow.world.Chunks.ContainsKey((X, Z)))
            {
                MainWindow.world.Chunks.TryGetValue((X, Z), out chunk);
                return chunk;
            }
            MainWindow.world.Chunks.Add((X, Z), chunk);
            Thread chunkGen = new Thread(() =>
            {
                while (runningThreads + 1 >= maxThreads)
                {
                    Thread.Sleep(100);
                }
                runningThreads++;
                Stopwatch GenTimer = new Stopwatch();
                GenTimer.Start();
                try
                {
                    for (int x2 = 0; x2 < 16; x2++)
                    {
                        for (int z2 = 0; z2 < 16; z2++)
                        {
                            for (int y = GetHeight(X * 16 + x2, Z * 16 + z2); y <= GetHeight(X * 16 + x2, Z * 16 + z2); y++)
                            {
                                Thread.Sleep(10);
                                chunk.Blocks.Add((x2, y, z2), new Block(x2, y, z2));
                            }
                        }
                    }
                    try
                    {
                        byte[] data = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(chunk));
                        GZipStream chOut = new GZipStream(new FileStream($"Worlds/{MainWindow.world.Name}/ChunkData/{X}.{Z}.gz", FileMode.Create), CompressionLevel.Optimal);
                        chOut.Write(data, 0, data.Length);
                        chOut.Close();
                    }
                    catch (IOException)
                    {
                        Console.WriteLine($"Failed to save chunk: {X}/{Z}");
                    }
                    catch
                    {
                    }
                    Logger.LogQueue.Enqueue($"Chunk {X}/{Z} generated in: {GenTimer.ElapsedTicks / 10000d} ms");
                }
                catch { }
                finally
                {
                    runningThreads--;
                }
            })
            {
                Name = $"Chunk Gen {X}/{Z}",
            };
            chunkGen.Start();
            return chunk;
        }
        public static Chunk GetChunk(int X, int Z)
        {
            GenTime.Restart();
            Console.Title = $"{runningThreads}";
            if (ShouldLoadChunks && File.Exists($"Worlds/{MainWindow.world.Name}/ChunkData/{X}.{Z}.gz"))
            {
                uint length;
                byte[] b = new byte[4];
                using (FileStream fs = File.OpenRead($"Worlds/{MainWindow.world.Name}/ChunkData/{X}.{Z}.gz"))
                {
                    fs.Position = fs.Length - 4;
                    fs.Read(b, 0, 4);
                    length = BitConverter.ToUInt32(b, 0);
                }
                byte[] data = new byte[length];
                GZipStream chIn = new GZipStream(new FileStream($"Worlds/{MainWindow.world.Name}/ChunkData/{X}.{Z}.gz", FileMode.Open), CompressionMode.Decompress);
                chIn.Read(data, 0, data.Length);
                chIn.Close();
                Chunk ch = JsonConvert.DeserializeObject<Chunk>(Encoding.ASCII.GetString(data));
                ch.X = X; ch.Z = Z;
                MainWindow.world.Chunks.Add((X, Z), ch);
                Logger.LogQueue.Enqueue($"Loaded chunk {X}/{Z} in: {GenTime.ElapsedTicks / 10000d} ms");
                data = null;
                return ch;
            }
            else
            {
                Chunk ch = GenChunk(X, Z);
                return ch;
            }
        }
        // old terrain gen function
        //public static int GetHeight(int x, int z) => (int)Math.Abs(((Math.Sin(Util.DegToRad(x)) * 25 + Math.Sin(Util.DegToRad(z)) * 10) * 1.2));// 2;
        public static int GetHeight(int x, int z)
        {
            int y = (int)Math.Max(Math.Floor(Math.Abs(((Math.Sin(Util.DegToRad(x)) * 25 + Math.Sin(Util.DegToRad(z)) * 10) * 1.2))), 0);
            return y;
        }
    }
}