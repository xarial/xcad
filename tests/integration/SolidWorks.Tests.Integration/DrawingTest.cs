using NUnit.Framework;
using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Documents;
using Xarial.XCad.SolidWorks.Documents;

namespace SolidWorks.Tests.Integration
{
    public class DrawingTest : IntegrationTests
    {
        [Test]
        public void ActiveSheetTest() 
        {
            string name;

            using (var doc = OpenDataDocument("Sheets1.SLDDRW"))
            {
                name = (m_App.Documents.Active as ISwDrawing).Sheets.Active.Name;
            }

            Assert.AreEqual("Sheet2", name);
        }

        [Test]
        public void IterateSheetsTest()
        {
            string[] confNames;

            using (var doc = OpenDataDocument("Sheets1.SLDDRW"))
            {
                confNames = (m_App.Documents.Active as ISwDrawing).Sheets.Select(x => x.Name).ToArray();
            }

            Assert.That(confNames.SequenceEqual(new string[] 
            {
                "Sheet1", "Sheet2", "MySheet", "Sheet3"
            }));
        }

        [Test]
        public void GetSheetByNameTest()
        {
            IXSheet sheet1;
            IXSheet sheet2;
            IXSheet sheet3;
            bool r1;
            bool r2;
            Exception e1 = null;

            using (var doc = OpenDataDocument("Sheets1.SLDDRW"))
            {
                var sheets = (m_App.Documents.Active as ISwDrawing).Sheets;

                sheet1 = sheets["Sheet1"];
                r1 = sheets.TryGet("Sheet2", out sheet2);
                r2 = sheets.TryGet("Sheet4", out sheet3);

                try
                {
                    var sheet4 = sheets["Sheet4"];
                }
                catch (Exception ex)
                {
                    e1 = ex;
                }
            }

            Assert.IsNotNull(sheet1);
            Assert.IsNotNull(sheet2);
            Assert.IsNull(sheet3);
            Assert.IsTrue(r1);
            Assert.IsFalse(r2);
            Assert.IsNotNull(e1);
        }

        [Test]
        public void CreateModelViewBasedTest() 
        {
            var refDocPathName = "";
            var viewOrientName = "";

            using (var doc = OpenDataDocument("Selections1.SLDPRT"))
            {
                var partDoc = m_App.Documents.Active as IXDocument3D;
                var view = partDoc.ModelViews[StandardViewType_e.Right];
                
                using(var drw = NewDocument(Interop.swconst.swDocumentTypes_e.swDocDRAWING))
                {
                    var drwDoc = m_App.Documents.Active as ISwDrawing;
                    var drwView = (ISwModelBasedDrawingView)drwDoc.Sheets.Active.DrawingViews.CreateModelViewBased(view);
                    refDocPathName = drwView.DrawingView.ReferencedDocument.GetPathName();
                    viewOrientName = drwView.DrawingView.GetOrientationName();
                }
            }

            Assert.AreEqual(GetFilePath("Selections1.SLDPRT"), refDocPathName);
            Assert.AreEqual("*Right", viewOrientName);
        }

        [Test]
        public void DrawingEventsTest()
        {
            var sheetActiveCount = 0;
            var sheetName = "";

            using (var doc = OpenDataDocument("Sheets1.SLDDRW"))
            {
                var draw = (ISwDrawing)m_App.Documents.Active;

                draw.Sheets.SheetActivated += (d, s) =>
                {
                    sheetName = s.Name;
                    sheetActiveCount++;
                };

                var feat = (IFeature)draw.Drawing.FeatureByName("MySheet");
                feat.Select2(false, -1);
                const int swCommands_Activate_Sheet = 1206;
                m_App.Sw.RunCommand(swCommands_Activate_Sheet, "");
            }

            Assert.AreEqual(1, sheetActiveCount);
            Assert.AreEqual("MySheet", sheetName);
        }
    }
}
