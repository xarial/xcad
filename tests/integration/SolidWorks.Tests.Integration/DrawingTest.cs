using NUnit.Framework;
using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Structures;
using Xarial.XCad.Geometry.Structures;
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

        [Test]
        public void DrawingViewIterateTest() 
        {
            string[] sheet1Views;
            string[] sheet2Views;

            using (var doc = OpenDataDocument("Drawing1\\Drawing1.SLDDRW"))
            {
                var sheets = (m_App.Documents.Active as ISwDrawing).Sheets;
                sheet1Views = sheets["Sheet1"].DrawingViews.Select(v => v.Name).ToArray();
                sheet2Views = sheets["Sheet2"].DrawingViews.Select(v => v.Name).ToArray();
            }

            CollectionAssert.AreEqual(sheet1Views, new string[] { "Drawing View1", "Drawing View2", "Drawing View3" });
            CollectionAssert.AreEqual(sheet2Views, new string[] { "Drawing View4", "Drawing View5" });
        }

        [Test]
        public void DrawingViewDocumentTest()
        {
            string view1DocPath;
            string view2DocPath;
            string view3DocPath;
            string view4DocPath;
            string view5DocPath;

            string view1Conf;
            string view2Conf;
            string view3Conf;
            string view4Conf;
            string view5Conf;

            using (var doc = OpenDataDocument("Drawing1\\Drawing1.SLDDRW"))
            {
                var sheets = (m_App.Documents.Active as ISwDrawing).Sheets;
                
                var v1 = sheets["Sheet1"].DrawingViews["Drawing View1"];
                var v2 = sheets["Sheet1"].DrawingViews["Drawing View2"];
                var v3 = sheets["Sheet1"].DrawingViews["Drawing View3"];
                var v4 = sheets["Sheet2"].DrawingViews["Drawing View4"];
                var v5 = sheets["Sheet2"].DrawingViews["Drawing View5"];

                view1DocPath = v1.ReferencedDocument.Path;
                view2DocPath = v2.ReferencedDocument.Path;
                view3DocPath = v3.ReferencedDocument.Path;
                view4DocPath = v4.ReferencedDocument.Path;
                view5DocPath = v5.ReferencedDocument != null ? v5.ReferencedDocument.Path : "";

                view1Conf = v1.ReferencedConfiguration.Name;
                view2Conf = v2.ReferencedConfiguration.Name;
                view3Conf = v3.ReferencedConfiguration.Name;
                view4Conf = v4.ReferencedConfiguration.Name;
                view5Conf = v5.ReferencedConfiguration != null ? v5.ReferencedConfiguration.Name : "";
            }

            Assert.That(string.Equals(view1DocPath, GetFilePath("Drawing1\\Part1.sldprt"), StringComparison.CurrentCultureIgnoreCase));
            Assert.That(string.Equals(view2DocPath, GetFilePath("Drawing1\\Part1.sldprt"), StringComparison.CurrentCultureIgnoreCase));
            Assert.That(string.Equals(view3DocPath, GetFilePath("Drawing1\\Part1.sldprt"), StringComparison.CurrentCultureIgnoreCase));
            Assert.That(string.Equals(view4DocPath, GetFilePath("Drawing1\\Part2.sldprt"), StringComparison.CurrentCultureIgnoreCase));
            Assert.That(string.IsNullOrEmpty(view5DocPath));

            Assert.That(string.Equals(view1Conf, "Conf1", StringComparison.CurrentCultureIgnoreCase));
            Assert.That(string.Equals(view2Conf, "Conf1", StringComparison.CurrentCultureIgnoreCase));
            Assert.That(string.Equals(view3Conf, "Default", StringComparison.CurrentCultureIgnoreCase));
            Assert.That(string.Equals(view4Conf, "Default", StringComparison.CurrentCultureIgnoreCase));
            Assert.That(string.IsNullOrEmpty(view5Conf));
        }

        [Test]
        public void DrawingPaddingTest()
        {
            Thickness p1;
            Thickness p2;
            Thickness p3;
            Thickness p4;
            Thickness p5;
            Thickness p6;

            double s1;
            double s2;
            double s3;

            using (var doc = OpenDataDocument("Drawing2\\Drawing2.SLDDRW"))
            {
                var sheets = (m_App.Documents.Active as ISwDrawing).Sheets;

                var sheet1 = sheets["Sheet1"];
                var sheet2 = sheets["Sheet2"];
                var sheet3 = sheets["Sheet3"];

                s1 = sheet1.Scale.AsDouble();
                s2 = sheet2.Scale.AsDouble();
                s3 = sheet3.Scale.AsDouble();

                p1 = sheet1.DrawingViews["Drawing View1"].Padding;
                p2 = sheet1.DrawingViews["Drawing View2"].Padding;
                p3 = sheet2.DrawingViews["Drawing View3"].Padding;
                p4 = sheet2.DrawingViews["Drawing View4"].Padding;
                p5 = sheet3.DrawingViews["Drawing View5"].Padding;
                p6 = sheet3.DrawingViews["Drawing View6"].Padding;
            }

            Assert.That(p1.Bottom / s1, Is.EqualTo(0.016819999999999998).Within(0.0001).Percent);
            Assert.That(p1.Top / s1, Is.EqualTo(0.016819999999999998).Within(0.0001).Percent);
            Assert.That(p1.Right / s1, Is.EqualTo(0.016819999999999998).Within(0.0001).Percent);
            Assert.That(p1.Left / s1, Is.EqualTo(0.016819999999999998).Within(0.0001).Percent);

            Assert.That(p2.Bottom / s1, Is.EqualTo(0.016819999999999998).Within(0.0001).Percent);
            Assert.That(p2.Top / s1, Is.EqualTo(0.016819999999999998).Within(0.0001).Percent);
            Assert.That(p2.Right / s1, Is.EqualTo(0.016819999999999998).Within(0.0001).Percent);
            Assert.That(p2.Left / s1, Is.EqualTo(0.016819999999999998).Within(0.0001).Percent);

            Assert.That(p3.Bottom / s2, Is.EqualTo(0.033639999999999996).Within(0.0001).Percent);
            Assert.That(p3.Top / s2, Is.EqualTo(0.033639999999999996).Within(0.0001).Percent);
            Assert.That(p3.Right / s2, Is.EqualTo(0.033639999999999996).Within(0.0001).Percent);
            Assert.That(p3.Left / s2, Is.EqualTo(0.033639999999999996).Within(0.0001).Percent);

            Assert.That(p4.Bottom / s2, Is.EqualTo(0.033639999999999996).Within(0.0001).Percent);
            Assert.That(p4.Top / s2, Is.EqualTo(0.033639999999999996).Within(0.0001).Percent);
            Assert.That(p4.Right / s2, Is.EqualTo(0.033639999999999996).Within(0.0001).Percent);
            Assert.That(p4.Left / s2, Is.EqualTo(0.033639999999999996).Within(0.0001).Percent);

            Assert.That(p5.Bottom / s3, Is.EqualTo(0.0014).Within(0.0001).Percent);
            Assert.That(p5.Top / s3, Is.EqualTo(0.0014).Within(0.0001).Percent);
            Assert.That(p5.Right / s3, Is.EqualTo(0.0014).Within(0.0001).Percent);
            Assert.That(p5.Left / s3, Is.EqualTo(0.0014).Within(0.0001).Percent);

            Assert.That(p6.Bottom / s3, Is.EqualTo(0.0014).Within(0.0001).Percent);
            Assert.That(p6.Top / s3, Is.EqualTo(0.0014).Within(0.0001).Percent);
            Assert.That(p6.Right / s3, Is.EqualTo(0.0014).Within(0.0001).Percent);
            Assert.That(p6.Left / s3, Is.EqualTo(0.0014).Within(0.0001).Percent);
        }

        [Test]
        public void DrawingLocationTest()
        {
            Point p1;
            Point p2;
            Point p3;
            Point p4;
            Point p5;
            Point p6;

            double s1;
            double s2;
            double s3;

            using (var doc = OpenDataDocument("Drawing2\\Drawing2.SLDDRW"))
            {
                var sheets = (m_App.Documents.Active as ISwDrawing).Sheets;

                var sheet1 = sheets["Sheet1"];
                var sheet2 = sheets["Sheet2"];
                var sheet3 = sheets["Sheet3"];

                s1 = sheet1.Scale.AsDouble();
                s2 = sheet2.Scale.AsDouble();
                s3 = sheet3.Scale.AsDouble();

                p1 = sheet1.DrawingViews["Drawing View1"].Location;
                p2 = sheet1.DrawingViews["Drawing View2"].Location;
                p3 = sheet2.DrawingViews["Drawing View3"].Location;
                p4 = sheet2.DrawingViews["Drawing View4"].Location;
                p5 = sheet3.DrawingViews["Drawing View5"].Location;
                p6 = sheet3.DrawingViews["Drawing View6"].Location;
            }

            Assert.That(p1.X / s1, Is.EqualTo(0.347683547297297).Within(0.0001).Percent);
            Assert.That(p1.Y / s1, Is.EqualTo(0.611770675675676).Within(0.0001).Percent);
            Assert.That(p1.Z / s1, Is.EqualTo(0).Within(0.0001).Percent);

            Assert.That(p2.X / s1, Is.EqualTo(0.76128881162062212).Within(0.0001).Percent);
            Assert.That(p2.Y / s1, Is.EqualTo(0.53438399106822387).Within(0.0001).Percent);
            Assert.That(p2.Z / s1, Is.EqualTo(0).Within(0.0001).Percent);

            Assert.That(p3.X / s2, Is.EqualTo(0.83543905405405439).Within(0.0001).Percent);
            Assert.That(p3.Y / s2, Is.EqualTo(1.0496589189189187).Within(0.0001).Percent);
            Assert.That(p3.Z / s2, Is.EqualTo(0).Within(0.0001).Percent);

            Assert.That(p4.X / s2, Is.EqualTo(1.8526512837837843).Within(0.0001).Percent);
            Assert.That(p4.Y / s2, Is.EqualTo(0.94919351351351344).Within(0.0001).Percent);
            Assert.That(p4.Z / s2, Is.EqualTo(0).Within(0.0001).Percent);

            Assert.That(p5.X / s3, Is.EqualTo(0.022835313821058443).Within(0.0001).Percent);
            Assert.That(p5.Y / s3, Is.EqualTo(0.081958522817812171).Within(0.0001).Percent);
            Assert.That(p5.Z / s3, Is.EqualTo(0).Within(0.0001).Percent);

            Assert.That(p6.X / s3, Is.EqualTo(0.037347512600946653).Within(0.0001).Percent);
            Assert.That(p6.Y / s3, Is.EqualTo(0.048798225460288336).Within(0.0001).Percent);
            Assert.That(p6.Z / s3, Is.EqualTo(0).Within(0.0001).Percent);
        }

        [Test]
        public void DrawingBoundaryTest()
        {
            Rect2D b1;
            Rect2D b2;
            Rect2D b3;
            Rect2D b4;
            Rect2D b5;
            Rect2D b6;

            double s1;
            double s2;
            double s3;

            using (var doc = OpenDataDocument("Drawing2\\Drawing2.SLDDRW"))
            {
                var sheets = (m_App.Documents.Active as ISwDrawing).Sheets;

                var sheet1 = sheets["Sheet1"];
                var sheet2 = sheets["Sheet2"];
                var sheet3 = sheets["Sheet3"];

                s1 = sheet1.Scale.AsDouble();
                s2 = sheet2.Scale.AsDouble();
                s3 = sheet3.Scale.AsDouble();

                b1 = sheet1.DrawingViews["Drawing View1"].Boundary;
                b2 = sheet1.DrawingViews["Drawing View2"].Boundary;
                b3 = sheet2.DrawingViews["Drawing View3"].Boundary;
                b4 = sheet2.DrawingViews["Drawing View4"].Boundary;
                b5 = sheet3.DrawingViews["Drawing View5"].Boundary;
                b6 = sheet3.DrawingViews["Drawing View6"].Boundary;
            }

            Assert.That(b1.CenterPoint.X / s1, Is.EqualTo(0.347683547297297).Within(0.0001).Percent);
            Assert.That(b1.CenterPoint.Y / s1, Is.EqualTo(0.611770675675676).Within(0.0001).Percent);
            Assert.That(b1.CenterPoint.Z / s1, Is.EqualTo(0).Within(0.0001).Percent);
            Assert.That(b1.Width / s1, Is.EqualTo(0.31397657558361641).Within(0.0001).Percent);
            Assert.That(b1.Height / s1, Is.EqualTo(0.25674397158642376).Within(0.0001).Percent);

            Assert.That(b2.CenterPoint.X / s1, Is.EqualTo(0.76128881162062212).Within(0.0001).Percent);
            Assert.That(b2.CenterPoint.Y / s1, Is.EqualTo(0.53438399106822387).Within(0.0001).Percent);
            Assert.That(b2.CenterPoint.Z / s1, Is.EqualTo(0).Within(0.0001).Percent);
            Assert.That(b2.Width / s1, Is.EqualTo(0.27343183861370846).Within(0.0001).Percent);
            Assert.That(b2.Height / s1, Is.EqualTo(0.27343183861370851).Within(0.0001).Percent);

            Assert.That(b3.CenterPoint.X / s2, Is.EqualTo(0.83543905405405439).Within(0.0001).Percent);
            Assert.That(b3.CenterPoint.Y / s2, Is.EqualTo(1.0496589189189187).Within(0.0001).Percent);
            Assert.That(b3.CenterPoint.Z / s2, Is.EqualTo(0).Within(0.0001).Percent);
            Assert.That(b3.Width / s2, Is.EqualTo(1.1886263023344656).Within(0.0001).Percent);
            Assert.That(b3.Height / s2, Is.EqualTo(0.95969588634569514).Within(0.0001).Percent);

            Assert.That(b4.CenterPoint.X / s2, Is.EqualTo(1.8526512837837843).Within(0.0001).Percent);
            Assert.That(b4.CenterPoint.Y / s2, Is.EqualTo(0.94919351351351344).Within(0.0001).Percent);
            Assert.That(b4.CenterPoint.Z / s2, Is.EqualTo(0).Within(0.0001).Percent);
            Assert.That(b4.Width / s2, Is.EqualTo(0.18717591930685407).Within(0.0001).Percent);
            Assert.That(b4.Height / s2, Is.EqualTo(0.18717591930685418).Within(0.0001).Percent);

            Assert.That(b5.CenterPoint.X / s3, Is.EqualTo(0.022835313821058443).Within(0.0001).Percent);
            Assert.That(b5.CenterPoint.Y / s3, Is.EqualTo(0.081958522817812171).Within(0.0001).Percent);
            Assert.That(b5.CenterPoint.Z / s3, Is.EqualTo(0).Within(0.0001).Percent);
            Assert.That(b5.Width / s3, Is.EqualTo(0.021489105038907764).Within(0.0001).Percent);
            Assert.That(b5.Height / s3, Is.EqualTo(0.017673598105761589).Within(0.0001).Percent);

            Assert.That(b6.CenterPoint.X / s3, Is.EqualTo(0.037347512600946653).Within(0.0001).Percent);
            Assert.That(b6.CenterPoint.Y / s3, Is.EqualTo(0.048798225460288336).Within(0.0001).Percent);
            Assert.That(b6.CenterPoint.Z / s3, Is.EqualTo(0).Within(0.0001).Percent);
            Assert.That(b6.Width / s3, Is.EqualTo(0.042765306435618093).Within(0.0001).Percent);
            Assert.That(b6.Height / s3, Is.EqualTo(0.042765306435618093).Within(0.0001).Percent);
        }

        [Test]
        public void ViewTypesTest()
        {
            Type t1;
            Type t2;
            Type t3;
            Type t4;
            Type t5;
            Type t6;
            Type t7;
            Type t8;
            Type t9;
            Type t10;
            Type t11;

            using (var doc = OpenDataDocument("Drawing3\\Drawing3.SLDDRW"))
            {
                var sheet = (m_App.Documents.Active as ISwDrawing).Sheets.First();

                t1 = sheet.DrawingViews["Drawing View1"].GetType();
                t2 = sheet.DrawingViews["Drawing View2"].GetType();
                t3 = sheet.DrawingViews["Drawing View3"].GetType();
                t4 = sheet.DrawingViews["Drawing View4"].GetType();
                t5 = sheet.DrawingViews["Drawing View5"].GetType();
                t6 = sheet.DrawingViews["Drawing View6"].GetType();
                t7 = sheet.DrawingViews["Drawing View7"].GetType();
                t8 = sheet.DrawingViews["Drawing View8"].GetType();
                t9 = sheet.DrawingViews["Section View B-B"].GetType();
                t10 = sheet.DrawingViews["Removed Section1"].GetType();
                t11 = sheet.DrawingViews["Detail View C (4 : 1)"].GetType();
            }

            Assert.That(typeof(ISwModelBasedDrawingView).IsAssignableFrom(t1));
            Assert.That(typeof(ISwProjectedDrawingView).IsAssignableFrom(t2));
            Assert.That(typeof(ISwProjectedDrawingView).IsAssignableFrom(t3));
            Assert.That(typeof(ISwModelBasedDrawingView).IsAssignableFrom(t4));
            Assert.That(typeof(ISwProjectedDrawingView).IsAssignableFrom(t5));
            Assert.That(typeof(ISwProjectedDrawingView).IsAssignableFrom(t6));
            Assert.That(typeof(ISwModelBasedDrawingView).IsAssignableFrom(t7));
            Assert.That(typeof(ISwAuxiliaryDrawingView).IsAssignableFrom(t8));
            Assert.That(typeof(ISwSectionDrawingView).IsAssignableFrom(t9));
            Assert.That(typeof(ISwSectionDrawingView).IsAssignableFrom(t10));
            Assert.That(typeof(ISwDetailDrawingView).IsAssignableFrom(t11));
        }

        [Test]
        public void ViewDependencyTypesTest()
        {
            string[] d1;
            string[] d2;
            string[] d3;
            string b1;
            string b2;

            using (var doc = OpenDataDocument("Drawing3\\Drawing3.SLDDRW"))
            {
                var sheet = (m_App.Documents.Active as ISwDrawing).Sheets.First();

                d1 = sheet.DrawingViews["Drawing View1"].DependentViews.Select(v => v.Name).ToArray();
                d2 = sheet.DrawingViews["Drawing View4"].DependentViews.Select(v => v.Name).ToArray();
                d3 = sheet.DrawingViews["Drawing View3"].DependentViews.Select(v => v.Name).ToArray();
                b1 = sheet.DrawingViews["Drawing View1"].BaseView?.Name;
                b2 = sheet.DrawingViews["Drawing View2"].BaseView?.Name;
            }

            CollectionAssert.AreEquivalent(new string[] { "Drawing View2", "Drawing View3", "Detail View C (4 : 1)" }, d1);
            CollectionAssert.AreEquivalent(new string[] { "Drawing View5", "Drawing View6" }, d2);
            CollectionAssert.AreEquivalent(new string[] { "Section View B-B" }, d3);
            Assert.IsNull(b1);
            Assert.AreEqual("Drawing View1", b2);
        }
    }
}

