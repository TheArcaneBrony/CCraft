using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;

using Newtonsoft.Json;

namespace MCClone
{
    public class TerrainGen
    {
        public static World world = new World(0, 0, 0);
        private static bool ShouldLoadChunks = true;
        public static Stopwatch GenTime = new Stopwatch();
        public static int runningThreads = 0, waitingthreads = 0;
        public static int maxThreads = Environment.ProcessorCount * 4;
        public static void GenTerrain()
        {
            double renderDistance = 10, genDistance = 1.2;
            for (int x = 0; x < renderDistance * genDistance; x++)
            {
                for (double y = 0; y < 360; y++)
                {
                    (int, int) pos = ((int)(world.Player.X / 16 + x * Math.Sin(Util.DegToRad(y))), (int)(world.Player.Z / 16 + x * Math.Cos(Util.DegToRad(y))));
                    if (!world.Chunks.ContainsKey(pos))
                        GetChunk(pos.Item1, pos.Item2);
                }
            }
        }
        public static Random rnd = new Random();
        public static Chunk GenChunk(int X, int Z)
        {
            Chunk chunk = new Chunk(X, Z);
            if (world.Chunks.ContainsKey((X, Z)))
            {
                world.Chunks.TryGetValue((X, Z), out chunk);
                return chunk;
            }
            world.Chunks.Add((X, Z), chunk);
            Thread chunkGen = new Thread(() =>
            {
                waitingthreads++;
                while (runningThreads + 1 >= maxThreads)
                {
                    Thread.Sleep(100);
                }
                waitingthreads--;
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
                        byte[] data = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(new SaveChunk(chunk)));
                        GZipStream chOut = new GZipStream(new FileStream($"Worlds/{world.Name}/ChunkData/{X}.{Z}.gz", FileMode.Create), CompressionLevel.Optimal);
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
            if (ShouldLoadChunks && File.Exists($"Worlds/{world.Name}/ChunkData/{X}.{Z}.gz"))
            {
                uint length;
                byte[] b = new byte[4];
                using (FileStream fs = File.OpenRead($"Worlds/{world.Name}/ChunkData/{X}.{Z}.gz"))
                {
                    fs.Position = fs.Length - 4;
                    fs.Read(b, 0, 4);
                    length = BitConverter.ToUInt32(b, 0);
                }
                byte[] data = new byte[length];
                GZipStream chIn = new GZipStream(new FileStream($"Worlds/{world.Name}/ChunkData/{X}.{Z}.gz", FileMode.Open), CompressionMode.Decompress);
                chIn.Read(data, 0, data.Length);
                chIn.Close();
                SaveChunk sch = JsonConvert.DeserializeObject<SaveChunk>(Encoding.ASCII.GetString(data));
                Chunk ch = new Chunk(X, Z);
                foreach (Block bl in sch.Blocks)
                {
                    ch.Blocks.Add((bl.X, bl.Y, bl.Z), bl);
                }
                world.Chunks.Add((X, Z), ch);
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