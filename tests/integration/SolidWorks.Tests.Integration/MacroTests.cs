using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad;
using Xarial.XCad.SolidWorks;

namespace SolidWorks.Tests.Integration
{
    public class MacroTests : IntegrationTests
    {
        [Test]
        public void RunVbaMacro() 
        {
            string val;

            using (var doc = NewDocument(Interop.swconst.swDocumentTypes_e.swDocPART))
            {
                var macro = m_App.OpenMacro(GetFilePath("VbaMacro.swp"));
                macro.Run();
                m_App.Sw.IActiveDoc2.Extension.CustomPropertyManager[""].Get5("Field1", false, out val, out _, out _);
            }

            Assert.AreEqual("main", val);
        }

        [Test]
        public void RunVbaMacroCustomEntryPoint()
        {
            string val;

            using (var doc = NewDocument(Interop.swconst.swDocumentTypes_e.swDocPART))
            {
                var macro = (SwVbaMacro)m_App.OpenMacro(GetFilePath("VbaMacro.swp"));
                var proc = macro.EntryPoints.First(e => e.ProcedureName == "Func1");
                macro.Run(proc);
                m_App.Sw.IActiveDoc2.Extension.CustomPropertyManager[""].Get5("Field1", false, out val, out _, out _);
            }

            Assert.AreEqual("Func1", val);
        }

        [Test]
        public void VbaMacroEntryPoints()
        {
            var macro = (SwVbaMacro)m_App.OpenMacro(GetFilePath("VbaMacro.swp"));
            var entryPoints = macro.EntryPoints.Select(e => $"{e.ModuleName}.{e.ProcedureName}");

            Assert.That(entryPoints.SequenceEqual(new string[] { "VbaMacro1.main", "VbaMacro1.Func1", "VbaMacro1.Func3", "Module1.Func4" }));
        }

        [Test]
        public void RunVsta1Macro()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void RunVsta3Macro()
        {
            throw new NotImplementedException();
        }
    }
}
