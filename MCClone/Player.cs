using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCClone
{
    public class Player
    {
        public Player() { }
        public Player(double X, double Y, double Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double XV { get; set; }
        public double YV { get; set; }
        public double ZV { get; set; }
        public double LX { get; set; }
        public double LY { get; set; }
        public bool Flying { get; set; }
        public Vector3 CPos { get; set; }
        public Vector3 CFPt { get; set; }
        public string Name { get; set; }
    }
}
