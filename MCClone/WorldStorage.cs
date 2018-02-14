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
        public Chunk() { Blocks = new List<Block>();  }
        public Chunk(double X, double Z)
        {
            this.X = X;
            this.Z = Z;
            this.Blocks = new List<Block>();
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
}