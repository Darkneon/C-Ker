using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cker.Models;
using System.IO;

namespace Cker
{
    public class ScenarioParser
    {
        const string Comment = "//";
        const string NewTarget = "NEWT";
        const string StartTime = "STARTTIME";
        const string TimeStep = "TIMESTEP";
        const string Time = "TIME";
        const string Range = "RANGE";

        public class Simulator
        {
            static public float StartTime { get; set; }
            static public float TimeStep { get; set; }
            static public float Time { get; set; }
            static public int Range { get; set; }
        }
        


        public static List<Vessel> Parse(string path, string filename)
        {
            List<Vessel> result = new List<Vessel>();
            string filepath = Path.Combine(path, filename);

            foreach (string line in File.ReadLines(filepath))
            {
                result.AddRange(ParseText(line));
            }

            return result;
        }

        public static List<Vessel> ParseText(string text)
        {
            List<Vessel> result = new List<Vessel>();

            foreach (string line in text.Split('\n'))
            {
                Vessel record = ParseLine(line);
                if (record != null)
                {
                    result.Add(record);
                }

            }

            return result;
        }

        //-------------------------------------------------------------------
        // Private Methods
        //-------------------------------------------------------------------

        private static Vessel ParseLine(string line)
        {
            Vessel result = null;

            if (!string.IsNullOrWhiteSpace(line) && !line.TrimStart().StartsWith(Comment))
            {
                char[] delimiterChars = { ' ', '\t' };
                string[] fields = line.Split(delimiterChars);
                fields = fields.Where(x => !string.IsNullOrEmpty(x)).ToArray();

                switch (fields[0])
                {
                    case StartTime:
                        Simulator.StartTime = Convert.ToSingle(fields[1]);
                        break;
                    case TimeStep:
                        Simulator.TimeStep = Convert.ToSingle(fields[1]);
                        break;
                    case Time:
                        Simulator.Time = Convert.ToSingle(fields[1]);
                        break;
                    case Range:
                        Simulator.Range = Convert.ToInt32(fields[1]);
                        break;
                    case NewTarget:

                        result = new Vessel(fields);
                        break;
                    default:
                        break;
                }
            }

            return result;
        }
    }
}
