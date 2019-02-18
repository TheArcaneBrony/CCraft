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
        public static Stopwatch GenTime = new Stopwatch();
        public static int runningThreads = 0;
        public static int maxThreads = (int)Math.Pow(MainWindow.genDistance * MainWindow.renderDistance, 2) * Environment.ProcessorCount;
        public static void GenTerrain()
        {
            // old initial gen code
            /*for (int x = -16; x < 16; x++)
            {
                for (int z = -16; z < 16; z++)
                {
                    GetChunk(chunkList, x, z);
                }
            }*/

            GetChunk((int)MainWindow.world.Player.X / 16, (int)MainWindow.world.Player.Z / 16);
            //for (int x = (int)(MainWindow.world.Player.X / 16 - MainWindow.renderDistance); x < MainWindow.world.Player.X / 16 + MainWindow.renderDistance; x++)
            //  for (int z = (int)(MainWindow.world.Player.Z / 16 - MainWindow.renderDistance); z < MainWindow.world.Player.Z / 16 + MainWindow.renderDistance; z++)
            for (int x = 0; x < MainWindow.renderDistance; x++)
            {
                //GetChunk(x, z);
                for (int y = -x; y < x; y++)
                {
                    // infront and behind
                    GetChunk((int)MainWindow.world.Player.X / 16 + x, (int)MainWindow.world.Player.Z / 16 - y);
                    GetChunk((int)MainWindow.world.Player.X / 16 - x, (int)MainWindow.world.Player.Z / 16 - y);
                    GetChunk((int)MainWindow.world.Player.X / 16 + x, (int)MainWindow.world.Player.Z / 16 + y);
                    GetChunk((int)MainWindow.world.Player.X / 16 - x, (int)MainWindow.world.Player.Z / 16 + y);
                    // left and right
                    GetChunk((int)MainWindow.world.Player.X / 16 + y, (int)MainWindow.world.Player.Z / 16 - x);
                    GetChunk((int)MainWindow.world.Player.X / 16 - y, (int)MainWindow.world.Player.Z / 16 - x);
                    GetChunk((int)MainWindow.world.Player.X / 16 + y, (int)MainWindow.world.Player.Z / 16 + x);
                    GetChunk((int)MainWindow.world.Player.X / 16 - y, (int)MainWindow.world.Player.Z / 16 + x);
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
            runningThreads++;
            Thread chunkGen = new Thread(() =>
            {
                while (runningThreads >= maxThreads)
                {
                    Thread.Sleep(10);
                }

                Stopwatch GenTimer = new Stopwatch();
                GenTimer.Start();
                try
                {
                    for (int x2 = 0; x2 < 16; x2++)
                    {
                        for (int z2 = 0; z2 < 16; z2++)
                        {
                            int by = GetHeight(X * 16 + x2, Z * 16 + z2);
                            by = Math.Max(by, 0);
                            for (int y = by; y <= by; y++)
                            {
                             //   Thread.Sleep(100);
                                chunk.Blocks.Add((x2, y, z2), new Block(x2, y, z2));
                            }
                        }
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
        public static Chunk GetChunk(int X, int Z)
        {
            GenTime.Restart();
            if (File.Exists($"Worlds/{MainWindow.world.Name}/ChunkData/{X}.{Z}.gz"))
            {
                return GenChunk(X, Z);
                //Chunk ch = JsonConvert.DeserializeObject<Chunk>(File.ReadAllText($"Worlds/{MainWindow.world.Name}/ChunkData/{X}.{Z}.gz"));
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
                //Chunk ch = (Chunk)Util.deserializeToDictionaryOrList(Encoding.ASCII.GetString(data));
                ch.X = X; ch.Z = Z;
                MainWindow.world.Chunks.Add((X, Z), ch);
                Logger.LogQueue.Add($"Loaded chunk {X}/{Z} in: {GenTime.ElapsedTicks / 10000d} ms");
                // Thread.Sleep(10);
                data = null;
                return ch;
            }
            else
            {
                // Logger.LogQueue.Add($"Generating chunk {X}/{Z}");
                Chunk ch = GenChunk(X, Z);
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