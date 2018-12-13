using System;
using System.Collections.Generic;
using System.IO;

namespace MCClone
{
    public class Block
    {
        public Block(byte X, UInt16 Y, byte Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }
        public byte X { get; set; }
        public UInt16 Y { get; set; }
        public byte Z { get; set; }
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
        public List<Block> Blocks { get; set; } = new List<Block>();
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
        [Newtonsoft.Json.JsonIgnore]
        public string Name { get; set; } = "SP_DEV_" + Directory.GetDirectories("Worlds/").Length; //"SP_DEV";
        public double SpawnX { get; set; } = 0;
        public double SpawnY { get; set; } = 10;
        public double SpawnZ { get; set; } = 0;
        public Player Player { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public List<Chunk> Chunks { get; set; } = new List<Chunk>();
    }
}