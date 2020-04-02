//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Moq;
using NUnit.Framework;
using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.SolidWorks;

namespace Sw.Tests
{
    public class SwAddInExTest
    {
        [Test]
        public void TestConnectToSW()
        {
            var addInExMock1 = new Mock<SwAddInEx>();

            bool connectCalled = false;
            addInExMock1.Setup(a => a.OnConnect()).Callback(
                () =>
                {
                    connectCalled = true;
                });

            var addInExMock2 = new Mock<SwAddInEx>();
            addInExMock2.Setup(a => a.OnConnect())
                .Callback(() => throw new Exception());

            var addInExMock3 = new Mock<SwAddInEx>() { CallBase = true };

            var res1 = addInExMock1.Object.ConnectToSW(new Mock<SldWorks>().Object, 0);
            var res2 = addInExMock2.Object.ConnectToSW(new Mock<SldWorks>().Object, 0);
            var res3 = addInExMock3.Object.ConnectToSW(new Mock<SldWorks>().Object, 0);

            Assert.IsTrue(connectCalled);
            Assert.IsTrue(res1);
            Assert.IsFalse(res2);
            Assert.IsTrue(res3);
        }

        [Test]
        public void DisconnectFromSWTest()
        {
            var addInExMock1 = new Mock<SwAddInEx>();
            bool disconnectCalled = false;
            addInExMock1.Setup(a => a.OnDisconnect()).Callback(
                () =>
                {
                    disconnectCalled = true;
                });

            var addInExMock2 = new Mock<SwAddInEx>();
            addInExMock2.Setup(a => a.OnDisconnect())
                .Callback(() => throw new Exception());

            var swMock = new Mock<SldWorks>();
            swMock.Setup(m => m.GetCommandManager(It.IsAny<int>()))
                .Returns(new Mock<CommandManager>().Object);

            var addInExMock3 = new Mock<SwAddInEx>() { CallBase = true };

            addInExMock1.Object.ConnectToSW(swMock.Object, 0);
            addInExMock2.Object.ConnectToSW(swMock.Object, 0);
            addInExMock3.Object.ConnectToSW(swMock.Object, 0);

            var res1 = addInExMock1.Object.DisconnectFromSW();
            var res2 = addInExMock2.Object.DisconnectFromSW();
            var res3 = addInExMock3.Object.DisconnectFromSW();

            Assert.IsTrue(disconnectCalled);
            Assert.IsTrue(res1);
            Assert.IsFalse(res2);
            Assert.IsTrue(res3);
        }
    }
}
