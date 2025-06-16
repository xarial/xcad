using NUnit.Framework;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Documents.Structures;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks.Annotations;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry;

namespace SolidWorks.Tests.Integration
{
    public class DrawingTest : IntegrationTests
    {
        [Test]
        public void NewDrawingTest() 
        {
            int drw1SheetsCount;
            double[] drw1Prps;

            int drw2SheetsCount;
            string drw2SheetName;
            double[] drw2Prps;

            var drw1 = m_App.Documents.PreCreate<ISwDrawing>();
            drw1.Commit();

            drw1SheetsCount = (drw1.Drawing.GetSheetNames() as string[]).Length;
            drw1Prps = (double[])drw1.Drawing.Sheet[(drw1.Drawing.GetSheetNames() as string[]).First()].GetProperties2();

            var drw2 = m_App.Documents.PreCreate<ISwDrawing>();
            drw2.Sheets.First().PaperSize = new PaperSize(StandardPaperSize_e.A2Landscape);
            drw2.Sheets.First().Scale = new Scale(5, 3);
            drw2.Sheets.First().Name = "My Sheet 1";
            drw2.Commit();

            drw2SheetsCount = (drw2.Drawing.GetSheetNames() as string[]).Length;
            drw2SheetName = (drw2.Drawing.GetSheetNames() as string[]).First();
            drw2Prps = (double[])drw2.Drawing.Sheet[(drw2.Drawing.GetSheetNames() as string[]).First()].GetProperties2();

            drw1.Close();
            drw2.Close();

            Assert.AreEqual(1, drw1SheetsCount);
            Assert.AreEqual((int)swDwgPaperSizes_e.swDwgPapersUserDefined, (int)drw1Prps[0]);
            Assert.AreEqual(1, drw1Prps[2]);
            Assert.AreEqual(1, drw1Prps[3]);
            Assert.AreEqual(0.1, drw1Prps[5]);
            Assert.AreEqual(0.1, drw1Prps[6]);

            Assert.AreEqual(1, drw2SheetsCount);
            Assert.AreEqual("My Sheet 1", drw2SheetName);
            Assert.AreEqual((int)swDwgPaperSizes_e.swDwgPaperA2size, (int)drw2Prps[0]);
            Assert.AreEqual(5, drw2Prps[2]);
            Assert.AreEqual(3, drw2Prps[3]);
        }

        [Test]
        public void NewDrawingCustomSheetsTest()
        {
            var drw = m_App.Documents.PreCreate<ISwDrawing>();

            var sheet1 = drw.Sheets.PreCreate<ISwSheet>();
            sheet1.Name = "TestSheet1";
            sheet1.PaperSize = new PaperSize(StandardPaperSize_e.A4Portrait);
            sheet1.Scale = new Scale(1, 1);

            var sheet2 = drw.Sheets.PreCreate<ISwSheet>();
            sheet2.Name = "TestSheet2";
            sheet2.PaperSize = new PaperSize(StandardPaperSize_e.A4Landscape);
            sheet2.Scale = new Scale(1, 2);

            var sheet3 = drw.Sheets.PreCreate<ISwSheet>();
            sheet3.Name = "Sheet1";
            
            drw.Sheets.AddRange(new ISwSheet[] { sheet1, sheet2, sheet3 });

            drw.Commit();

            var sheetNames = (string[])drw.Drawing.GetSheetNames();
            var prps1 = (double[])drw.Drawing.Sheet["TestSheet1"]?.GetProperties2();
            var prps2 = (double[])drw.Drawing.Sheet["TestSheet2"]?.GetProperties2();
            var prps3 = (double[])drw.Drawing.Sheet["Sheet1"]?.GetProperties2();

            drw.Close();

            Assert.IsNotNull(drw);
            Assert.That(sheetNames.SequenceEqual(new string[] { "TestSheet1", "TestSheet2", "Sheet1" }));
            Assert.AreEqual((double)swDwgPaperSizes_e.swDwgPaperA4sizeVertical, prps1[0]);
            Assert.AreEqual(1, prps1[2]);
            Assert.AreEqual(1, prps1[3]);
            Assert.AreEqual((double)swDwgPaperSizes_e.swDwgPaperA4size, prps2[0]);
            Assert.AreEqual(1, prps2[2]);
            Assert.AreEqual(2, prps2[3]);
            Assert.AreEqual((int)swDwgPaperSizes_e.swDwgPapersUserDefined, (int)prps3[0]);
            Assert.AreEqual(1, prps3[2]);
            Assert.AreEqual(1, prps3[3]);
            Assert.AreEqual(0.1, prps3[5]);
            Assert.AreEqual(0.1, prps3[6]);
        }

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
                var sheets = (m_App.Documents.Active as ISwDrawing).Sheets;

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
        public void CreateModelViewBasedTest() 
        {
            var refDocPathName = "";
            
            string o1;
            string o2;
            string o3;
            string o4;
            string o5;
            string o6;
            string o7;
            string o8;
            string o9;

            using (var doc = OpenDataDocument("Selections1.SLDPRT"))
            {
                var partDoc = m_App.Documents.Active as IXDocument3D;
                
                using(var drw = NewDocument(swDocumentTypes_e.swDocDRAWING))
                {
                    var drwDoc = m_App.Documents.Active as ISwDrawing;

                    var v1 = (ISwModelBasedDrawingView)drwDoc.Sheets.Active.DrawingViews.CreateModelViewBased(partDoc.ModelViews[StandardViewType_e.Back]);
                    var v2 = (ISwModelBasedDrawingView)drwDoc.Sheets.Active.DrawingViews.CreateModelViewBased(partDoc.ModelViews[StandardViewType_e.Bottom]);
                    var v3 = (ISwModelBasedDrawingView)drwDoc.Sheets.Active.DrawingViews.CreateModelViewBased(partDoc.ModelViews[StandardViewType_e.Dimetric]);
                    var v4 = (ISwModelBasedDrawingView)drwDoc.Sheets.Active.DrawingViews.CreateModelViewBased(partDoc.ModelViews[StandardViewType_e.Front]);
                    var v5 = (ISwModelBasedDrawingView)drwDoc.Sheets.Active.DrawingViews.CreateModelViewBased(partDoc.ModelViews[StandardViewType_e.Isometric]);
                    var v6 = (ISwModelBasedDrawingView)drwDoc.Sheets.Active.DrawingViews.CreateModelViewBased(partDoc.ModelViews[StandardViewType_e.Left]);
                    var v7 = (ISwModelBasedDrawingView)drwDoc.Sheets.Active.DrawingViews.CreateModelViewBased(partDoc.ModelViews[StandardViewType_e.Right]);
                    var v8 = (ISwModelBasedDrawingView)drwDoc.Sheets.Active.DrawingViews.CreateModelViewBased(partDoc.ModelViews[StandardViewType_e.Top]);
                    var v9 = (ISwModelBasedDrawingView)drwDoc.Sheets.Active.DrawingViews.CreateModelViewBased(partDoc.ModelViews[StandardViewType_e.Trimetric]);

                    refDocPathName = v1.DrawingView.ReferencedDocument.GetPathName();
                    
                    o1 = v1.DrawingView.GetOrientationName();
                    o2 = v2.DrawingView.GetOrientationName();
                    o3 = v3.DrawingView.GetOrientationName();
                    o4 = v4.DrawingView.GetOrientationName();
                    o5 = v5.DrawingView.GetOrientationName();
                    o6 = v6.DrawingView.GetOrientationName();
                    o7 = v7.DrawingView.GetOrientationName();
                    o8 = v8.DrawingView.GetOrientationName();
                    o9 = v9.DrawingView.GetOrientationName();
                }
            }

            Assert.AreEqual(GetFilePath("Selections1.SLDPRT"), refDocPathName);
            Assert.AreEqual("*Back", o1);
            Assert.AreEqual("*Bottom", o2);
            Assert.AreEqual("*Dimetric", o3);
            Assert.AreEqual("*Front", o4);
            Assert.AreEqual("*Isometric", o5);
            Assert.AreEqual("*Left", o6);
            Assert.AreEqual("*Right", o7);
            Assert.AreEqual("*Top", o8);
            Assert.AreEqual("*Trimetric", o9);
        }

