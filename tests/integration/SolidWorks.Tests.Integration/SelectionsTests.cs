using NUnit.Framework;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Features;
using Xarial.XCad.SolidWorks.Geometry;

namespace SolidWorks.Tests.Integration
{
    public class SelectionsTests : IntegrationTests
    {
        [Test]
        public void IterateSelectionsTest()
        {
            int selCount;
            var selTypes = new List<Type>();

            using (var doc = OpenDataDocument("Selections1.SLDPRT"))
            {
                var part = (IPartDoc)m_App.Sw.IActiveDoc2;
                (part as IModelDoc2).ClearSelection2(true);
                (part.GetEntityByName("Face1", (int)swSelectType_e.swSelFACES) as IEntity).Select4(true, null);
                (part.GetEntityByName("Face2", (int)swSelectType_e.swSelFACES) as IEntity).Select4(true, null);
                (part.GetEntityByName("Edge1", (int)swSelectType_e.swSelEDGES) as IEntity).Select4(true, null);
                (part.FeatureByName("Sketch1") as IFeature).Select2(true, -1);

                selCount = m_App.Documents.Active.Selections.Count;

                foreach (var sel in m_App.Documents.Active.Selections) 
                {
                    selTypes.Add(sel.GetType());
                }
            }

            Assert.AreEqual(4, selCount);
            Assert.That(typeof(ISwPlanarFace).IsAssignableFrom(selTypes[0]));
            Assert.That(typeof(ISwCylindricalFace).IsAssignableFrom(selTypes[1]));
            Assert.That(typeof(ISwLinearEdge).IsAssignableFrom(selTypes[2]));
            Assert.That(typeof(ISwSketch2D).IsAssignableFrom(selTypes[3]));
        }

        [Test]
        public void NewSelectionEventsTest() 
        {
            var selTypes = new List<Type>();

            using (var doc = OpenDataDocument("Selections1.SLDPRT"))
            {
                var part = (IPartDoc)m_App.Sw.IActiveDoc2;

                m_App.Documents.Active.Selections.NewSelection += (d, o) => selTypes.Add(o.GetType());

                (part.GetEntityByName("Face1", (int)swSelectType_e.swSelFACES) as IEntity).Select4(true, null);
                (part.GetEntityByName("Edge1", (int)swSelectType_e.swSelEDGES) as IEntity).Select4(true, null);
            }

            Assert.That(typeof(ISwPlanarFace).IsAssignableFrom(selTypes[0]));
            Assert.That(typeof(ISwLinearEdge).IsAssignableFrom(selTypes[1]));
        }

        [Test]
        public void ClearSelectionEventsTest()
        {
            int clearSelCount = 0;

            using (var doc = OpenDataDocument("Selections1.SLDPRT"))
            {
                var part = (IPartDoc)m_App.Sw.IActiveDoc2;

                m_App.Documents.Active.Selections.ClearSelection += (d) => clearSelCount++;

                (part.GetEntityByName("Face1", (int)swSelectType_e.swSelFACES) as IEntity).Select4(true, null);
                (part.GetEntityByName("Edge1", (int)swSelectType_e.swSelEDGES) as IEntity).Select4(false, null);
                (part.GetEntityByName("Face1", (int)swSelectType_e.swSelFACES) as IEntity).Select4(true, null);
                (part as IModelDoc2).ClearSelection2(true);
                (part.GetEntityByName("Face1", (int)swSelectType_e.swSelFACES) as IEntity).Select4(true, null);
                (part.GetEntityByName("Face1", (int)swSelectType_e.swSelFACES) as IEntity).DeSelect();
            }

            Assert.AreEqual(2, clearSelCount);
        }

        [Test]
        public void AddRemoveSelectionTest()
        {
            int selCount1;
            int selCount2;

            int r1;
            int r2;
            int r3;
            int r4;
            int r5;
            int r6;

            using (var doc = OpenDataDocument("Selections1.SLDPRT"))
            {
                var model = m_App.Documents.Active;

                var part = (IPartDoc)model.Model;
                var selMgr = model.Model.ISelectionManager;

                model.Model.ClearSelection2(true);
                
                var e1 = model.CreateObjectFromDispatch<ISwFace>(part.GetEntityByName("Face1", (int)swSelectType_e.swSelFACES) as IEntity);
                var e2 = model.CreateObjectFromDispatch<ISwEdge>(part.GetEntityByName("Edge1", (int)swSelectType_e.swSelEDGES) as IEntity);
                var e3 = model.CreateObjectFromDispatch<ISwFace>(part.GetEntityByName("Face2", (int)swSelectType_e.swSelFACES) as IEntity);
                var e4 = model.CreateObjectFromDispatch<ISwFeature>(part.FeatureByName("Sketch1") as IFeature);

                model.Selections.AddRange(new ISwSelObject[] { e1, e2, e3, e4 });

                selCount1 = selMgr.GetSelectedObjectCount2(-1);
                r1 = m_App.Sw.IsSame(e1.Dispatch, selMgr.GetSelectedObject6(1, -1));
                r2 = m_App.Sw.IsSame(e2.Dispatch, selMgr.GetSelectedObject6(2, -1));
                r3 = m_App.Sw.IsSame(e3.Dispatch, selMgr.GetSelectedObject6(3, -1));
                r4 = m_App.Sw.IsSame(e4.Feature, selMgr.GetSelectedObject6(4, -1));

                model.Selections.RemoveRange(new ISwSelObject[] { e1, e3});
                selCount2 = selMgr.GetSelectedObjectCount2(-1);
                r5 = m_App.Sw.IsSame(e2.Dispatch, selMgr.GetSelectedObject6(1, -1));
                r6 = m_App.Sw.IsSame(e4.Feature, selMgr.GetSelectedObject6(2, -1));
            }

            Assert.AreEqual(4, selCount1);
            Assert.AreEqual((int)swObjectEquality.swObjectSame, r1);
            Assert.AreEqual((int)swObjectEquality.swObjectSame, r2);
            Assert.AreEqual((int)swObjectEquality.swObjectSame, r3);
            Assert.AreEqual((int)swObjectEquality.swObjectSame, r4);

            Assert.AreEqual(2, selCount2);
            Assert.AreEqual((int)swObjectEquality.swObjectSame, r5);
            Assert.AreEqual((int)swObjectEquality.swObjectSame, r6);
        }
    }
}
