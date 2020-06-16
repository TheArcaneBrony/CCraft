using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
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
        public Dimension dim;
        public Chunk(Dimension dim, int X, int Z)
        {
            this.X = X;
            this.Z = Z;
            this.dim = dim;
        }
        [JsonIgnore]
        public int X { get; set; }
        [JsonIgnore]
        public int Z { get; set; }
        public bool Finished { get; set; } = false;
        /*[JsonIgnore]*/
        public bool Dirty { get; set; } = false;
        public ConcurrentDictionary<(byte X, byte Y, byte Z), Block> Blocks { get; } = new ConcurrentDictionary<(byte X, byte Y, byte Z), Block>();
        public void Save()
        {
            try
            {
                Directory.CreateDirectory($"Worlds/{dim.World.Name}/{dim.Name}/ChunkData/");
                byte[] data = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(new SaveChunk(this)));
                GZipStream chOut = new GZipStream(new FileStream($"Worlds/{dim.World.Name}/{dim.Name}/ChunkData/{X}.{Z}.gz", FileMode.Create), dim.World.CompressionLevel);
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
            for (int i = 0; i < 32; i++)
            {
                Dimensions.Add(i, new Dimension(this, i));
            }
        }
        public void Save()
        {
            Directory.CreateDirectory($"Worlds/{Name}");
            foreach (KeyValuePair<int,Dimension> dim in Dimensions.Where((a) => { return a.Value.Dirty; }))
            {
                dim.Value.Save();
            }
            File.WriteAllText($"Worlds/{Name}/World.json", JsonConvert.SerializeObject(this, Formatting.Indented));
        }
        

        public int ChunkCount { get { int a = 0; foreach (KeyValuePair<int,Dimension> d in Dimensions) a += d.Value.Chunks.Count; return a; } }
        [JsonIgnore]
        public string Name { get; set; } = "" + Directory.GetDirectories("Worlds/").Length; //"SP_DEV";
        
        public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.Optimal;
        /*[JsonIgnore]*/
        public Dictionary<int, Dimension> Dimensions { get; } = new Dictionary<int, Dimension>();
    }
    public class Dimension
    {
        /// <summary>
        /// Parent World object
        /// </summary>
        public World World;
        /// <summary>
        /// Dimension ID
        /// </summary>
        public int Id;
        public Dimension(World w, int id)
        {
            this.World = w;
            this.Id = id;
            Name = "DIM" + id;
        }
        /*[JsonIgnore]*/
        public string Name { get; set; } = "DIM";
        public double SpawnX { get; set; } = 0;
        public double SpawnY { get; set; } = 10;
        public double SpawnZ { get; set; } = 0;
        /*[JsonIgnore]*/
        public bool Dirty { get; set; } = false;
        public bool AnyChunksDirty { get { return Dirty || Chunks.Any((a)=> { return a.Value.Dirty; }); } }
        /*[JsonIgnore]*/
        public ConcurrentDictionary<(int X, int Z), Chunk> Chunks { get; } = new ConcurrentDictionary<(int X, int Z), Chunk>();
        public void Save()
        {
            Directory.CreateDirectory($"Worlds/{World.Name}/{Name}");
            foreach (KeyValuePair<(int X, int Z), Chunk> chunk in Chunks.Where((a)=> { return a.Value.Dirty; }))
            {
                chunk.Value.Save();
            }
            File.WriteAllText($"Worlds/{World.Name}/{Name}/Dimension.json", JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}