using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cker.Models
{
    public class Vessel
    {
        public enum TargetType
        {
            Human = 1,
            SpeedBoat,
            FishingBoat,
            CargoVessel,
            PassengerVessel
        }

        public int ID { get; set; }
        public TargetType Type { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public float VX_0 { get; set; }
        public float VY_0 { get; set; }
        public int StartTime { get; set; }

        public Vessel(int id, TargetType type, int x, int y, float vx_0, float vy_0, int startTime)
        {
            ID = id;
            Type = type;
            X = x;
            Y = y;
            VX_0 = vx_0;
            VY_0 = vy_0;
            StartTime = startTime;
        }

        public override string ToString()
        {
            return String.Format("{0} - {5} - ({1},{2}), ({3}, {4})", ID, X, Y, VX_0, VY_0, Type.ToString());
        }
    }
}
