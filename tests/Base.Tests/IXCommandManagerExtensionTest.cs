using Moq;
using NUnit.Framework;
using Xarial.XCad.UI.Commands;
using Xarial.XCad.UI.Commands.Structures;

namespace Base.Tests
{
    public class IXCommandManagerExtensionTest
    {
        public enum Commands1_e
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
    }
}