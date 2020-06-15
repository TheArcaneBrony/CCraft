using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace MCClone
{
    public class Block
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
                GZipStream chOut = new GZipStream(new FileStream($"Worlds/{MainWindow.world.Name}/ChunkData/{X}.{Z}.gz", FileMode.Create), MainWindow.world.CompressionLevel);
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
        public World(string Name)
        {
            for (int i = 0; i < 3; i++)
            {
                Dimensions.Add(new Dimension(this, i));
            }
        }
        public void Save()
        {
            foreach (Dimension dim in Dimensions)
            {
                dim.Save();
            }
        }
        

        public int ChunkCount { get { int a = 0; foreach (Dimension d in Dimensions) a += d.Chunks.Count; return a; } }
        [JsonIgnore]
        public string Name { get; set; } = "" + Directory.GetDirectories("Worlds/").Length; //"SP_DEV";
        
        public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.Optimal;
        
        public List<Dimension> Dimensions = new List<Dimension>();
    }
    public class Dimension
    {
        private World w;
        private int id;
        public Dimension(World w, int id)
        {
            this.w = w;
            this.id = id;
            Name = "DIM" + id;
        }
        [JsonIgnore]
        public string Name { get; set; } = "DIM";
        public double SpawnX { get; set; } = 0;
        public double SpawnY { get; set; } = 10;
        public double SpawnZ { get; set; } = 0;
        [JsonIgnore]
        public ConcurrentDictionary<(int X, int Z), Chunk> Chunks { get; } = new ConcurrentDictionary<(int X, int Z), Chunk>();
        public void Save()
        {

            foreach (KeyValuePair<(int X, int Z), Chunk> chunk in Chunks)
            {
                chunk.Value.Save();
            }
            File.WriteAllText($"Worlds/{Name}/World.json", JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}