        [Test]
        public void CreateFlatPatternNotOpenedTest()
        {
            bool view1IsFlatPattern;
            string view1Conf;
            int edgeCount1;

            using (var drw = NewDocument(swDocumentTypes_e.swDocDRAWING))
            {
                var drwDoc = m_App.Documents.Active as ISwDrawing;

                var drwView1 = drwDoc.Sheets.Active.DrawingViews.PreCreate<ISwFlatPatternDrawingView>();

                var refDoc = m_App.Documents.PreCreate<IXPart>();
                refDoc.Path = GetFilePath("FlatPattern1.SLDPRT");

                drwView1.ReferencedDocument = refDoc;
                drwView1.Commit();

                view1IsFlatPattern = drwView1.DrawingView.IsFlatPatternView();
                view1Conf = drwView1.DrawingView.ReferencedConfiguration;
                edgeCount1 = drwView1.DrawingView.GetVisibleEntityCount2((Component2)(drwView1.DrawingView.GetVisibleComponents() as object[]).First(), (int)swViewEntityType_e.swViewEntityType_Edge);
            }

            Assert.IsTrue(view1IsFlatPattern);
            Assert.AreEqual(12, edgeCount1);
            Assert.That(view1Conf.StartsWith("DefaultSM-FLAT-PATTERN"));
        }

        [Test]
        public void CreateFlatPatternMultiBodyTest()
        {
            bool view1IsFlatPattern;
            bool view2IsFlatPattern;

            string view1Conf;
            string view2Conf;

            int edgeCount1;
            int edgeCount2;

            using (var doc = OpenDataDocument("FlatPattern2.SLDPRT"))
            {
                var partDoc = m_App.Documents.Active as IXPart;

                using (var drw = NewDocument(swDocumentTypes_e.swDocDRAWING))
                {
                    var drwDoc = m_App.Documents.Active as ISwDrawing;
                    
                    var drwView1 = drwDoc.Sheets.Active.DrawingViews.PreCreate<ISwFlatPatternDrawingView>();
                    drwView1.SheetMetalBody = (IXSolidBody)partDoc.Bodies["Edge-Flange2"];
                    drwView1.Commit();

                    var drwView2 = drwDoc.Sheets.Active.DrawingViews.PreCreate<ISwFlatPatternDrawingView>();
                    drwView2.SheetMetalBody = (IXSolidBody)partDoc.Bodies["Hem1"];
                    drwView2.Commit();

                    view1IsFlatPattern = drwView1.DrawingView.IsFlatPatternView();
                    view1Conf = drwView1.DrawingView.ReferencedConfiguration;
                    edgeCount1 = drwView1.DrawingView.GetVisibleEntityCount2((Component2)(drwView1.DrawingView.GetVisibleComponents() as object[]).First(), (int)swViewEntityType_e.swViewEntityType_Edge);

                    view2IsFlatPattern = drwView2.DrawingView.IsFlatPatternView();
                    view2Conf = drwView2.DrawingView.ReferencedConfiguration;
                    edgeCount2 = drwView2.DrawingView.GetVisibleEntityCount2((Component2)(drwView2.DrawingView.GetVisibleComponents() as object[]).First(), (int)swViewEntityType_e.swViewEntityType_Edge);
                }
            }

            Assert.IsTrue(view1IsFlatPattern);
            Assert.IsTrue(view2IsFlatPattern);
            Assert.AreEqual(6, edgeCount1);
            Assert.AreEqual(5, edgeCount2);
            Assert.That(view1Conf.StartsWith("DefaultSM-FLAT-PATTERN"));
            Assert.That(view2Conf.StartsWith("DefaultSM-FLAT-PATTERN"));
            Assert.AreNotEqual(view1Conf, view2Conf);
        }

