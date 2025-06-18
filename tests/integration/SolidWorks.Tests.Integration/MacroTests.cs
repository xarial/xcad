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

            using (var dataFile = GetDataFile("VbaMacro.swp"))
            {
                using (var doc = NewDataDocument(Interop.swconst.swDocumentTypes_e.swDocPART))
                {
                    var macro = Application.OpenMacro(dataFile.FilePath);
                    macro.Run();
                    Application.Sw.IActiveDoc2.Extension.CustomPropertyManager[""].Get5("Field1", false, out val, out _, out _);
                }
            }

            Assert.AreEqual("main", val);
        }

        [Test]
        public void RunVbaMacroCustomEntryPoint()
        {
            string val;

            using (var dataFile = GetDataFile("VbaMacro.swp"))
            {
                using (var doc = NewDataDocument(Interop.swconst.swDocumentTypes_e.swDocPART))
                {
                    var macro = (ISwVbaMacro)Application.OpenMacro(dataFile.FilePath);
                    var proc = macro.EntryPoints.First(e => e.ProcedureName == "Func1");
                    macro.Run(proc);
                    Application.Sw.IActiveDoc2.Extension.CustomPropertyManager[""].Get5("Field1", false, out val, out _, out _);
                }
            }

            Assert.AreEqual("Func1", val);
        }

        [Test]
        public void VbaMacroEntryPoints()
        {
            string[] entryPoints;

            using (var dataFile = GetDataFile("VbaMacro.swp"))
            {
                var macro = (ISwVbaMacro)Application.OpenMacro(dataFile.FilePath);
                entryPoints = macro.EntryPoints.Select(e => $"{e.ModuleName}.{e.ProcedureName}").ToArray();
            }

            Assert.That(entryPoints.SequenceEqual(new string[] { "VbaMacro1.main", "VbaMacro1.Func1", "VbaMacro1.Func3", "Module1.Func4" }));
        }

        [Test]
        public void RunVsta1Macro() 
        {
            using (var dataFile = GetDataFile(@"VstaMacro\Vsta1Macro\SwMacro\bin\Debug\Vsta1Macro.dll"))
            {
                using (var doc = NewDataDocument(Interop.swconst.swDocumentTypes_e.swDocPART))
                {
                    var macro = (ISwVstaMacro)Application.OpenMacro(dataFile.FilePath);
                    macro.Version = VstaMacroVersion_e.Vsta1;

                    var proc = macro.EntryPoints.First();

                    if (Application.Version.Major < Xarial.XCad.SolidWorks.Enums.SwVersion_e.Sw2021)
                    {
                        macro.Run(proc, Xarial.XCad.Enums.MacroRunOptions_e.UnloadAfterRun);
                        Application.Sw.IActiveDoc2.Extension.CustomPropertyManager[""].Get5("Field1", false, out string val, out _, out _);
                        Assert.AreEqual("VstaMacroText", val);
                    }
                    else
                    {
                        Assert.Throws<NotSupportedException>(() => macro.Run(proc, Xarial.XCad.Enums.MacroRunOptions_e.UnloadAfterRun));
                    }
                }
            }
        }
    }
}
