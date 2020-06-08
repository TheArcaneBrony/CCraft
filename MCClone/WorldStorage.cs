using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace MCClone
{
    public class Block : ICollidable
    {
        public Block(byte X, byte Y, byte Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }
        public byte X { get; set; }
        public byte Y { get; set; }
        public byte Z { get; set; }
        public byte Type { get; set; }
        double ICollidable.Width { get => 1; set => value=1; }
        double ICollidable.Height{ get => 1; set => value = 1; }

    }
    public class Chunk
    {
        public Chunk(int X, int Z)
        {
            this.X = X;
            this.Z = Z;
        }
        [Newtonsoft.Json.JsonIgnore]
        public int X { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public int Z { get; set; }
        public bool Finished { get; set; } = false;
        public ConcurrentDictionary<(byte X, byte Y, byte Z), Block> Blocks { get; } = new ConcurrentDictionary<(byte X, byte Y, byte Z), Block>();
        public void Save()
        {
            try
            {
                byte[] data = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(new SaveChunk(this)));
                GZipStream chOut = new GZipStream(new FileStream($"Worlds/{MainWindow.world.Name}/ChunkData/{X}.{Z}.gz", FileMode.Create), CompressionLevel.Optimal);
                chOut.Write(data, 0, data.Length);
                chOut.Close();
            }
            catch (IOException)
            {
                Logger.Log("World Storage", $"Failed to save chunk: {X}/{Z}");
            }
            catch
            {
                throw;
            }
        }
    }
    public class SaveChunk
    {
        public SaveChunk(Chunk chunk)
        {
            if (chunk != null && chunk.Blocks != null)
                Blocks = new List<Block>(chunk.Blocks.Values);
        }
        public List<Block> Blocks { get; } = new List<Block>();
    }
    public class World
    {
        public World(double SpawnX, double SpawnY, double SpawnZ)
        {
            this.SpawnX = SpawnX;
            this.SpawnY = SpawnY;
            this.SpawnZ = SpawnZ;
            Player = new Player(SpawnX, SpawnY, SpawnZ);
        }
        [JsonIgnore]
        public string Name { get; set; } = "SP_DEV_" + Directory.GetDirectories("Worlds/").Length; //"SP_DEV";
        public double SpawnX { get; set; } = 0;
        public double SpawnY { get; set; } = 10;
        public double SpawnZ { get; set; } = 0;
        public Player Player { get; set; }
        [JsonIgnore]
        public ConcurrentDictionary<(int X, int Z), Chunk> Chunks { get; } = new ConcurrentDictionary<(int X, int Z), Chunk>();
    }
}