        [Test]
        public void CreateFlatPatternMultiBodyMultiConfAssemblyTest()
        {
            bool view1IsFlatPattern;
            bool view2IsFlatPattern;

            string view1Conf;
            string view2Conf;

            int edgeCount1;
            int edgeCount2;

            using (var doc = OpenDataDocument(@"SheetMetalAssembly1\Assem1.SLDASM"))
            {
                var assmDoc = m_App.Documents.Active as IXAssembly;

                var comp1 = assmDoc.Configurations.Active.Components["Part1-1"];
                var comp2 = assmDoc.Configurations.Active.Components["Part1-2"];

                var partDoc = (IXPart)assmDoc.Dependencies.First();

                using (var drw = NewDocument(swDocumentTypes_e.swDocDRAWING))
                {
                    var drwDoc = m_App.Documents.Active as ISwDrawing;

                    var drwView1 = drwDoc.Sheets.Active.DrawingViews.PreCreate<ISwFlatPatternDrawingView>();
                    drwView1.SheetMetalBody = (IXSolidBody)comp1.Bodies["Cut-Extrude1"];
                    drwView1.ReferencedDocument = partDoc;
                    drwView1.ReferencedConfiguration = comp1.ReferencedConfiguration;
                    drwView1.Commit();

                    var drwView2 = drwDoc.Sheets.Active.DrawingViews.PreCreate<ISwFlatPatternDrawingView>();
                    drwView2.SheetMetalBody = (IXSolidBody)comp2.Bodies["Edge-Flange2"];
                    drwView2.ReferencedDocument = partDoc;
                    drwView2.ReferencedConfiguration = comp2.ReferencedConfiguration;
                    drwView2.Commit();

                    view1IsFlatPattern = drwView1.DrawingView.IsFlatPatternView();
                    view1Conf = drwView1.DrawingView.ReferencedConfiguration;
                    edgeCount1 = drwView1.DrawingView.GetVisibleEntityCount2((Component2)(drwView1.DrawingView.GetVisibleComponents() as object[]).First(), (int)swViewEntityType_e.swViewEntityType_Edge);

                    view2IsFlatPattern = drwView2.DrawingView.IsFlatPatternView();
                    view2Conf = drwView2.DrawingView.ReferencedConfiguration;
                    edgeCount2 = drwView2.DrawingView.GetVisibleEntityCount2((Component2)(drwView2.DrawingView.GetVisibleComponents() as object[]).First(), (int)swViewEntityType_e.swViewEntityType_Edge);
                }
            }

            Assert.IsTrue(view1IsFlatPattern);
            Assert.IsTrue(view2IsFlatPattern);
            Assert.AreEqual(5, edgeCount1);
            Assert.AreEqual(12, edgeCount2);
            Assert.That(view1Conf.StartsWith("Conf1SM-FLAT-PATTERN"));
            Assert.That(view2Conf.StartsWith("DefaultSM-FLAT-PATTERN"));
            Assert.AreNotEqual(view1Conf, view2Conf);
        }

        [Test]
        public void CreateFlatPatternMultiConfTest()
        {
            bool view1IsFlatPattern;
            bool view2IsFlatPattern;

            string view1Conf;
            string view2Conf;

            int edgeCount1;
            int edgeCount2;

            using (var doc = OpenDataDocument("SheetMetal2.SLDPRT"))
            {
                var partDoc = m_App.Documents.Active as IXPart;

                using (var drw = NewDocument(swDocumentTypes_e.swDocDRAWING))
                {
                    var drwDoc = m_App.Documents.Active as ISwDrawing;

                    var drwView1 = drwDoc.Sheets.Active.DrawingViews.PreCreate<ISwFlatPatternDrawingView>();
                    drwView1.ReferencedDocument = partDoc;
                    drwView1.ReferencedConfiguration = partDoc.Configurations["Default"];
                    drwView1.Commit();

                    var drwView2 = drwDoc.Sheets.Active.DrawingViews.PreCreate<ISwFlatPatternDrawingView>();
                    drwView2.ReferencedDocument = partDoc;
                    drwView2.ReferencedConfiguration = partDoc.Configurations["Conf1"];
                    drwView2.Commit();

                    view1IsFlatPattern = drwView1.DrawingView.IsFlatPatternView();
                    view1Conf = drwView1.DrawingView.ReferencedConfiguration;
                    edgeCount1 = drwView1.DrawingView.GetVisibleEntityCount2((Component2)(drwView1.DrawingView.GetVisibleComponents() as object[]).First(), (int)swViewEntityType_e.swViewEntityType_Edge);

                    view2IsFlatPattern = drwView2.DrawingView.IsFlatPatternView();
                    view2Conf = drwView2.DrawingView.ReferencedConfiguration;
                    edgeCount2 = drwView2.DrawingView.GetVisibleEntityCount2((Component2)(drwView2.DrawingView.GetVisibleComponents() as object[]).First(), (int)swViewEntityType_e.swViewEntityType_Edge);
                }
            }

            Assert.IsTrue(view1IsFlatPattern);
            Assert.IsTrue(view2IsFlatPattern);
            Assert.AreEqual(6, edgeCount1);
            Assert.AreEqual(8, edgeCount2);
            Assert.That(view1Conf.StartsWith("DefaultSM-FLAT-PATTERN"));
            Assert.That(view2Conf.StartsWith("Conf1SM-FLAT-PATTERN"));
            Assert.AreNotEqual(view1Conf, view2Conf);
        }

