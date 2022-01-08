using NUnit.Framework;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry;

namespace SolidWorks.Tests.Integration
{
    public class EntititiesTest : IntegrationTests
    {
        [Test]
        public void ResilientEntititiesTest()
        {
            bool a1, a2, a3, a4, a5, a6;
            bool a1_1, a2_1, a3_1, a4_1, a5_1, a6_1;
            Type t1, t2, t3, t4, t5, t6;
            Type t1_1, t2_1, t3_1, t4_1, t5_1, t6_1;
            bool r1, r2, r3, r4, r5, r6;
            bool r1_1, r2_1, r3_1, r4_1, r5_1, r6_1;

            using (var doc = OpenDataDocument("EntitiesBodies1.SLDPRT"))
            {
                var part = m_App.Documents.Active as ISwPart;
                
                var b1 = (ISwBody)part.Bodies["BODY1"];
                var b2 = (ISwBody)part.Bodies["BODY2"];
                var f1 = part.CreateObjectFromDispatch<ISwFace>(part.Part.GetEntityByName("FACE1", (int)swSelectType_e.swSelFACES));
                var f2 = part.CreateObjectFromDispatch<ISwFace>(part.Part.GetEntityByName("FACE2", (int)swSelectType_e.swSelFACES));
                var e1 = part.CreateObjectFromDispatch<ISwEdge>(part.Part.GetEntityByName("EDGE1", (int)swSelectType_e.swSelEDGES));
                var e2 = part.CreateObjectFromDispatch<ISwEdge>(part.Part.GetEntityByName("EDGE2", (int)swSelectType_e.swSelEDGES));

                var b1_1 = b1.CreateResilient();
                var b2_1 = b2.CreateResilient();
                var f1_1 = f1.CreateResilient();
                var f2_1 = f2.CreateResilient();
                var e1_1 = e1.CreateResilient();
                var e2_1 = e2.CreateResilient();

                r1 = b1.IsResilient;
                r2 = b2.IsResilient;
                r3 = f1.IsResilient;
                r4 = f2.IsResilient;
                r5 = e1.IsResilient;
                r6 = e2.IsResilient;

                t1 = b1.GetType();
                t2 = b2.GetType();
                t3 = f1.GetType();
                t4 = f2.GetType();
                t5 = e1.GetType();
                t6 = e2.GetType();

                r1_1 = b1_1.IsResilient;
                r2_1 = b2_1.IsResilient;
                r3_1 = f1_1.IsResilient;
                r4_1 = f2_1.IsResilient;
                r5_1 = e1_1.IsResilient;
                r6_1 = e2_1.IsResilient;

                t1_1 = b1_1.GetType();
                t2_1 = b2_1.GetType();
                t3_1 = f1_1.GetType();
                t4_1 = f2_1.GetType();
                t5_1 = e1_1.GetType();
                t6_1 = e2_1.GetType();

                part.Model.ForceRebuild3(false);

                a1 = b1.IsAlive;
                a2 = b2.IsAlive;
                a3 = f1.IsAlive;
                a4 = f2.IsAlive;
                a5 = e1.IsAlive;
                a6 = e2.IsAlive;

                a1_1 = b1_1.IsAlive;
                a2_1 = b2_1.IsAlive;
                a3_1 = f1_1.IsAlive;
                a4_1 = f2_1.IsAlive;
                a5_1 = e1_1.IsAlive;
                a6_1 = e2_1.IsAlive;
            }

            Assert.IsFalse(a1);
            Assert.IsFalse(a2);
            Assert.IsFalse(a3);
            Assert.IsFalse(a4);
            Assert.IsFalse(a5);
            Assert.IsFalse(a6);

            Assert.IsFalse(r1);
            Assert.IsFalse(r2);
            Assert.IsFalse(r3);
            Assert.IsFalse(r4);
            Assert.IsFalse(r5);
            Assert.IsFalse(r6);

            Assert.IsTrue(a1_1);
            Assert.IsTrue(a2_1);
            Assert.IsTrue(a3_1);
            Assert.IsTrue(a4_1);
            Assert.IsTrue(a5_1);
            Assert.IsTrue(a6_1);

            Assert.IsTrue(r1_1);
            Assert.IsTrue(r2_1);
            Assert.IsTrue(r3_1);
            Assert.IsTrue(r4_1);
            Assert.IsTrue(r5_1);
            Assert.IsTrue(r6_1);

            Assert.AreEqual(t1, t1_1);
            Assert.AreEqual(t2, t2_1);
            Assert.AreEqual(t3, t3_1);
            Assert.AreEqual(t4, t4_1);
            Assert.AreEqual(t5, t5_1);
            Assert.AreEqual(t6, t6_1);
        }
    }
}

