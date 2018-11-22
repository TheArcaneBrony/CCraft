using System;
using System.Collections.Generic;

namespace MCClone
{
    public class Block
    {
        public Block(int X, int Y, int Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
    }
    public class Chunk
    {
        public Chunk() { }
        public Chunk(int X, int Z)
        {
            this.X = X;
            this.Z = Z;
        }
        public Chunk(int X, int Z, List<Block> Blocks)
        {
            this.X = X;
            this.Z = Z;
            this.Blocks = Blocks;
        }
        public int X { get; set; }
        public int Z { get; set; }
        public List<Block> Blocks { get; set; }
    }

    public class World
    {
        public World() { }
        public World(double SpawnX, double SpawnY, double SpawnZ)
        {
            this.SpawnX = SpawnX;
            this.SpawnY = SpawnY;
            this.SpawnZ = SpawnZ;
        }
        public string Name { get; set; } = "SP_DEV";
        public double SpawnX { get; set; } = 0;
        public double SpawnY { get; set; } = 10;
        public double SpawnZ { get; set; } = 0;
        public Player Player { get; set; } = new Player();
        public List<Player> OnlinePlayers { get; set; } = new List<Player>();
        public List<Chunk> Chunks { get; set; } = new List<Chunk>();
        public int BlockCount { get; set; } = 0;
    }
}