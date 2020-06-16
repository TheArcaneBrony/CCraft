using OpenTK;
using System;

namespace MCClone
{
    public class Player
    {
        public Player(World world)
        {
            CurrentWorld = world;
        }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public (double, double) pos { get => (X, Z); set { X = value.Item1; Z = value.Item2; } }
        public double YV { get; set; }
        public double LX { get; set; }
        public double LY { get; set; }
        public int DimensionId { get; set; } = 0;
        private Dimension _CurrentDimension;
        public World CurrentWorld;
        public Dimension CurrentDimension
        {
            get
            {
                if (_CurrentDimension != null && _CurrentDimension.Id == DimensionId)
                    return _CurrentDimension;
                else return CurrentWorld.Dimensions[DimensionId];
            }
            set
            {
                DimensionId = _CurrentDimension.Id;
                _CurrentDimension = value;
            }
        }
        public bool Flying { get; set; }
        public bool InAir { get; set; } = true;
        public Vector3 CPos { get; set; }
        public Vector3 CFPt { get; set; }
        public string Name { get; set; } = "Player_" + new Random().Next(0, 9999);
    }
}