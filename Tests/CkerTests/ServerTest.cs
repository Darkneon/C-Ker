using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;
using Cker;

namespace CKerTests
{
    public class ServerTest
    {
        private class TargetRecordComparer : IEqualityComparer<Server.TargetRecord>
        {
            public bool Equals(Server.TargetRecord t1, Server.TargetRecord t2)
            {
                return (t1.ID   == t2.ID)   &&
                       (t1.Type == t2.Type) &&
                       (t1.VX_0 == t2.VX_0) &&
                       (t1.VY_0 == t2.VY_0) &&
                       (t1.X    == t2.X)    &&
                       (t1.Y    == t2.Y);
            }

            public int GetHashCode(Server.TargetRecord obj)
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void ParseText_EmptyText_ReturnEmptyList()
        {
            string text = "\n";
            List<Server.TargetRecord> actual = Server.ParseText(text);
            List<Server.TargetRecord> expected = new List<Server.TargetRecord>();            

            Assert.AreEqual(expected.Count, actual.Count);
        }

        [Test]
        public void ParseText_ValidLine_ReturnListWithOneTargetRecordObject() 
        {
            string text = "NEWT	 001	  		1			4990           		0        		0.1			   	    0.1  		 10\n";
            List<Server.TargetRecord> actual = Server.ParseText(text);
            List<Server.TargetRecord> expected = new List<Server.TargetRecord>();
            expected.Add(new Server.TargetRecord(new string[] {"NEWT", "1", "1", "4990", "0",	"0.1", "0.1", "10" }));

            Assert.AreEqual(expected.Count, actual.Count);

            for (int i = 0; i != expected.Count; i++)
            {
                Assert.IsTrue(new TargetRecordComparer().Equals(expected[i], actual[i]));
            }
        }
    }
}
