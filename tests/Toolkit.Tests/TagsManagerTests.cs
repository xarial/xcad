using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Toolkit.Data;

namespace Toolkit.Tests
{
    public class TagsManagerTests
    {
        [Test]
        public void TestPut() 
        {
            var tagsMgr = new TagsManager();

            tagsMgr.Put<string>("ABC", "Test");
            tagsMgr.Put<int>("XYZ", 10);

            var r1 = tagsMgr.Get<string>("ABC");

            tagsMgr.Put<string>("ABC", "Test2");

            var r2 = tagsMgr.Get<string>("ABC");

            var r3 = tagsMgr.Get<int>("XYZ");
            var r4 = tagsMgr.Contains("XYZ");

            Assert.AreEqual("Test", r1);
            Assert.AreEqual("Test2", r2);
            Assert.AreEqual(10, r3);
            Assert.IsTrue(r4);
        }

        [Test]
        public void TestPop()
        {
            var tagsMgr = new TagsManager();

            tagsMgr.Put<string>("ABC", "Test");
            tagsMgr.Put<string>("ABC", "Test2");
            tagsMgr.Put<int>("XYZ", 10);

            var r1 = tagsMgr.Pop<string>("ABC");
            var r2 = tagsMgr.Pop<int>("XYZ");

            var r3 = tagsMgr.Contains("ABC");
            var r4 = tagsMgr.Contains("XYZ");

            Assert.AreEqual("Test2", r1);
            Assert.AreEqual(10, r2);
            Assert.IsFalse(r3);
            Assert.IsFalse(r4);
        }

        [Test]
        public void TestErrors() 
        {
            var tagsMgr = new TagsManager();

            tagsMgr.Put<string>("ABC", "Test");
            tagsMgr.Put<int>("XYZ", 10);

            Assert.Throws<KeyNotFoundException>(() => tagsMgr.Get<int>("KLM"));
            Assert.Throws<InvalidCastException>(() => tagsMgr.Get<int>("ABC"));
        }
    }
}