        [Test]
        public void CreateFlatPatternOptionsTest()
        {
            int GenBendNotesCount(IView view)
                => ((object[])view.GetNotes() ?? new object[0]).Count(n => ((INote)n).IsBendLineNote);

            bool view1IsFlatPattern;
            int view1EdgeCount;
            int view1BendLinesCount;
            int view1BendNotesCount;

            bool view2IsFlatPattern;
            int view2EdgeCount;
            int view2BendLinesCount;
            int view2BendNotesCount;

            bool view3IsFlatPattern;
            int view3EdgeCount;
            int view3BendLinesCount;
            int view3BendNotesCount;

            bool view4IsFlatPattern;
            int view4EdgeCount;
            int view4BendLinesCount;
            int view4BendNotesCount;

            bool view5IsFlatPattern;
            int view5EdgeCount;
            int view5BendLinesCount;
            int view5BendNotesCount;

            using (var drw = NewDocument(swDocumentTypes_e.swDocDRAWING))
            {
                var drwDoc = m_App.Documents.Active as ISwDrawing;

                var refDoc1 = m_App.Documents.PreCreate<IXPart>();
                refDoc1.Path = GetFilePath("SheetMetal3.SLDPRT");

                var drwView1 = drwDoc.Sheets.Active.DrawingViews.PreCreate<ISwFlatPatternDrawingView>();
                drwView1.ReferencedDocument = refDoc1;
                drwView1.Options = FlatPatternViewOptions_e.BendNotes | FlatPatternViewOptions_e.BendLines;
                drwView1.Commit();

                var drwView2 = drwDoc.Sheets.Active.DrawingViews.PreCreate<ISwFlatPatternDrawingView>();
                drwView2.ReferencedDocument = refDoc1;
                drwView2.Options = FlatPatternViewOptions_e.Default;
                drwView2.Commit();

                var refDoc2 = m_App.Documents.PreCreate<IXPart>();
                refDoc2.Path = GetFilePath("SheetMetal4.SLDPRT");

                var drwView3 = drwDoc.Sheets.Active.DrawingViews.PreCreate<ISwFlatPatternDrawingView>();
                drwView3.ReferencedDocument = refDoc2;
                drwView3.Options = FlatPatternViewOptions_e.BendNotes | FlatPatternViewOptions_e.BendLines;
                drwView3.Commit();

                var drwView4 = drwDoc.Sheets.Active.DrawingViews.PreCreate<ISwFlatPatternDrawingView>();
                drwView4.ReferencedDocument = refDoc2;
                drwView4.Options = FlatPatternViewOptions_e.BendLines;
                drwView4.Commit();

                var drwView5 = drwDoc.Sheets.Active.DrawingViews.PreCreate<ISwFlatPatternDrawingView>();
                drwView5.ReferencedDocument = refDoc2;
                drwView5.Options = FlatPatternViewOptions_e.Default;
                drwView5.Commit();

                view1IsFlatPattern = drwView1.DrawingView.IsFlatPatternView();
                view1EdgeCount = drwView1.DrawingView.GetVisibleEntityCount2((Component2)(drwView1.DrawingView.GetVisibleComponents() as object[]).First(), (int)swViewEntityType_e.swViewEntityType_Edge);
                view1BendLinesCount = drwView1.DrawingView.GetBendLineCount();
                view1BendNotesCount = GenBendNotesCount(drwView1.DrawingView);

                view2IsFlatPattern = drwView2.DrawingView.IsFlatPatternView();
                view2EdgeCount = drwView2.DrawingView.GetVisibleEntityCount2((Component2)(drwView2.DrawingView.GetVisibleComponents() as object[]).First(), (int)swViewEntityType_e.swViewEntityType_Edge);
                view2BendLinesCount = drwView2.DrawingView.GetBendLineCount();
                view2BendNotesCount = GenBendNotesCount(drwView2.DrawingView);

                view3IsFlatPattern = drwView3.DrawingView.IsFlatPatternView();
                view3EdgeCount = drwView3.DrawingView.GetVisibleEntityCount2((Component2)(drwView3.DrawingView.GetVisibleComponents() as object[]).First(), (int)swViewEntityType_e.swViewEntityType_Edge);
                view3BendLinesCount = drwView3.DrawingView.GetBendLineCount();
                view3BendNotesCount = GenBendNotesCount(drwView3.DrawingView);

                view4IsFlatPattern = drwView4.DrawingView.IsFlatPatternView();
                view4EdgeCount = drwView4.DrawingView.GetVisibleEntityCount2((Component2)(drwView4.DrawingView.GetVisibleComponents() as object[]).First(), (int)swViewEntityType_e.swViewEntityType_Edge);
                view4BendLinesCount = drwView4.DrawingView.GetBendLineCount();
                view4BendNotesCount = GenBendNotesCount(drwView4.DrawingView);

                view5IsFlatPattern = drwView5.DrawingView.IsFlatPatternView();
                view5EdgeCount = drwView5.DrawingView.GetVisibleEntityCount2((Component2)(drwView5.DrawingView.GetVisibleComponents() as object[]).First(), (int)swViewEntityType_e.swViewEntityType_Edge);
                view5BendLinesCount = drwView5.DrawingView.GetBendLineCount();
                view5BendNotesCount = GenBendNotesCount(drwView5.DrawingView);
            }

            Assert.IsTrue(view1IsFlatPattern);
            Assert.AreEqual(4, view1EdgeCount);
            Assert.AreEqual(0, view1BendLinesCount);
            Assert.AreEqual(0, view1BendNotesCount);

            Assert.IsTrue(view2IsFlatPattern);
            Assert.AreEqual(4, view2EdgeCount);
            Assert.AreEqual(0, view2BendLinesCount);
            Assert.AreEqual(0, view2BendNotesCount);

            Assert.IsTrue(view3IsFlatPattern);
            Assert.AreEqual(8, view3EdgeCount);
            Assert.AreEqual(2, view3BendLinesCount);
            Assert.AreEqual(2, view3BendNotesCount);

            Assert.IsTrue(view4IsFlatPattern);
            Assert.AreEqual(8, view4EdgeCount);
            Assert.AreEqual(2, view4BendLinesCount);
            Assert.AreEqual(0, view4BendNotesCount);

            Assert.IsTrue(view5IsFlatPattern);
            Assert.AreEqual(8, view5EdgeCount);
            Assert.AreEqual(0, view5BendLinesCount);
            Assert.AreEqual(0, view5BendNotesCount);
        }

