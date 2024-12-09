using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.SolidWorks;
using Xarial.XCad;
using SolidWorks.Interop.sldworks;
using Moq;
using Xarial.XCad.SolidWorks.Enums;
using Xarial.XCad.SolidWorks.Services;
using Microsoft.Win32;

namespace SolidWorks.Tests
{
    public class SwVersionTest
    {
        [Test]
        public void IsVersionNewerOrEqualTest()
        {
            var sw2017sp23Mock = new Mock<SldWorks>();
            sw2017sp23Mock.Setup(m => m.RevisionNumber()).Returns("25.2.3");
            var app = SwApplicationFactory.FromPointer(sw2017sp23Mock.Object);

            var r1 = app.IsVersionNewerOrEqual(SwVersion_e.Sw2017);
            var r2 = app.IsVersionNewerOrEqual(SwVersion_e.Sw2017, 2);
            var r3 = app.IsVersionNewerOrEqual(SwVersion_e.Sw2017, 2, 3);
            var r4 = app.IsVersionNewerOrEqual(SwVersion_e.Sw2017, 3);
            var r5 = app.IsVersionNewerOrEqual(SwVersion_e.Sw2017, 1);
            var r6 = app.IsVersionNewerOrEqual(SwVersion_e.Sw2016, 4);
            var r7 = app.IsVersionNewerOrEqual(SwVersion_e.Sw2018, 0);

            Assert.IsTrue(r1);
            Assert.IsTrue(r2);
            Assert.IsTrue(r3);
            Assert.IsFalse(r4);
            Assert.IsTrue(r5);
            Assert.IsTrue(r6);
            Assert.IsFalse(r7);
            Assert.Throws<ArgumentException>(() => app.IsVersionNewerOrEqual(SwVersion_e.Sw2017, null, 1));
        }

        [Test]
        public void EqualityTest() 
        {
            var v1 = SwApplicationFactory.CreateVersion(SwVersion_e.Sw2020);
            var v2 = SwApplicationFactory.CreateVersion(SwVersion_e.Sw2020);
            var v3 = SwApplicationFactory.CreateVersion(SwVersion_e.Sw2019);

            Assert.That(v1.Equals(v2));
            Assert.That(!v1.Equals(v3));
        }

        [Test]
        public void CompareTest()
        {
            var v1 = SwApplicationFactory.CreateVersion(SwVersion_e.Sw2020);
            var v2 = SwApplicationFactory.CreateVersion(SwVersion_e.Sw2020);
            var v3 = SwApplicationFactory.CreateVersion(SwVersion_e.Sw2019);

            Assert.AreEqual(0, v1.CompareTo(v2));
            Assert.AreEqual(1, v1.CompareTo(v3));
            Assert.AreEqual(-1, v3.CompareTo(v2));
        }

        [Test]
        public void CompareExtensionTest()
        {
            var v1 = SwApplicationFactory.CreateVersion(SwVersion_e.Sw2020);
            var v2 = SwApplicationFactory.CreateVersion(SwVersion_e.Sw2020);
            var v3 = SwApplicationFactory.CreateVersion(SwVersion_e.Sw2019);

            Assert.AreEqual(VersionEquality_e.Same, v1.Compare(v2));
            Assert.AreEqual(VersionEquality_e.Newer, v1.Compare(v3));
            Assert.AreEqual(VersionEquality_e.Older, v3.Compare(v2));
        }

        [Test]
        public void VersionMapperTest() 
        {
            var mapper = new SwVersionMapper();

            var av1 = mapper.FromApplicationRevision(20);
            var av2 = mapper.FromApplicationRevision(100);

            var fv1 = mapper.FromFileRevision(14000);
            var fv2 = mapper.FromFileRevision(100000);

            var yv1 = mapper.FromReleaseYear(2014);
            var yv2 = mapper.FromReleaseYear(2100);
            var yv3 = mapper.FromReleaseYear(1998, "+");

            var vn1 = mapper.GetVersionName(SwVersion_e.Sw97Plus);
            var vn2 = mapper.GetVersionName(SwVersion_e.Sw2011);
            var vn3 = mapper.GetVersionName((SwVersion_e)131);

            Assert.AreEqual(SwVersion_e.Sw2012, av1);
            Assert.AreEqual((SwVersion_e)100, av2);

            Assert.AreEqual(SwVersion_e.Sw2021, fv1);
            Assert.AreEqual((SwVersion_e)115, fv2);

            Assert.AreEqual(SwVersion_e.Sw2014, yv1);
            Assert.AreEqual((SwVersion_e)108, yv2);
            Assert.AreEqual(SwVersion_e.Sw98Plus, yv3);

            Assert.AreEqual("SOLIDWORKS 1997+", vn1);
            Assert.AreEqual("SOLIDWORKS 2011", vn2);
            Assert.AreEqual("SOLIDWORKS 2123", vn3);
        }
    }
}
