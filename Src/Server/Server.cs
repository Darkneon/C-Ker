using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Cker
{
    public class Server
    {
        const string Comment = "//";
        const string NewTarget = "NEWT";
        const string StartTime = "STARTTIME";
        const string TimeStep = "TIMESTEP";
        const string Time = "TIME";
        const string Range = "RANGE";
        
        public enum TargetType
        {
            Human = 1,
            SpeedBoat,
            FishingBoat,
            CargoVessel,
            PassengerVessel                        
        }

        public class Simulator 
        {
            static public int StartTime { get; set; }
            static public int TimeStep { get; set; }
            static public int Time { get; set; }
            static public int Range { get; set; }
        }

        public class TargetRecord 
        {
            public int ID { get; set; }
            public TargetType Type { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public float VX_0 { get; set; }
            public float VY_0 { get; set; }
            public int StartTime { get; set; }

            public TargetRecord(string[] parameters) 
            {
                ID   = Convert.ToInt32(parameters[1]);
                Type = (TargetType)Enum.Parse(typeof(TargetType), parameters[2]);
                X    = Convert.ToInt32(parameters[3]);
                Y    = Convert.ToInt32(parameters[4]);
                VX_0 = Convert.ToSingle(parameters[5]);
                VY_0 = Convert.ToSingle(parameters[6]);
                StartTime = Convert.ToInt32(parameters[7]);
                
            }

            public override string ToString() 
            {
                return String.Format("{0} - {5} - ({1},{2}), ({3}, {4})", ID, X, Y, VX_0, VY_0, Type.ToString());
            }
        }

        public static List<TargetRecord> Parse(string path, string filename)
        {
            List<TargetRecord> result = new List<TargetRecord>();
            string filepath = Path.Combine(path, filename);

            foreach (string line in File.ReadLines(filepath)) 
            {

                if ( !string.IsNullOrWhiteSpace(line) && !line.TrimStart().StartsWith(Comment) ) 
                {
                    char[] delimiterChars = {' ', '\t'};
                    string[] fields = line.Split(delimiterChars);
                    fields = fields.Where(x => !string.IsNullOrEmpty(x)).ToArray();

                    switch (fields[0]) 
                    {
                        case StartTime:
                            Simulator.StartTime = Convert.ToInt32(fields[1]);
                            break;
                        case TimeStep:
                            Simulator.TimeStep = Convert.ToInt32(fields[1]);
                            break;
                        case Time:
                            Simulator.Time = Convert.ToInt32(fields[1]);
                            break;
                        case Range:
                            Simulator.Range = Convert.ToInt32(fields[1]);
                            break;
                        case NewTarget:
                            result.Add(new TargetRecord(fields));
                            break;
                        default:                           
                            break;
                    }                    
                }

            }

            return result;
        }
    }
}