        [Test]
        public void CreateProjectedViewTest()
        {
            double[] t1;
            double[] t2;
            double[] t3;
            double[] t4;
            double[] t5;
            double[] t6;
            double[] t7;
            double[] t8;

            double[] o1;
            double[] o2;
            double[] o3;
            double[] o4;
            double[] o5;
            double[] o6;
            double[] o7;
            double[] o8;

            using (var doc = OpenDataDocument("Drawing4\\Drawing4.slddrw"))
            {
                var drwDoc = m_App.Documents.Active as ISwDrawing;

                var sheet = drwDoc.Sheets["Sheet1"];

                var srcView = sheet.DrawingViews.First();

                var v1 = sheet.DrawingViews.PreCreate<ISwProjectedDrawingView>();
                v1.BaseView = srcView;
                v1.Direction = ProjectedViewDirection_e.Bottom;
                v1.Commit();

                var v2 = sheet.DrawingViews.PreCreate<ISwProjectedDrawingView>();
                v2.BaseView = srcView;
                v2.Direction = ProjectedViewDirection_e.IsoBottomLeft;
                v2.Commit();

                var v3 = sheet.DrawingViews.PreCreate<ISwProjectedDrawingView>();
                v3.BaseView = srcView;
                v3.Direction = ProjectedViewDirection_e.IsoBottomRight;
                v3.Commit();

                var v4 = sheet.DrawingViews.PreCreate<ISwProjectedDrawingView>();
                v4.BaseView = srcView;
                v4.Direction = ProjectedViewDirection_e.IsoTopLeft;
                v4.Commit();

                var v5 = sheet.DrawingViews.PreCreate<ISwProjectedDrawingView>();
                v5.BaseView = srcView;
                v5.Direction = ProjectedViewDirection_e.IsoTopRight;
                v5.Commit();

                var v6 = sheet.DrawingViews.PreCreate<ISwProjectedDrawingView>();
                v6.BaseView = srcView;
                v6.Direction = ProjectedViewDirection_e.Left;
                v6.Commit();

                var v7 = sheet.DrawingViews.PreCreate<ISwProjectedDrawingView>();
                v7.BaseView = srcView;
                v7.Direction = ProjectedViewDirection_e.Right;
                v7.Commit();

                var v8 = sheet.DrawingViews.PreCreate<ISwProjectedDrawingView>();
                v8.BaseView = srcView;
                v8.Direction = ProjectedViewDirection_e.Top;
                v8.Commit();

                t1 = (double[])v1.DrawingView.ModelToViewTransform.ArrayData;
                t2 = (double[])v2.DrawingView.ModelToViewTransform.ArrayData;
                t3 = (double[])v3.DrawingView.ModelToViewTransform.ArrayData;
                t4 = (double[])v4.DrawingView.ModelToViewTransform.ArrayData;
                t5 = (double[])v5.DrawingView.ModelToViewTransform.ArrayData;
                t6 = (double[])v6.DrawingView.ModelToViewTransform.ArrayData;
                t7 = (double[])v7.DrawingView.ModelToViewTransform.ArrayData;
                t8 = (double[])v8.DrawingView.ModelToViewTransform.ArrayData;

                o1 = (double[])v1.DrawingView.GetOutline();
                o2 = (double[])v2.DrawingView.GetOutline();
                o3 = (double[])v3.DrawingView.GetOutline();
                o4 = (double[])v4.DrawingView.GetOutline();
                o5 = (double[])v5.DrawingView.GetOutline();
                o6 = (double[])v6.DrawingView.GetOutline();
                o7 = (double[])v7.DrawingView.GetOutline();
                o8 = (double[])v8.DrawingView.GetOutline();
            }

            AssertCompareDoubleArray(t1, new double[] { 1, 0, 0, 0, 0, -1, 0, 1, 0, 0.587737905405405, 0.323514932432432, 0, 1, 0, 0, 0 }, 8, 5);
            AssertCompareDoubleArray(t2, new double[] { 0.707106781186546, -0.408204055911355, -0.577381545199984, 0, 0.81654081188575, -0.577287712085543, 0.707106781186549, 0.408204055911354, 0.577381545199982, 0.386120969982898, 0.285537997009925, 0, 1, 0, 0, 0 }, 8, 5);
            AssertCompareDoubleArray(t3, new double[] { 0.707106781186546, 0.408204055911355, 0.577381545199984, 0, 0.81654081188575, -0.577287712085543, -0.707106781186549, 0.408204055911354, 0.577381545199982, 0.789354840827913, 0.285537997009925, 0, 1, 0, 0, 0 }, 8, 5);
            AssertCompareDoubleArray(t4, new double[] { 0.707106781186546, 0.408204055911355, -0.577381545199984, 0, 0.81654081188575, 0.577287712085543, 0.707106781186549, -0.408204055911354, 0.577381545199982, 0.386120969982898, 0.68877186785494, 0, 1, 0, 0, 0 }, 8, 5);
            AssertCompareDoubleArray(t5, new double[] { 0.707106781186546, -0.408204055911355, 0.577381545199984, 0, 0.81654081188575, 0.577287712085543, -0.707106781186549, -0.408204055911354, 0.577381545199982, 0.789354840827913, 0.68877186785494, 0, 1, 0, 0, 0 }, 8, 5);
            AssertCompareDoubleArray(t6, new double[] { 0, 0, -1, 0, 1, 0, 1, 0, 0, 0.424097905405405, 0.487154932432432, 0, 1, 0, 0, 0 }, 8, 5);
            AssertCompareDoubleArray(t7, new double[] { 0, 0, 1, 0, 1, 0, -1, 0, 0, 0.751377905405406, 0.487154932432432, 0, 1, 0, 0, 0 }, 8, 5);
            AssertCompareDoubleArray(t8, new double[] { 1, 0, 0, 0, 0, 1, 0, -1, 0, 0.587737905405405, 0.650794932432433, 0, 1, 0, 0, 0 }, 8, 5);

            AssertCompareDoubleArray(o1, new double[] { 0.510917905405405, 0.239874932432432, 0.664557905405405, 0.393514932432432 }, 8, 5);
            AssertCompareDoubleArray(o2, new double[] { 0.288917941265718, 0.175210846612623, 0.492263568750489, 0.404804717457638 }, 8, 5);
            AssertCompareDoubleArray(o3, new double[] { 0.683212242060321, 0.175210846612623, 0.886557869545093, 0.404804717457638 }, 8, 5);
            AssertCompareDoubleArray(o4, new double[] { 0.288917941265718, 0.569505147407226, 0.492263568750489, 0.799099018252241 }, 8, 5);
            AssertCompareDoubleArray(o5, new double[] { 0.683212242060321, 0.569505147407226, 0.886557869545093, 0.799099018252241 }, 8, 5);
            AssertCompareDoubleArray(o6, new double[] { 0.340457905405405, 0.410334932432432, 0.494097905405405, 0.563974932432432 }, 8, 5);
            AssertCompareDoubleArray(o7, new double[] { 0.681377905405405, 0.410334932432432, 0.835017905405406, 0.563974932432432 }, 8, 5);
            AssertCompareDoubleArray(o8, new double[] { 0.510917905405405, 0.580794932432432, 0.664557905405405, 0.734434932432433 }, 8, 5);
        }

