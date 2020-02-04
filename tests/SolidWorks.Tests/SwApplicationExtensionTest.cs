using Moq;
using NUnit.Framework;
using SolidWorks.Interop.sldworks;
using System;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Enums;

namespace Sw.Tests
{
    public class Tests
    {
        [Test]
        public void IsVersionNewerOrEqualTest()
        {
            var sw2017sp23Mock = new Mock<SldWorks>();
            sw2017sp23Mock.Setup(m => m.RevisionNumber()).Returns("25.2.3");
            var app = SwApplication.FromPointer(sw2017sp23Mock.Object);

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
    }
}