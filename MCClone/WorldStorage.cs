using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MCClone
{
    public class Chunk
    {
        public Chunk() { }
        public Chunk(double X, double Z)
        {
            this.X = X;
            this.Z = Z;
        }
        public Chunk(double X, double Z, List<Block> Blocks)
        {
            this.X = X;
            this.Z = Z;
            this.Blocks = Blocks;
        }
        public double X { get; set; }
        public double Z { get; set; }
        public double XS { get; set; }
        public double ZS { get; set; }
        public List<Block> Blocks { get; set; }
        public Dictionary<Double[],Block> Blocks2 { get; set; }
    }
    public class Block
    {
        public Block() { }
        public Block(double X, double Y, double Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
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
        public List<Player> onlinePlayers { get; set; } = new List<Player>();
        public List<Chunk> Chunks { get; set; } = new List<Chunk>();
        
        public int BlockCount { get; set; } = 0;
    }
}