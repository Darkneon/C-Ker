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
        public float X { get; set; }
        public float Y { get; set; }
        public float VX_0 { get; set; }
        public float VY_0 { get; set; }
        public float StartTime { get; set; }

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

        public void UpdatePositions(int currentTime)
        {
            float timespan = currentTime - StartTime;
            X = X + VX_0 * timespan;
            Y = Y + VY_0 * timespan;
        }

        public Vessel(string[] parameters)
        {
            ID = Convert.ToInt32(parameters[1]);
            Type = (TargetType)Enum.Parse(typeof(TargetType), parameters[2]);
            X = Convert.ToSingle(parameters[3]);
            Y = Convert.ToSingle(parameters[4]);
            VX_0 = Convert.ToSingle(parameters[5]);
            VY_0 = Convert.ToSingle(parameters[6]);
            StartTime = Convert.ToSingle(parameters[7]);
        }

        public override string ToString()
        {
            return String.Format("{0} - {5} - ({1},{2}), ({3}, {4})", ID, X, Y, VX_0, VY_0, Type.ToString());
        }
    }
}
