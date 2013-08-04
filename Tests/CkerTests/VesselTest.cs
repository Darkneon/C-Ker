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
    class VesselTest
    {
        [Test]
        public void GetDistanceBetween_TwoVessels_ReturnsEmptyList()
        {
            Vessel v1 = new Vessel(1, Vessel.TargetType.Human, 7.83, 105.84, 1, 1, 1);
            Vessel v2 = new Vessel(2, Vessel.TargetType.Human, 42.92, 66.28, 1, 1, 1);

            double exected = Math.Sqrt(Math.Pow(42.92 - 7.83, 2) + Math.Pow(105.84 - 66.28, 2));
            double actual = v1.GetDistanceBetween(v2);
            Assert.AreEqual(exected, actual, double.Epsilon);
        }
    }
}
