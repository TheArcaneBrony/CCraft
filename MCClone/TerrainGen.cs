using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MCClone
{
    public class TerrainGen
    {
        public static World world = new World(0, 20, 0)
        {
            Name = "Test"
        };
        private static bool ShouldLoadChunks = true;
        public static Stopwatch GenTime = new Stopwatch();
        public static int runningThreads = 0, waitingthreads = 0;
        public static int maxThreads = Environment.ProcessorCount * 16;
        public static List<Thread> threads = new List<Thread>();
        public static void GenTerrain()
        {
            double renderDistance = 2, genDistance = 1.2;
            for (int x = (int)(-renderDistance * genDistance); x < renderDistance * genDistance; x++)
            {
                for (double z = -renderDistance * genDistance; z < renderDistance * genDistance; z++)
                {
                    //(int, int) pos = ((int)(world.Player.X / 16 + x * Math.Sin(Util.DegToRad(z))), (int)(world.Player.Z / 16 + x * Math.Cos(Util.DegToRad(z))));
                    (int, int) pos = ((int)(world.Player.X / 16 + x), (int)(world.Player.Z / 16 + z));
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
            Thread chunkGen = new Thread(() =>
            {
                waitingthreads++;
                while (runningThreads >= maxThreads)
                {
                    Thread.Sleep(500);
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
                                // Thread.Sleep(10);
                                chunk.Blocks.Add((x2, y, z2), new Block(x2, y, z2));
                            }
                        }
                    }
                    try
                    {
                        Task.Run(() =>
                        {
                            byte[] data = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(new SaveChunk(chunk)));
                            GZipStream chOut = new GZipStream(new FileStream($"Worlds/{world.Name}/ChunkData/{X}.{Z}.gz", FileMode.Create), CompressionLevel.Optimal);
                            chOut.Write(data, 0, data.Length);
                            chOut.Close();
                        });
                    }
                    catch (IOException)
                    {
                        Console.WriteLine($"Failed to save chunk: {X}/{Z}");
                    }
                    catch
                    {
                        throw;
                    }
                    chunk.Finished = true;
                    Logger.LogQueue.Enqueue($"Chunk {X}/{Z} generated in: {GenTimer.ElapsedTicks / 10000d} ms, {chunk.Blocks.Count} blocks.");
                }
                catch { }
                finally
                {
                    runningThreads--;
                    Thread.Sleep(250);
                    DataStore.Threads.Remove(Thread.CurrentThread);
                }
            })
            {
                Name = $"Chunk Gen {X}/{Z}",
            };
            threads.Add(chunkGen);
            DataStore.Threads.Add(chunkGen);
            chunkGen.Start();
            return chunk;
        }
        public static Chunk GetChunk(int X, int Z)
        {
            GenTime.Restart();
            Chunk ch = new Chunk(X, Z);
            Directory.CreateDirectory($"Worlds/{world.Name}/ChunkData/");
            if (DataStore.Server || !DataStore.Multiplayer)
            {
                if (ShouldLoadChunks && File.Exists($"Worlds/{world.Name}/ChunkData/{X}.{Z}.gz"))
                {
                    int length;
                    byte[] b = new byte[4];
                    using (FileStream fs = File.OpenRead($"Worlds/{world.Name}/ChunkData/{X}.{Z}.gz"))
                    {
                        fs.Position = fs.Length - 4;
                        fs.Read(b, 0, 4);
                        length = BitConverter.ToInt32(b, 0);
                    }
                    byte[] data = new byte[length];
                    GZipStream chIn = new GZipStream(new FileStream($"Worlds/{world.Name}/ChunkData/{X}.{Z}.gz", FileMode.Open), CompressionMode.Decompress);
                    chIn.Read(data, 0, data.Length);
                    chIn.Close();
                    SaveChunk sch = JsonConvert.DeserializeObject<SaveChunk>(Encoding.ASCII.GetString(data));
                    foreach (Block bl in sch.Blocks)
                    {
                        ch.Blocks.Add((bl.X, bl.Y, bl.Z), bl);
                    }
                    Logger.LogQueue.Enqueue($"Loaded chunk {X}/{Z} in: {GenTime.ElapsedTicks / 10000d} ms");
                }
                else
                {
                    ch = GenChunk(X, Z);
                }
            }
            else
            {
                ch = DownloadChunk(X,Z);   
            }
            world.Chunks.Add((X, Z), ch);
            return ch;
        }
        public static Chunk DownloadChunk(int X, int Z)
        {
            while (MainWindow._serverStream == null) Thread.Sleep(500);
            NetworkHelper.Send(MainWindow._serverStream, $"getchunk {X} {Z}");

            Chunk ch = new Chunk(X, Z);
            Task.Run(() =>
            {
                SaveChunk sch = JsonConvert.DeserializeObject<SaveChunk>(NetworkHelper.Receive(MainWindow._serverStream));
                foreach (Block bl in sch.Blocks)
                {
                    ch.Blocks.Add((bl.X, bl.Y, bl.Z), bl);
                }
                world.Chunks[(X, Z)] = ch;
            });
            return ch;
        }
        // old terrain gen function
        //public static int GetHeight(int x, int z) => (int)Math.Abs(((Math.Sin(Util.DegToRad(x)) * 25 + Math.Sin(Util.DegToRad(z)) * 10) * 1.2));// 2;
        public static int GetHeight(int x, int z)
        {
            //  return 0; // multiplayer performance testing
            int y = (int)Math.Max(Math.Floor(Math.Abs(((Math.Sin(Util.DegToRad(x)) * 25 + Math.Sin(Util.DegToRad(z)) * 10) * 1.2))), 0);
            return y;
        }
    }
}