        [Test]
        public void CreateProjectedViewHiddenBodiesTest()
        {
            double[] t1;
            double[] t2;
            double[] t3;
            double[] t4;
            double[] t5;
            double[] t6;
            double[] t7;
            double[] t8;

            double[] o1;
            double[] o2;
            double[] o3;
            double[] o4;
            double[] o5;
            double[] o6;
            double[] o7;
            double[] o8;

            using (var doc = OpenDataDocument("Drawing7\\Drawing7.slddrw"))
            {
                var drwDoc = m_App.Documents.Active as ISwDrawing;

                var sheet = drwDoc.Sheets["Sheet1"];

                var srcView = sheet.DrawingViews.First();

                var v1 = sheet.DrawingViews.PreCreate<ISwProjectedDrawingView>();
                v1.BaseView = srcView;
                v1.Direction = ProjectedViewDirection_e.Bottom;
                v1.Commit();

                var v2 = sheet.DrawingViews.PreCreate<ISwProjectedDrawingView>();
                v2.BaseView = srcView;
                v2.Direction = ProjectedViewDirection_e.IsoBottomLeft;
                v2.Commit();

                var v3 = sheet.DrawingViews.PreCreate<ISwProjectedDrawingView>();
                v3.BaseView = srcView;
                v3.Direction = ProjectedViewDirection_e.IsoBottomRight;
                v3.Commit();

                var v4 = sheet.DrawingViews.PreCreate<ISwProjectedDrawingView>();
                v4.BaseView = srcView;
                v4.Direction = ProjectedViewDirection_e.IsoTopLeft;
                v4.Commit();

                var v5 = sheet.DrawingViews.PreCreate<ISwProjectedDrawingView>();
                v5.BaseView = srcView;
                v5.Direction = ProjectedViewDirection_e.IsoTopRight;
                v5.Commit();

                var v6 = sheet.DrawingViews.PreCreate<ISwProjectedDrawingView>();
                v6.BaseView = srcView;
                v6.Direction = ProjectedViewDirection_e.Left;
                v6.Commit();

                var v7 = sheet.DrawingViews.PreCreate<ISwProjectedDrawingView>();
                v7.BaseView = srcView;
                v7.Direction = ProjectedViewDirection_e.Right;
                v7.Commit();

                var v8 = sheet.DrawingViews.PreCreate<ISwProjectedDrawingView>();
                v8.BaseView = srcView;
                v8.Direction = ProjectedViewDirection_e.Top;
                v8.Commit();

                t1 = (double[])v1.DrawingView.ModelToViewTransform.ArrayData;
                t2 = (double[])v2.DrawingView.ModelToViewTransform.ArrayData;
                t3 = (double[])v3.DrawingView.ModelToViewTransform.ArrayData;
                t4 = (double[])v4.DrawingView.ModelToViewTransform.ArrayData;
                t5 = (double[])v5.DrawingView.ModelToViewTransform.ArrayData;
                t6 = (double[])v6.DrawingView.ModelToViewTransform.ArrayData;
                t7 = (double[])v7.DrawingView.ModelToViewTransform.ArrayData;
                t8 = (double[])v8.DrawingView.ModelToViewTransform.ArrayData;

                o1 = (double[])v1.DrawingView.GetOutline();
                o2 = (double[])v2.DrawingView.GetOutline();
                o3 = (double[])v3.DrawingView.GetOutline();
                o4 = (double[])v4.DrawingView.GetOutline();
                o5 = (double[])v5.DrawingView.GetOutline();
                o6 = (double[])v6.DrawingView.GetOutline();
                o7 = (double[])v7.DrawingView.GetOutline();
                o8 = (double[])v8.DrawingView.GetOutline();
            }

            AssertCompareDoubleArray(t1, new double[] { -0.743556290714679, -2.3346605064371E-15, -0.668673345168347, 0, 1, -3.49148133884313E-15, 0.668673345168347, -2.59611291340972E-15, -0.743556290714679, 0.0547408008947171, 0.0564476828938053, 0.0133648726402715, 0.2, 0, 0, 0 }, 8, 5);
            AssertCompareDoubleArray(t2, new double[] { -0.525773695358265, 0.849521769818257, 0.0432987745111885, 0.707106781186549, 0.408204055911354, 0.577381545199982, 0.47282345676723, 0.334188885725388, -0.815325559140832, -0.0075554844322618, 0.0558347994283769, 0.0621268921683018, 0.2, 0, 0, 0 }, 8, 5);
            AssertCompareDoubleArray(t3, new double[] { -0.525773695358265, 0.242476382481988, -0.815332585640831, -0.707106781186549, 0.408204055911354, 0.577381545199982, 0.47282345676723, 0.880099228880454, -0.0431662606061474, 0.170832787129413, -0.0196943200236168, -0.0466961386706678, 0.2, 0, 0, 0 }, 8, 5);
            AssertCompareDoubleArray(t4, new double[] { -0.525773695358265, 0.242476382481988, 0.815332585640831, 0.707106781186549, -0.408204055911354, 0.577381545199982, 0.47282345676723, 0.880099228880454, 0.0431662606061474, -0.00614760627092219, 0.157286073376719, 0.0466961386706678, 0.2, 0, 0, 0 }, 8, 5);
            AssertCompareDoubleArray(t5, new double[] { -0.525773695358265, 0.849521769818257, -0.0432987745111885, -0.707106781186549, -0.408204055911354, 0.577381545199982, 0.47282345676723, 0.334188885725388, 0.815325559140832, 0.172240665290753, 0.235630949151392, -0.0621268921683018, 0.2, 0, 0, 0 }, 8, 5);
            AssertCompareDoubleArray(t6, new double[] { 2.59611291340972E-15, 0.668673345168347, 0.743556290714679, 1, 0, -3.49148133884313E-15, -2.3346605064371E-15, 0.743556290714679, -0.668673345168347, 0.0871286778020237, 0.104812466799383, 0.0942384041745543, 0.2, 0, 0, 0 }, 8, 5);
            AssertCompareDoubleArray(t7, new double[] { 2.59611291340972E-15, 0.668673345168347, -0.743556290714679, -1, 0, -3.49148133884313E-15, -2.3346605064371E-15, 0.743556290714679, 0.668673345168347, 0.21082973233652, 0.104812466799383, -0.0942384041745543, 0.2, 0, 0, 0 }, 8, 5);
            AssertCompareDoubleArray(t8, new double[] { -0.743556290714679, -2.3346605064371E-15, 0.668673345168347, 0, -1, -3.49148133884313E-15, 0.668673345168347, -2.59611291340972E-15, 0.743556290714679, 0.0547408008947171, 0.179906995985504, -0.0133648726402715, 0.2, 0, 0, 0 }, 8, 5);

            AssertCompareDoubleArray(o1, new double[] { 0.102869343262963, 0.0450250174328657, 0.19508906687558, 0.0678703483547448 }, 8, 5);
            AssertCompareDoubleArray(o2, new double[] { 0.0201880642006202, -0.014333479501821, 0.097974196214908, 0.0708920086581153 }, 8, 5);
            AssertCompareDoubleArray(o3, new double[] { 0.198576335762295, -0.0101885494797548, 0.276362467776583, 0.0695628349587284 }, 8, 5);
            AssertCompareDoubleArray(o4, new double[] { 0.0215959423619598, 0.166791843920581, 0.0993820743762476, 0.246543228359064 }, 8, 5);
            AssertCompareDoubleArray(o5, new double[] { 0.199984213923635, 0.165462670221194, 0.277770345937923, 0.25068815838113 }, 8, 5);
            AssertCompareDoubleArray(o6, new double[] { 0.0757060123410838, 0.0721883483547448, 0.0985513432629629, 0.164166330524564 }, 8, 5);
            AssertCompareDoubleArray(o7, new double[] { 0.19940706687558, 0.0721883483547448, 0.222252397797459, 0.164166330524564 }, 8, 5);
            AssertCompareDoubleArray(o8, new double[] { 0.102869343262963, 0.168484330524564, 0.19508906687558, 0.191329661446443 }, 8, 5);
        }

        [Test]
        public void CreateSectionViewTest()
        {
            int t1;

            using (var doc = OpenDataDocument("Drawing5\\Drawing5.slddrw"))
            {
                var drwDoc = m_App.Documents.Active as ISwDrawing;

                var sheet = drwDoc.Sheets["Sheet1"];

                var srcView = sheet.DrawingViews.First();

                var v1 = sheet.DrawingViews.PreCreate<ISwSectionDrawingView>();
                var sectionLine = v1.Annotations.PreCreate<ISwSectionLine>();
                sectionLine.Definition = new Line(new Point(0, -0.1, -0.065), new Point(0, 0.3, -0.065));

                v1.BaseView = srcView;
                v1.SectionLine = sectionLine;
                v1.Commit();

                t1 = v1.DrawingView.Type;
            }

            Assert.AreEqual((int)swDrawingViewTypes_e.swDrawingSectionView, t1);
        }

