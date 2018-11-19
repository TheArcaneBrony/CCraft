using OpenTK;
using System;

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
        public int Xa { get; set; }
        public int Ya { get; set; }
        public int Za { get; set; }
        public float Xo { get; set; }
        public float Yo { get; set; }
        public float Zo { get; set; }
        public int Xc { get { return (int)this.Xa/16; } }
        public int Zc { get { return (int)this.Za / 16; } }
        public double XV { get; set; }
        public double YV { get; set; }
        public double ZV { get; set; }
        public double LX { get; set; }
        public double LY { get; set; }
        public bool Flying { get; set; }
        public bool InAir { get; set; }

        public Vector3 CPos { get; set; }
        public Vector3 CFPt { get; set; }
        public string Name { get; set; } = "Player_" + new Random().Next(0, 9999);
    }
}