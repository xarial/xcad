using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Documents.Structures;
using Xarial.XCad.SwDocumentManager.Documents;

namespace SolidWorksDocMgr.Tests.Integration
{
    public class DrawingTest : IntegrationTests
    {
        [Test]
        public void ActiveSheetTest()
        {
            string name;

            using (var doc = OpenDataDocument("Sheets1.SLDDRW"))
            {
                name = (m_App.Documents.Active as ISwDmDrawing).Sheets.Active.Name;
            }

            Assert.AreEqual("Sheet2", name);
        }

        [Test]
        public void IterateSheetsTest()
        {
            string[] confNames;

            using (var doc = OpenDataDocument("Sheets1.SLDDRW"))
            {
                confNames = (m_App.Documents.Active as ISwDmDrawing).Sheets.Select(x => x.Name).ToArray();
            }

            CollectionAssert.AreEquivalent(confNames, new string[]
            {
                "Sheet1", "Sheet2", "MySheet", "Sheet3"
            });
        }

        [Test]
        public void SheetPropertiesTest()
        {
            Scale scale1;
            Scale scale2;
            Scale scale3;

            PaperSize paperSize1;
            PaperSize paperSize2;
            PaperSize paperSize3;

            using (var doc = OpenDataDocument("Drawing1.SLDDRW"))
            {
                var sheets = (m_App.Documents.Active as ISwDmDrawing).Sheets;

                var sheet1 = sheets["Sheet1"];
                var sheet2 = sheets["Sheet2"];
                var sheet3 = sheets["Sheet3"];

                scale1 = sheet1.Scale;
                paperSize1 = sheet1.PaperSize;

                scale2 = sheet2.Scale;
                paperSize2 = sheet2.PaperSize;

                scale3 = sheet3.Scale;
                paperSize3 = sheet3.PaperSize;
            }

            Assert.AreEqual(2, scale1.Numerator);
            Assert.AreEqual(5, scale1.Denominator);
            Assert.That(paperSize1.StandardPaperSize.HasValue);
            Assert.AreEqual(StandardPaperSize_e.A2Landscape, paperSize1.StandardPaperSize.Value);

            Assert.AreEqual(3, scale2.Numerator);
            Assert.AreEqual(1, scale2.Denominator);
            Assert.That(paperSize2.StandardPaperSize.HasValue);
            Assert.AreEqual(StandardPaperSize_e.A4Portrait, paperSize2.StandardPaperSize.Value);

            Assert.AreEqual(1, scale3.Numerator);
            Assert.AreEqual(1, scale3.Denominator);
            Assert.That(!paperSize3.StandardPaperSize.HasValue);
            Assert.AreEqual(0.25, paperSize3.Width);
            Assert.AreEqual(0.15, paperSize3.Height);
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
                var sheets = (m_App.Documents.Active as ISwDmDrawing).Sheets;

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
        public void DrawingViewIterateTest()
        {
            string[] sheet1Views;
            string[] sheet2Views;

            using (var doc = OpenDataDocument("Drawing1\\Drawing1.SLDDRW"))
            {
                var sheets = (m_App.Documents.Active as ISwDmDrawing).Sheets;
                sheet1Views = sheets["Sheet1"].DrawingViews.Select(v => v.Name).ToArray();
                sheet2Views = sheets["Sheet2"].DrawingViews.Select(v => v.Name).ToArray();
            }

            CollectionAssert.AreEquivalent(sheet1Views, new string[] { "Drawing View1", "Drawing View2", "Drawing View3" });
            CollectionAssert.AreEquivalent(sheet2Views, new string[] { "Drawing View4", "Drawing View5" });
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
                var sheets = (m_App.Documents.Active as ISwDmDrawing).Sheets;

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
    }
}
