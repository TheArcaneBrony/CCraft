using System;

using OpenTK;

namespace MCClone
{
    public class Player
    {
        public Player(double X, double Y, double Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public (double, double) pos { get => (X, Z); set { X = value.Item1; Z = value.Item2; } }
        public double YV { get; set; }
        public double LX { get; set; }
        public double LY { get; set; }
        public bool Flying { get; set; }
        public bool InAir { get; set; } = true;
        public Vector3 CPos { get; set; }
        public Vector3 CFPt { get; set; }
        public string Name { get; set; } = "Player_" + new Random().Next(0, 9999);
    }
}