        [Test]
        public void CreateDetailedViewTest()
        {
            int t1;

            using (var doc = OpenDataDocument("Drawing5\\Drawing5.slddrw"))
            {
                var drwDoc = m_App.Documents.Active as ISwDrawing;

                var sheet = drwDoc.Sheets["Sheet1"];

                var srcView = sheet.DrawingViews.First();

                var v1 = sheet.DrawingViews.PreCreate<ISwDetailDrawingView>();
                var detCircle = v1.Annotations.PreCreate<ISwDetailCircle>();
                detCircle.Definition = new Circle(new Axis(new Point(0.15, 0.035, -0.06), new Vector(0, 0, 1)), 0.1);
                v1.BaseView = srcView;
                v1.DetailCircle = detCircle;
                v1.Commit();

                t1 = v1.DrawingView.Type;
            }

            Assert.AreEqual((int)swDrawingViewTypes_e.swDrawingDetailView, t1);
        }

        [Test]
        public void CreateRelativeViewTest()
        {
            int t1;
            int c1;

            using (var part = OpenDataDocument(@"Drawing9\Part1.sldprt"))
            {
                var partDoc = m_App.Documents.Active as ISwPart;

                var face1 = partDoc.CreateObjectFromDispatch<ISwPlanarFace>(partDoc.Part.GetEntityByName("FACE1", (int)swSelectType_e.swSelFACES));
                var face2 = partDoc.CreateObjectFromDispatch<ISwPlanarFace>(partDoc.Part.GetEntityByName("FACE2", (int)swSelectType_e.swSelFACES));

                using (var doc = NewDocument(swDocumentTypes_e.swDocDRAWING))
                {
                    var drwDoc = m_App.Documents.Active as ISwDrawing;

                    var sheet = drwDoc.Sheets.First();

                    var relView = sheet.DrawingViews.PreCreate<ISwRelativeDrawingView>();
                    relView.Orientation = new RelativeDrawingViewOrientation(face1, StandardViewType_e.Front, face2, StandardViewType_e.Bottom);
                    relView.Commit();

                    t1 = relView.DrawingView.Type;
                    c1 = ((object[])relView.DrawingView.GetVisibleEntities2((Component2)((object[])relView.DrawingView.GetVisibleComponents()).First(), (int)swViewEntityType_e.swViewEntityType_Edge)).Length;
                }
            }

            Assert.AreEqual((int)swDrawingViewTypes_e.swDrawingRelativeView, t1);
            Assert.AreEqual(4, c1);
        }

        [Test]
        public void InsertPredefinedViewTest()
        {
            string view1RefDocOrig;
            string view2RefDocOrig;
            string view3RefDocOrig;
            string view4RefDocOrig;

            string view1RefDoc;
            string view2RefDoc;
            string view3RefDoc;
            string view4RefDoc;

            using (var part = OpenDataDocument("TessPart1.SLDPRT"))
            {
                var part1 = (ISwPart)m_App.Documents.Active;

                using (var doc = OpenDataDocument("Drawing2.slddrw"))
                {
                    var drwDoc = m_App.Documents.Active as ISwDrawing;

                    var sheet1 = drwDoc.Sheets["Sheet1"];
                    var sheet2 = drwDoc.Sheets["Sheet2"];

                    var view1 = sheet1.DrawingViews["Drawing View1"];
                    var view2 = sheet1.DrawingViews["Drawing View2"];
                    var view3 = sheet1.DrawingViews["Drawing View3"];
                    var view4 = sheet2.DrawingViews["Drawing View4"];

                    view1RefDocOrig = view1.ReferencedDocument?.Path;
                    view2RefDocOrig = view2.ReferencedDocument?.Path;
                    view3RefDocOrig = view3.ReferencedDocument?.Path;
                    view4RefDocOrig = view4.ReferencedDocument?.Path;

                    var part2 = (ISwPart)m_App.Documents.PreCreateFromPath(GetFilePath("Selections1.SLDPRT"));
                    var assm3 = (ISwAssembly)m_App.Documents.PreCreateFromPath(GetFilePath(@"Assembly1\TopAssem1.SLDASM"));

                    view1.ReferencedDocument = part1;
                    view3.ReferencedDocument = part2;
                    view4.ReferencedDocument = assm3;

                    view1RefDoc = view1.ReferencedDocument?.Path;
                    view2RefDoc = view2.ReferencedDocument?.Path;
                    view3RefDoc = view3.ReferencedDocument?.Path;
                    view4RefDoc = view4.ReferencedDocument?.Path;
                }
            }
           
            Assert.IsNull(view1RefDocOrig);
            Assert.IsNull(view2RefDocOrig);
            Assert.IsNull(view3RefDocOrig);
            Assert.IsNull(view4RefDocOrig);

            Assert.That(string.Equals(view1RefDoc, GetFilePath("TessPart1.SLDPRT"), StringComparison.CurrentCultureIgnoreCase));
            Assert.That(string.Equals(view2RefDoc, GetFilePath("TessPart1.SLDPRT"), StringComparison.CurrentCultureIgnoreCase));
            Assert.That(string.Equals(view3RefDoc, GetFilePath("Selections1.SLDPRT"), StringComparison.CurrentCultureIgnoreCase));
            Assert.That(string.Equals(view4RefDoc, GetFilePath(@"Assembly1\TopAssem1.SLDASM"), StringComparison.CurrentCultureIgnoreCase));
        }

