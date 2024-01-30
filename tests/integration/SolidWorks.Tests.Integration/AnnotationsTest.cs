using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Base;
using Xarial.XCad.Enums;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks.Annotations;
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
    }
}
