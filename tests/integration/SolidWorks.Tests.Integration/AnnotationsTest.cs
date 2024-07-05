using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Enums;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks.Annotations;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.Toolkit;
using Xarial.XCad.Toolkit.Utils;

namespace SolidWorks.Tests.Integration
{
    public class AnnotationsTest : IntegrationTests 
    {
        [Test]
        public void CreateNoteTest() 
        {
            string font;
            int heightInPts;
            bool isPts;
            bool isBold;
            bool isUnderline;
            bool isItalic;
            bool isStrikeout;
            double angle;
            double[] pos;
            System.Drawing.Color color;

            using (var doc = NewDocument(Interop.swconst.swDocumentTypes_e.swDocDRAWING)) 
            {
                var drw = m_App.Documents.Active;

                var note = drw.Annotations.PreCreate<ISwNote>();
                note.Text = "Test Note";
                note.Color = System.Drawing.Color.Red;
                note.Font = new Font("Arial", null, 25, FontStyle_e.Bold | FontStyle_e.Underline |FontStyle_e.Strikeout | FontStyle_e.Italic);
                note.Angle = Math.PI / 2;
                note.Position = new Point(0.1, 0.2, 0);

                note.Commit();

                font = note.Note.IGetTextFormat().TypeFaceName;
                heightInPts = note.Note.IGetTextFormat().CharHeightInPts;
                isPts = note.Note.IGetTextFormat().IsHeightSpecifiedInPts();
                isBold = note.Note.IGetTextFormat().Bold;
                isItalic = note.Note.IGetTextFormat().Italic;
                isUnderline = note.Note.IGetTextFormat().Underline;
                isStrikeout = note.Note.IGetTextFormat().Strikeout;
                angle = note.Note.Angle;
                pos = (double[])note.Note.IGetAnnotation().GetPosition();
                color = ColorUtils.FromColorRef(note.Annotation.Color);
            }

            Assert.AreEqual(System.Drawing.Color.Red.R, color.R);
            Assert.AreEqual(System.Drawing.Color.Red.G, color.G);
            Assert.AreEqual(System.Drawing.Color.Red.B, color.B);
            Assert.AreEqual("Arial", font);
            Assert.AreEqual(25, heightInPts);
            Assert.IsTrue(isPts);
            Assert.IsTrue(isBold);
            Assert.IsTrue(isItalic);
            Assert.IsTrue(isStrikeout);
            Assert.That(Math.PI / 2, Is.EqualTo(angle).Within(0.001).Percent);
            Assert.That(Math.PI / 2, Is.EqualTo(angle).Within(0.001).Percent);
            Assert.That(0.1, Is.EqualTo(pos[0]).Within(0.001).Percent);
            Assert.That(0.2, Is.EqualTo(pos[1]).Within(0.001).Percent);
        }

        [Test]
        public void OwnerTest()
        {
            Type t1;
            Type t2;
            Type t3;
            Type t4;

            string v1;
            string s1;
            string s2;

            int o1;
            int o2;
            string p1;
            string p2;

            using (var doc = OpenDataDocument(@"Drawing8\Drawing8.SLDDRW"))
            {
                var drw = (ISwDrawing)m_App.Documents.Active;

                var notes = drw.Annotations.Filter<IXDrawingNote>().ToArray();

                var note1 = notes.First(n => string.Equals(n.Text, "Note1", StringComparison.CurrentCultureIgnoreCase));
                var note2 = notes.First(n => string.Equals(n.Text, "Note2XYZ", StringComparison.CurrentCultureIgnoreCase));
                var note3 = notes.First(n => string.Equals(n.Text, "Note3", StringComparison.CurrentCultureIgnoreCase));
                var note4 = notes.First(n => string.Equals(n.Text, "Note4", StringComparison.CurrentCultureIgnoreCase));

                t1 = note1.Owner.GetType();
                s1 = ((IXSheet)note1.Owner).Name;
                t2 = note2.Owner.GetType();
                v1 = ((IXDrawingView)note2.Owner).Name;
                t3 = note3.Owner.GetType();
                s2 = ((IXSheet)note3.Owner).Name;
                t4 = note4.Owner.GetType();

                note3.Owner = drw.Sheets.Active.DrawingViews.First();
                note2.Owner = drw.Sheets.Active;

                drw.Rebuild();

                o1 = ((ISwNote)note3).Annotation.OwnerType;
                p1 = note3.Text;
                o2 = ((ISwNote)note2).Annotation.OwnerType;
                p2 = note2.Text;
            }

            Assert.IsTrue(typeof(IXSheet).IsAssignableFrom(t1));
            Assert.AreEqual(s1, "Sheet1");
            Assert.IsTrue(typeof(IXDrawingView).IsAssignableFrom(t2));
            Assert.AreEqual(v1, "Drawing View1");
            Assert.IsTrue(typeof(IXSheet).IsAssignableFrom(t3));
            Assert.AreEqual(s2, "Sheet1");
            Assert.IsTrue(typeof(IXSheetFormat).IsAssignableFrom(t4));
            //Assert.AreEqual(0, o1);
            Assert.AreEqual(p1, "Note3ABC");
            //Assert.AreEqual(1, o2);
            Assert.AreEqual(p2, "Note2");
        }
    }
}