        [Test]
        public void SetFlatPatternViewTest()
        {
            string view1RefDoc;
            string view1RefConf;
            int view1RefConfType;

            using (var doc = OpenDataDocument(@"FlatPatternDraw1\FlatPatternDraw1.slddrw"))
            {
                var drwDoc = m_App.Documents.Active as ISwDrawing;

                var sheet1 = drwDoc.Sheets["Sheet1"];

                var view1 = sheet1.DrawingViews["Drawing View1"];

                var part1 = (ISwPart)m_App.Documents.PreCreateFromPath(GetFilePath("FlatPattern1.SLDPRT"));

                view1.ReferencedDocument = part1;
                view1.ReferencedConfiguration = part1.Configurations["Default"];

                view1RefDoc = ((ISwDrawingView)view1).DrawingView.ReferencedDocument.GetPathName();
                view1RefConf = ((ISwDrawingView)view1).DrawingView.ReferencedConfiguration;
                view1RefConfType = ((ISwDrawingView)view1).DrawingView.ReferencedDocument.IGetConfigurationByName(view1RefConf).Type;
            }

            Assert.That(string.Equals(view1RefDoc, GetFilePath("FlatPattern1.SLDPRT"), StringComparison.CurrentCultureIgnoreCase));
            Assert.That(string.Equals(view1RefConf, "DefaultSM-FLAT-PATTERN", StringComparison.CurrentCultureIgnoreCase));
            Assert.AreEqual(view1RefConfType, (int)swConfigurationType_e.swConfiguration_SheetMetal);
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
            Type t12;

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
                t12 = sheet.DrawingViews["Drawing View9"].GetType();
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
            Assert.That(typeof(ISwFlatPatternDrawingView).IsAssignableFrom(t12));
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

        [Test]
        public void FlatPatternOptionsTest()
        {
            FlatPatternViewOptions_e opts1;
            FlatPatternViewOptions_e opts2;
            FlatPatternViewOptions_e opts3;

            int view1BendLines;
            int view2BendLines;
            int view3BendLines;

            bool view1BendNotes;
            bool view2BendNotes;
            bool view3BendNotes;

            using (var doc = OpenDataDocument("Drawing6\\Drawing6.slddrw"))
            {
                var drwDoc = m_App.Documents.Active as ISwDrawing;

                var sheet = drwDoc.Sheets["Sheet1"];

                var view1 = (ISwFlatPatternDrawingView)sheet.DrawingViews["Drawing View1"];
                opts1 = view1.Options;
                view1.Options = FlatPatternViewOptions_e.Default;
                view1BendLines = view1.DrawingView.GetBendLineCount();
                view1BendNotes = view1.DrawingView.ShowSheetMetalBendNotes;

                var view2 = (ISwFlatPatternDrawingView)sheet.DrawingViews["Drawing View2"];
                opts2 = view1.Options;
                view2.Options = FlatPatternViewOptions_e.BendLines | FlatPatternViewOptions_e.BendNotes;
                view2BendLines = view2.DrawingView.GetBendLineCount();
                view2BendNotes = view2.DrawingView.ShowSheetMetalBendNotes;

                var view3 = (ISwFlatPatternDrawingView)sheet.DrawingViews["Drawing View3"];
                opts3 = view1.Options;
                view3.Options = FlatPatternViewOptions_e.BendLines;
                view3BendLines = view3.DrawingView.GetBendLineCount();
                view3BendNotes = view3.DrawingView.ShowSheetMetalBendNotes;
            }

            Assert.AreEqual(FlatPatternViewOptions_e.BendLines | FlatPatternViewOptions_e.BendNotes, opts1);
            Assert.AreEqual(0, view1BendLines);
            Assert.AreEqual(false, view1BendNotes);

            Assert.AreEqual(FlatPatternViewOptions_e.Default, opts2);
            Assert.AreEqual(2, view2BendLines);
            Assert.AreEqual(true, view2BendNotes);

            Assert.AreEqual(FlatPatternViewOptions_e.Default, opts3);
            Assert.AreEqual(1, view3BendLines);
            Assert.AreEqual(false, view3BendNotes);
        }

        //NOTE: this test may stuck on the sheet properties dialog opened and needs to be closed manually by the user
        [Test]
        public void SheetCreatedEventTest() 
        {
            var res1 = new List<Tuple<string, string>>();

            string newSheetName;

            using (var doc = OpenDataDocument("Drawing3.slddrw"))
            {
                var drwDoc = m_App.Documents.Active as ISwDrawing;

                var sheetNames = (string[])drwDoc.Drawing.GetSheetNames();

                drwDoc.Sheets.SheetCreated += (d, s) => 
                {
                    res1.Add(new Tuple<string, string>(d.Path, s.Name));
                };

                const int swCommands_Insert_Sheet = 857;

                m_App.Sw.RunCommand(swCommands_Insert_Sheet, "");

                newSheetName = ((string[])drwDoc.Drawing.GetSheetNames()).Except(sheetNames).First();
            }

            Assert.AreEqual(1, res1.Count);
            Assert.AreEqual(GetFilePath("Drawing3.slddrw").ToLower(), res1[0].Item1.ToLower());
            Assert.AreEqual(newSheetName, res1[0].Item2);
        }

        [Test]
        public void DrawingViewCreatedEventTest()
        {
            var res1 = new List<Tuple<string, string, string>>();
            var res2 = new List<Tuple<string, string, string>>();

            string view1;
            string view2;

            using (var doc = OpenDataDocument("Drawing3.slddrw"))
            {
                var drwDoc = m_App.Documents.Active as ISwDrawing;

                var sheet1 = drwDoc.Sheets["Sheet1"];
                var sheet2 = drwDoc.Sheets["Sheet2"];

                sheet1.DrawingViews.ViewCreated += (d, s, v) =>
                {
                    res1.Add(new Tuple<string, string, string>(d.Path, s.Name, v.Name));
                };

                sheet2.DrawingViews.ViewCreated += (d, s, v) =>
                {
                    res2.Add(new Tuple<string, string, string>(d.Path, s.Name, v.Name));
                };

                drwDoc.Drawing.ActivateSheet("Sheet1");
                
                view1 = drwDoc.Drawing.CreateDrawViewFromModelView3(GetFilePath(@"Drawing9\Part1.sldprt"), "*Front", 0, 0, 0).Name;

                drwDoc.Drawing.ActivateSheet("Sheet2");

                view2 = drwDoc.Drawing.CreateDrawViewFromModelView3(GetFilePath("Cylinder1.sldprt"), "*Front", 0, 0, 0).Name;
            }

            Assert.AreEqual(1, res1.Count);
            Assert.AreEqual(GetFilePath("Drawing3.slddrw").ToLower(), res1[0].Item1.ToLower());
            Assert.AreEqual("Sheet1", res1[0].Item2);
            Assert.AreEqual(view1, res1[0].Item3);

            Assert.AreEqual(1, res2.Count);
            Assert.AreEqual(GetFilePath("Drawing3.slddrw").ToLower(), res2[0].Item1.ToLower());
            Assert.AreEqual("Sheet2", res2[0].Item2);
            Assert.AreEqual(view2, res2[0].Item3);
        }

        [Test]
        public void CloneSheetTest()
        {
            string expectedClonedSheetName;
            string clonedSheetName;
            int sheetsCount;

            using (var doc = OpenDataDocument("Drawing3.slddrw"))
            {
                var drwDoc = m_App.Documents.Active as ISwDrawing;
                
                var sheetNames = (string[])drwDoc.Drawing.GetSheetNames();

                var cloned = drwDoc.Sheets.First().Clone(drwDoc);
                clonedSheetName = cloned.Name;

                var newSheetNames = (string[])drwDoc.Drawing.GetSheetNames();

                expectedClonedSheetName = newSheetNames.Except(sheetNames).First();

                sheetsCount = newSheetNames.Length;
            }

            Assert.AreEqual(4, sheetsCount);
            Assert.AreEqual(expectedClonedSheetName, clonedSheetName);
        }
    }
}

