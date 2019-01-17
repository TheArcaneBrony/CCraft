using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MCClone
{
    class TerrainGen
    {
        public static bool GenLock = false;
        public static Stopwatch GenTime = new Stopwatch();
        public static int runningThreads = 0;
        public static int maxThreads = (int)(MainWindow.genDistance * MainWindow.renderDistance) * 4 * Environment.ProcessorCount;
        public static void GenTerrain(Dictionary<(int X, int Z), Chunk> chunkList)
        {
            // old initial gen code
            /*for (int x = -16; x < 16; x++)
            {
                for (int z = -16; z < 16; z++)
                {
                    GetChunk(chunkList, x, z);
                }
            }*/
            //GenLock = true;
            for (int x = (int)(MainWindow.world.Player.X / 16 - MainWindow.renderDistance); x < MainWindow.world.Player.X / 16 + MainWindow.renderDistance; x++)
                for (int z = (int)(MainWindow.world.Player.Z / 16 - MainWindow.renderDistance); z < MainWindow.world.Player.Z / 16 + MainWindow.renderDistance; z++)
                {
                    GetChunk(MainWindow.world.Chunks, x, z);
                }
            GenLock = false;
        }
        public static Random rnd = new Random();
        public static Chunk GenChunk(Dictionary<(int X, int Z), Chunk> chunkList, int X, int Z)
        {
            Chunk chunk = new Chunk(X, Z);
            chunkList.Add((X, Z), chunk);
            runningThreads++;
            while (runningThreads >= maxThreads) Thread.Sleep(50);
            Thread chunkGen = new Thread(() =>
            {
                Stopwatch GenTimer = new Stopwatch();
                GenTimer.Start();
                try
                {
                    for (byte x2 = 0; x2 < 0x10; x2++)
                    {
                        for (byte z2 = 0; z2 < 0x10; z2++)
                        {
                            int by = GetHeight(X * 16 + x2, Z * 16 + z2);
                            by = Math.Max(by, 0);
                            for (int y = by - 1; y < by; y++)
                            {
                                // chunk.Blocks.Add((x2,y,z2),new Block(x2, y, z2));
                                chunk.Blocks.Add(new Block(x2, y, z2));
                            }
                        }
                        //  Thread.Sleep(10);
                    }
                    try
                    {
                        //   Task.Run(() =>
                        //    {
                        //File.WriteAllText($"Worlds/{MainWindow.world.Name}/ChunkData/{X}.{Z}.gz", JsonConvert.SerializeObject(chunk));
                        //Thread.Sleep(20);
                        byte[] data = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(chunk));
                        //Thread.Sleep(20);
                        GZipStream chOut = new GZipStream(new FileStream($"Worlds/{MainWindow.world.Name}/ChunkData/{X}.{Z}.gz", FileMode.Create), CompressionLevel.Optimal);
                        //Thread.Sleep(20);
                        chOut.Write(data, 0, data.Length);
                        chOut.Close();
                        //     });
                    }
                    catch (IOException)
                    {
                        Console.WriteLine($"Failed to save chunk: {X}/{Z}");
                    }
                    catch
                    {
                    }
                    //   Task.Run(() => Logger.LogQueue.Add($"Chunk {X}/{Z} generated in: {GenTimer.ElapsedTicks / 10000d} ms"));
                    //Thread.Sleep(10);
                    Logger.LogQueue.Add($"Chunk {X}/{Z} generated in: {GenTimer.ElapsedTicks / 10000d} ms");
                }
                catch { }
                finally
                {
                }

                runningThreads--;
            })
            {
                Name = $"Chunk Gen {X}/{Z}",
                // Priority = ThreadPriority.Lowest
            };
            chunkGen.Start();
            //chunkGen.Join(25);
            return chunk;
        }
        public static Chunk GetChunk(Dictionary<(int X, int Z), Chunk> chunkList, int X, int Z)
        {
            GenTime.Restart();
            if (File.Exists($"Worlds/{MainWindow.world.Name}/ChunkData/{X}.{Z}.gz"))
            {
                //Chunk ch = JsonConvert.DeserializeObject<Chunk>(File.ReadAllText($"Worlds/{MainWindow.world.Name}/ChunkData/{X}.{Z}.gz"));
                uint length;
                Byte[] b = new byte[4];
                using (var fs = File.OpenRead($"Worlds/{MainWindow.world.Name}/ChunkData/{X}.{Z}.gz"))
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
                chunkList.Add((X, Z), ch);
                Logger.LogQueue.Add($"Loaded chunk {X}/{Z} in: {GenTime.ElapsedTicks / 10000d} ms");
                // Thread.Sleep(10);
                data = null;
                return ch;
            }
            else
            {
                // Logger.LogQueue.Add($"Generating chunk {X}/{Z}");
                var ch = GenChunk(chunkList, X, Z);
                //Logger.LogQueue.Add($"Chunk {X}/{Z} generated in: {GenTime.ElapsedTicks / 10000d} ms");
                return ch;
            }
        }
        // old terrain gen function
        //public static int GetHeight(int x, int z) => (int)Math.Abs(((Math.Sin(Util.DegToRad(x)) * 25 + Math.Sin(Util.DegToRad(z)) * 10) * 1.2));// 2;
        public static int GetHeight(int x, int z)
        {
            int y = (int)Math.Abs(((Math.Sin(Util.DegToRad(x)) * 25 + Math.Sin(Util.DegToRad(z)) * 10) * 1.2));
            return y;
        }
    }
}