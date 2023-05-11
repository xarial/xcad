using NUnit.Framework;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Documents.Extensions;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.Toolkit.Data;

namespace SolidWorks.Tests.Integration
{
    public class TagsTest : IntegrationTests
    {
        [Test]
        public void TagsPutTest() 
        {
            bool c1_1;
            bool c2_1;
            bool c3_1;

            bool c1_2;
            bool c2_2;

            bool c1_3;
            bool c2_3;

            bool e1;
            bool e2;

            using (var doc = OpenDataDocument("EntitiesBodies1.SLDPRT"))
            {
                var part = m_App.Documents.Active as ISwPart;

                var f1 = part.CreateObjectFromDispatch<ISwFace>(part.Part.GetEntityByName("FACE1", (int)swSelectType_e.swSelFACES));

                f1.Tags.Put("Tag1", "Test1");
                f1.Tags.Put("Tag2", 10d);

                c1_1 = f1.Tags.Contains("Tag1");
                c2_1 = f1.Tags.Contains("Tag2");
                c3_1 = f1.Tags.Contains("Tag3");

                var f2 = part.CreateObjectFromDispatch<ISwFace>(part.Part.GetEntityByName("FACE1", (int)swSelectType_e.swSelFACES));
                c1_2 = f2.Tags.Contains("Tag1");
                c2_2 = f2.Tags.Contains("Tag2");
                e1 = f2.Tags.IsEmpty;

                var f3 = part.CreateObjectFromDispatch<ISwFace>(part.Part.GetEntityByName("FACE2", (int)swSelectType_e.swSelFACES));
                c1_3 = f3.Tags.Contains("Tag1");
                c2_3 = f3.Tags.Contains("Tag2");
                e2 = f3.Tags.IsEmpty;
            }

            Assert.IsTrue(c1_1);
            Assert.IsTrue(c2_1);
            Assert.IsFalse(c3_1);
            Assert.IsTrue(c1_2);
            Assert.IsTrue(c2_2);
            Assert.IsFalse(c1_3);
            Assert.IsFalse(c1_3);
            Assert.IsFalse(e1);
            Assert.IsTrue(e2);
        }

        [Test]
        public void TagsGetTest()
        {
            bool e1;
            string t1;
            bool e2;

            using (var doc = OpenDataDocument("EntitiesBodies1.SLDPRT"))
            {
                var part = m_App.Documents.Active as ISwPart;

                var f1 = part.CreateObjectFromDispatch<ISwFace>(part.Part.GetEntityByName("FACE1", (int)swSelectType_e.swSelFACES));

                f1.Tags.Put("Tag1", "Test1");
                e1 = f1.Tags.IsEmpty;

                var f2 = part.CreateObjectFromDispatch<ISwFace>(part.Part.GetEntityByName("FACE1", (int)swSelectType_e.swSelFACES));
                t1 = f2.Tags.Get<string>("Tag1");
                e2 = f2.Tags.IsEmpty;
            }

            Assert.IsFalse(e1);
            Assert.IsFalse(e2);
            Assert.AreEqual("Test1", t1);
        }

        [Test]
        public void TagsPopTest()
        {
            bool e1;
            string t1;
            bool e2;

            using (var doc = OpenDataDocument("EntitiesBodies1.SLDPRT"))
            {
                var part = m_App.Documents.Active as ISwPart;

                var f1 = part.CreateObjectFromDispatch<ISwFace>(part.Part.GetEntityByName("FACE1", (int)swSelectType_e.swSelFACES));

                f1.Tags.Put("Tag1", "Test1");
                e1 = f1.Tags.IsEmpty;

                var f2 = part.CreateObjectFromDispatch<ISwFace>(part.Part.GetEntityByName("FACE1", (int)swSelectType_e.swSelFACES));
                t1 = f2.Tags.Pop<string>("Tag1");
                e2 = f2.Tags.IsEmpty;
            }

            Assert.IsFalse(e1);
            Assert.IsTrue(e2);
            Assert.AreEqual("Test1", t1);
        }

        [Test]
        public void TagsLifecycleTest()
        {
            int c1;
            int c2;
            int c3;
            int c4;
            int c5;

            var tagsReg = (GlobalTagsRegistry)m_App.GetType().GetProperty("TagsRegistry", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(m_App);
            c1 = tagsReg.Count;

            var part = (ISwPart)m_App.Documents.Open(GetFilePath("EntitiesBodies1.SLDPRT"), Xarial.XCad.Documents.Enums.DocumentState_e.ReadOnly);

            var f1 = part.CreateObjectFromDispatch<ISwFace>(part.Part.GetEntityByName("FACE1", (int)swSelectType_e.swSelFACES));
            f1.Tags.Put("Tag1", "Test1");

            c2 = tagsReg.Count;

            var t1 = f1.Tags.Pop<string>("Tag1");

            c3 = tagsReg.Count;

            var f2 = part.CreateObjectFromDispatch<ISwFace>(part.Part.GetEntityByName("FACE2", (int)swSelectType_e.swSelFACES));

            f2.Tags.Put("Tag1", "Test1");
            f2.Tags.Put("Tag2", "Test2");

            c4 = tagsReg.Count;

            part.Close();

            c5 = tagsReg.Count;

            Assert.AreEqual(0, c1);
            Assert.AreEqual(1, c2);
            Assert.AreEqual(0, c3);
            Assert.AreEqual(1, c4);
            Assert.AreEqual(0, c5);
        }
    }
}
