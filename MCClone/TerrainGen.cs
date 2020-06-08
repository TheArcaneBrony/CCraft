using Newtonsoft.Json;

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace MCClone
{
    public static class TerrainGen
    {
        public static World world = new World(0, 20, 0)
        {
            Name = "Test"
        };
        private static bool ShouldLoadChunks = true;
        public static Stopwatch GenTime = new Stopwatch();
        public static double renderDistance = 4, genDistance = 1;
        public static void Initialize(double distance)
        {
            Directory.CreateDirectory($"Worlds/{world.Name}/ChunkData/");
            /*for (int d = 0; d < renderDistance * genDistance; d++)
                for (int x = -d; x <= d; x++)
                {
                    for (int z = -d; z <= d; z++)
                    {
                        //(int, int) pos = ((int)(world.Player.X / 16 + x * Math.Sin(Util.DegToRad(z))), (int)(world.Player.Z / 16 + x * Math.Cos(Util.DegToRad(z))));
                        (int, int) pos = ((int)(world.Player.X / 16 + x), (int)(world.Player.Z / 16 + z));
                        if (!world.Chunks.ContainsKey(pos))
                            GetChunk(pos.Item1, pos.Item2);
                    }
                }*/
            for (int x = -(int)distance; x <= distance; x++)
            {
                for (int z = -(int)distance; z <= (int)distance; z++)
                {
                    //(int, int) pos = ((int)(world.Player.X / 16 + x * Math.Sin(Util.DegToRad(z))), (int)(world.Player.Z / 16 + x * Math.Cos(Util.DegToRad(z))));
                    (int, int) pos = ((int)(world.Player.X / 16 + x), (int)(world.Player.Z / 16 + z));
                    if (!world.Chunks.ContainsKey(pos))
                        new Task(() => { GetChunk(pos.Item1, pos.Item2); }).Start();
                        
                }
            }
            Logger.CompressLog();

        }
        public static void GenTerrain()
        {
            Initialize(renderDistance);
        }
        public static Chunk GenChunk(int X, int Z)
        {
            Chunk chunk = new Chunk(X, Z);
            if (world.Chunks.ContainsKey((X, Z)))
            {
                world.Chunks.TryGetValue((X, Z), out chunk);
                return chunk;
            }
            Stopwatch GenTimer = new Stopwatch();

            GenTimer.Start();
            /*for (byte pos = 0; pos <= 255; pos++)
            {
                byte y = GetHeight(X * 16 + pos % 16, Z * 16 + pos / 16);
                chunk.Blocks.AddOrUpdate(((byte)(pos % 16), y, (byte)(pos / 16)), new Block((byte)(pos % 16), y, (byte)(pos / 16)), (_, _1) => new Block((byte)(pos % 16), y, (byte)(pos / 16)));
            }*/
            for (byte x = 0; x < 16; x++)
            {
                for (byte z = 0; z < 16; z++)
                {
                    byte y = GetHeight(X * 16 + x, Z * 16 + z);
                    chunk.Blocks.AddOrUpdate((x, y, z), new Block(x, y, z), (_, _1) => new Block(x, y, z));
                }
            }
            chunk.Finished = true;
            LogEntry += $"Chunk {X}/{Z} generated in: {GenTimer.ElapsedTicks / 10000d} ms, {chunk.Blocks.Count} blocks.\n";
            Task.Run(() =>
            {
                chunk.Save();
                LogEntry += $"Chunk {X}/{Z} saved in: {GenTimer.ElapsedTicks / 10000d} ms, {chunk.Blocks.Count} blocks.\n";
            });

            return chunk;
        }
        static String LogEntry = "";
        public static Chunk GetChunk(int X, int Z)
        {
            LogEntry = "";
            GenTime.Restart();
            LogEntry += $"Getting chunk at {X}/{Z}\n";
            Chunk ch = new Chunk(X, Z);
            bool AlreadyLoaded = world.Chunks.TryGetValue((X, Z), out ch);
            if (AlreadyLoaded)
            {
                return ch;
            }
            
            
            if (ShouldLoadChunks && File.Exists($"Worlds/{world.Name}/ChunkData/{X}.{Z}.gz"))
            {
                LogEntry += $"Found chunk file for {X}/{Z}, loading...\n";
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
                    ch.Blocks.AddOrUpdate((bl.X, bl.Y, bl.Z), bl, (_, _1) => bl);
                }
                LogEntry += $"Loaded chunk {X}/{Z} in: {GenTime.ElapsedTicks / 10000d} ms\n";
            }
            else
            {
                LogEntry += $"Generating chunk at {X}/{Z}...\n";
                ch = GenChunk(X, Z);
            }

            try
            {
                LogEntry += $"Adding chunk at {X}/{Z} to world.";
                world.Chunks.AddOrUpdate((X, Z), ch, (_, _1) => ch);
            }
            catch { }
            Logger.Log("Terrain Gen", LogEntry);
            return ch;
        }
        public static byte GetHeight(int x, int z)
        {
            //return 0; // multiplayer performance testing
            //return (byte)Util.TruncateHeight((int)Math.Floor(Math.Abs(((Math.Sin(Util.DegToRad(x)) * 25 + Math.Sin(Util.DegToRad(z)) * 10) * 1.2))));
            return (byte)Util.TruncateHeight((int)Math.Floor((Math.Sin(Util.DegToRad(x))+Math.Cos(Util.DegToRad(z)))*15));
        }
    }
}