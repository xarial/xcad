using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.SolidWorks;
using Xarial.XCad;

namespace SolidWorks.Tests
{
    public class SwVersionTest
    {
        [Test]
        public void EqualityTest() 
        {
            var v1 = SwApplicationFactory.CreateVersion(Xarial.XCad.SolidWorks.Enums.SwVersion_e.Sw2020);
            var v2 = SwApplicationFactory.CreateVersion(Xarial.XCad.SolidWorks.Enums.SwVersion_e.Sw2020);
            var v3 = SwApplicationFactory.CreateVersion(Xarial.XCad.SolidWorks.Enums.SwVersion_e.Sw2019);

            Assert.That(v1.Equals(v2));
            Assert.That(!v1.Equals(v3));
        }

        [Test]
        public void CompareTest()
        {
            var v1 = SwApplicationFactory.CreateVersion(Xarial.XCad.SolidWorks.Enums.SwVersion_e.Sw2020);
            var v2 = SwApplicationFactory.CreateVersion(Xarial.XCad.SolidWorks.Enums.SwVersion_e.Sw2020);
            var v3 = SwApplicationFactory.CreateVersion(Xarial.XCad.SolidWorks.Enums.SwVersion_e.Sw2019);

            Assert.AreEqual(0, v1.CompareTo(v2));
            Assert.AreEqual(1, v1.CompareTo(v3));
            Assert.AreEqual(-1, v3.CompareTo(v2));
        }

        [Test]
        public void CompareExtensionTest()
        {
            var v1 = SwApplicationFactory.CreateVersion(Xarial.XCad.SolidWorks.Enums.SwVersion_e.Sw2020);
            var v2 = SwApplicationFactory.CreateVersion(Xarial.XCad.SolidWorks.Enums.SwVersion_e.Sw2020);
            var v3 = SwApplicationFactory.CreateVersion(Xarial.XCad.SolidWorks.Enums.SwVersion_e.Sw2019);

            Assert.AreEqual(VersionEquality_e.Same, v1.Compare(v2));
            Assert.AreEqual(VersionEquality_e.Newer, v1.Compare(v3));
            Assert.AreEqual(VersionEquality_e.Older, v3.Compare(v2));
        }
    }
}
