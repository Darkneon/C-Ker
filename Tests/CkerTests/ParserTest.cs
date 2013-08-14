using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;
using Cker;
using Cker.Models;

namespace CKerTests
{
    public class ParserTest
    {
        private class VesselComparer : IEqualityComparer<Vessel>
        {
            public bool Equals(Vessel t1, Vessel t2)
            {
                return (t1.ID   == t2.ID)   &&
                       (t1.Type == t2.Type) &&
                       (t1.VX_0 == t2.VX_0) &&
                       (t1.VY_0 == t2.VY_0) &&
                       (t1.X    == t2.X)    &&
                       (t1.Y    == t2.Y);
            }

            public int GetHashCode(Vessel obj)
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void ParseText_EmptyText_ReturnsEmptyList()
        {
            string text = "\n";
            List<Vessel> actual = ScenarioParser.ParseText(text);
            List<Vessel> expected = new List<Vessel>();            

            Assert.AreEqual(expected.Count, actual.Count);
        }

        [Test]
        public void ParseText_ValidLine_ReturnsListWithOneVesselObject() 
        {
            string text = "NEWT     001	  		1			4990           		0        		0.1			   	    0.1  		 10\n";
            List<Vessel> actual = ScenarioParser.ParseText(text);
            List<Vessel> expected = new List<Vessel>();
            expected.Add(new Vessel(new string[] { "NEWT", "1", "1", "4990", "0", "0.1", "0.1", "10" }));

            Assert.AreEqual(expected.Count, actual.Count);

            for (int i = 0; i != expected.Count; i++)
            {
                Assert.IsTrue(new VesselComparer().Equals(expected[i], actual[i]));
            }
        }

        [Test]
        public void ParseText_InvalidLineMissing_ReturnsZero()
        {
            string text = "001	  		1			4990           		0        		0.1			   	    0.1  		 10\n";                               
            List<Vessel> actual   = ScenarioParser.ParseText(text);
            int expected = 0;

            Assert.AreEqual(expected, actual.Count);
        }

        [Test]
        public void ParseText_InvalidLineValue_ThrowsFormatException()
        {            
            Assert.Throws<System.FormatException>(            
                delegate 
                {
                    string text = "NEWT 001	  		1			4990           		a        		0.1			   	    0.1  		 10\n";
                    ScenarioParser.ParseText(text);                
                }
            );            
        }

        [Test]
        public void ParseText_InvalidLineKeyword_ReturnsZero()
        {
            string text = "NOTEXISTING 001	  	1		4990           		0        		0.1			   	    0.1  		 10\n";
            List<Vessel> actual = ScenarioParser.ParseText(text);
            int expected = 0;

            Assert.AreEqual(expected, actual.Count);
        }


        [Test]
        public void ParseText_ValidSimulationIntegerDetails_ReturnsFilledScenarionParser_Simulator()
        {
            string text = "STARTTIME 0\n" +
                          "TIMESTEP 1\n" +
                          "TIME 500\n" +
                          "RANGE 500\n" ;

            ScenarioParser.ParseText(text);

            float expectedStartTime = 0f;
            float expectedTimeStep = 1f;
            float expectedTime = 500f;
            int expectedRange = 500;

            Assert.AreEqual(expectedStartTime, ScenarioParser.Simulator.StartTime, float.Epsilon);
            Assert.AreEqual(expectedTimeStep, ScenarioParser.Simulator.TimeStep, float.Epsilon);
            Assert.AreEqual(expectedTime, ScenarioParser.Simulator.Time, float.Epsilon);
            Assert.AreEqual(expectedRange, ScenarioParser.Simulator.Range);
        }

        [Test]
        public void ParseText_ValidSimulationFloatDetails_ReturnsFilledScenarionParser_Simulator()
        {
            string text = "STARTTIME 0.3\n" +
                          "TIMESTEP 1.1\n" +
                          "TIME 500.5\n" +
                          "RANGE 500\n";

            ScenarioParser.ParseText(text);

            float expectedStartTime = 0.3f;
            float expectedTimeStep = 1.1f;
            float expectedTime = 500.5f;
            
            Assert.AreEqual(expectedStartTime, ScenarioParser.Simulator.StartTime, float.Epsilon);
            Assert.AreEqual(expectedTimeStep, ScenarioParser.Simulator.TimeStep, float.Epsilon);
            Assert.AreEqual(expectedTime, ScenarioParser.Simulator.Time, float.Epsilon);
            
        }

        [Test]
        public void ParseText_InvalidSimulationDetails_ThrowsFormatException()
        {
            string text = "STARTTIME 0\n" +
                          "TIMESTEP A\n" +
                          "TIME B\n" +
                          "RANGE 500\n";

            Assert.Throws<System.FormatException>(
                delegate
                {
                    ScenarioParser.ParseText(text);
                }
            );  
        }
    }
}
