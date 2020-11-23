//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Moq;
using NUnit.Framework;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.UI.Commands;
using Xarial.XCad.UI.Commands.Attributes;
using Xarial.XCad.UI.Commands.Structures;
using System.Linq;
using Xarial.XCad.UI.Exceptions;

namespace Base.Tests
{
    public class IXCommandManagerExtensionTest
    {
        public static class ResourcesMock 
        {
            public static byte[] Res1 { get; set; } = new byte[] { 0, 1 };
            public static byte[] Res2 { get; set; } = new byte[] { 2, 3 };
            public static byte[] Res3 { get; set; } = new byte[] { 4, 5 };
        }

        public enum Commands1_e
        {
            Cmd1,
            Cmd2
        }

        [Title("CG1")]
        [Summary("D0")]
        [Icon(typeof(ResourcesMock), nameof(ResourcesMock.Res1))]
        [CommandGroupInfo(150)]
        public enum Commands2_e
        {
            [Title("C1")]
            [Summary("D1")]
            [Icon(typeof(ResourcesMock), nameof(ResourcesMock.Res2))]
            [CommandItemInfo(false, true, Xarial.XCad.UI.Commands.Enums.WorkspaceTypes_e.Assembly, true, Xarial.XCad.UI.Commands.Enums.RibbonTabTextDisplay_e.NoText)]
            [CommandSpacer]
            Cmd1,

            [Title("C2")]
            [Summary("D2")]
            [Icon(typeof(ResourcesMock), nameof(ResourcesMock.Res3))]
            Cmd2
        }

        [CommandGroupParent(125)]
        public enum Commands3_e
        {
            Cmd1,
            Cmd2
        }

        [CommandGroupParent(10)]
        public enum Commands4_e
        {
            Cmd1,
            Cmd2
        }

        [CommandGroupInfo(20)]
        public enum Commands5_e
        {
            Cmd1,
            Cmd2
        }

        [Test]
        public void TestAddCommandGroup()
        {
            CommandGroupSpec res = null;

            var mock = new Mock<IXCommandManager>();

            mock.Setup(m => m.AddCommandGroup(It.IsAny<CommandGroupSpec>()))
                .Callback((CommandGroupSpec s) =>
                {
                    res = s;
                }).Returns(new Mock<IXCommandGroup>().Object);

            mock.Object.AddCommandGroup<Commands1_e>();

            Assert.AreEqual(2, res.Commands.Length);
            Assert.AreEqual("Cmd1", res.Commands[0].Title);
            Assert.AreEqual("Cmd2", res.Commands[1].Title);
        }

        [Test]
        public void TestAddCommandGroupCustomAttributes()
        {
            CommandGroupSpec res = null;

            var mock = new Mock<IXCommandManager>();
            
            mock.Setup(m => m.AddCommandGroup(It.IsAny<CommandGroupSpec>()))
                .Callback((CommandGroupSpec s) =>
                {
                    res = s;
                }).Returns(new Mock<IXCommandGroup>().Object);

            mock.Object.AddCommandGroup<Commands2_e>();

            Assert.AreEqual(150, res.Id);
            Assert.AreEqual("CG1", res.Title);
            Assert.AreEqual("D0", res.Tooltip);
            Assert.That(res.Icon.Buffer.SequenceEqual(new byte[] { 0, 1 }));

            Assert.AreEqual(2, res.Commands.Length);
            
            Assert.AreEqual("C1", res.Commands[0].Title);
            Assert.AreEqual("D1", res.Commands[0].Tooltip);
            Assert.AreEqual(true, res.Commands[0].HasSpacer);
            Assert.AreEqual(false, res.Commands[0].HasMenu);
            Assert.AreEqual(true, res.Commands[0].HasToolbar);
            Assert.AreEqual(true, res.Commands[0].HasTabBox);
            Assert.AreEqual(Xarial.XCad.UI.Commands.Enums.RibbonTabTextDisplay_e.NoText, res.Commands[0].TabBoxStyle);
            Assert.AreEqual(Xarial.XCad.UI.Commands.Enums.WorkspaceTypes_e.Assembly, res.Commands[0].SupportedWorkspace);

            Assert.That(res.Commands[0].Icon.Buffer.SequenceEqual(new byte[] { 2, 3 }));

            Assert.AreEqual("C2", res.Commands[1].Title);
            Assert.AreEqual("D2", res.Commands[1].Tooltip);
            Assert.AreEqual(Xarial.XCad.UI.Commands.Enums.WorkspaceTypes_e.All, res.Commands[1].SupportedWorkspace);
            Assert.That(res.Commands[1].Icon.Buffer.SequenceEqual(new byte[] { 4, 5 }));
        }

        [Test]
        public void TestAddCommandGroupParent() 
        {
            CommandGroupSpec res = null;

            var parentSpec = new CommandGroupSpec(125);

            var cmdParentMock = new Mock<IXCommandGroup>();
            cmdParentMock.Setup(m => m.Spec).Returns(parentSpec);

            var mock = new Mock<IXCommandManager>();

            mock.Setup(m => m.AddCommandGroup(It.IsAny<CommandGroupSpec>()))
                .Callback((CommandGroupSpec s) =>
                {
                    res = s;
                }).Returns(new Mock<IXCommandGroup>().Object);

            mock.Setup(m => m.CommandGroups).Returns(new IXCommandGroup[] { cmdParentMock.Object });

            mock.Object.AddCommandGroup<Commands3_e>();

            Assert.AreEqual(parentSpec, res.Parent);

            Assert.Throws<ParentGroupNotFoundException>(() => mock.Object.AddCommandGroup<Commands4_e>());
            Assert.Throws<ParentGroupCircularDependencyException>(() => mock.Object.CreateSpecFromEnum<Commands5_e>(new CommandGroupSpec(20)));
        }
    }
}