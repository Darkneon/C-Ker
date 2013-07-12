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
            List<Vessel> actual = Parser.ParseText(text);
            List<Vessel> expected = new List<Vessel>();            

            Assert.AreEqual(expected.Count, actual.Count);
        }

        [Test]
        public void ParseText_ValidLine_ReturnsListWithOneVesselObject() 
        {
            string text = "NEWT	 001	  		1			4990           		0        		0.1			   	    0.1  		 10\n";
            List<Vessel> actual = Parser.ParseText(text);
            List<Vessel> expected = new List<Vessel>();
            expected.Add(new Vessel(new string[] { "NEWT", "1", "1", "4990", "0", "0.1", "0.1", "10" }));

            Assert.AreEqual(expected.Count, actual.Count);

            for (int i = 0; i != expected.Count; i++)
            {
                Assert.IsTrue(new VesselComparer().Equals(expected[i], actual[i]));
            }
        }

        [Test]
        public void ParseText_InvalidLineMissing_ReturnsEmpty()
        {
            string text = "001	  		1			4990           		0        		0.1			   	    0.1  		 10\n";
            //Assert.Throws<Exception>(() => Parser.ParseText(text));                        
            List<Vessel> actual   = Parser.ParseText(text);
            int expected = 0;

            Assert.AreEqual(expected, actual.Count);
        }
    }
}
