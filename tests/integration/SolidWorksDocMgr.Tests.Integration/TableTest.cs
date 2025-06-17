using Microsoft.SqlServer.Server;
using NUnit.Framework;
using SolidWorks.Interop.swdocumentmgr;
using SolidWorksDocMgr.Tests.Integration;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Exceptions;
using Xarial.XCad.SwDocumentManager.Documents;

namespace SolidWorksDocMgr.Tests.Integration
{
    public class TableTest : IntegrationTests
    {
        [Test]
        public void ReadVisibleTest()
        {
            DataTable data;

            var headerExpected = new string[] { "ITEM NO.", "PART NUMBER", "DESCRIPTION", "INDEX", "QTY." };
            var contentExpected = new string[][]
            {
                new string[] { "1", "Test1", "", "0", "" },
                new string[] { "2", "Part1", "Desc1", "1", "1" },
                new string[] { "3", "", "", "3", "" },
                new string[] { "4", "Part2", "", "4", "1" },
                new string[] { "", "", "", "5", "" },
                new string[] { "6", "", "Desc4", "7", "" }
            };

            string[] header;
            string[][] content;

            using (var doc = OpenDataDocument(@"Assembly18\Draw18.slddrw"))
            {
                var drw = (IXDrawing)doc.Document;
                var table = drw.Sheets.Active.Annotations.OfType<IXTable>().First();

                header = table.Columns.Select(c => c.Title).ToArray();
                content = table.Rows.Select(r => r.Cells.Select(c => c.Value).ToArray()).ToArray();

                data = table.Read(true);
            }

            Assert.AreEqual(5, data.Columns.Count);
            Assert.AreEqual(6, data.Rows.Count);
            CollectionAssert.AreEqual(headerExpected, data.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray());
            CollectionAssert.AreEqual(contentExpected[0], data.Rows[0].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(contentExpected[1], data.Rows[1].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(contentExpected[2], data.Rows[2].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(contentExpected[3], data.Rows[3].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(contentExpected[4], data.Rows[4].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(contentExpected[5], data.Rows[5].ItemArray.Select(i => i?.ToString()));

            Assert.AreEqual(5, header.Length);
            Assert.AreEqual(6, content.Length);
            CollectionAssert.AreEqual(headerExpected, data.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray());
            CollectionAssert.AreEqual(contentExpected[0], content[0]);
            CollectionAssert.AreEqual(contentExpected[1], content[1]);
            CollectionAssert.AreEqual(contentExpected[2], content[2]);
            CollectionAssert.AreEqual(contentExpected[3], content[3]);
            CollectionAssert.AreEqual(contentExpected[4], content[4]);
            CollectionAssert.AreEqual(contentExpected[5], content[5]);
        }

        [Test]
        public void BomGetItemNumberTest()
        {
            string i1;
            string i2;
            string i3;
            string i4;

            using (var doc = OpenDataDocument(@"Assembly18\Draw18.slddrw"))
            {
                var drw = (IXDrawing)doc.Document;
                var bomTable = drw.Sheets.Active.Annotations.OfType<IXBomTable>().First();

                var rowsRepo = bomTable.Rows;

                i1 = rowsRepo[0].ItemNumber;
                i2 = rowsRepo[2].ItemNumber;
                i3 = rowsRepo[4].ItemNumber;
                i4 = rowsRepo[5].ItemNumber;
            }

            Assert.AreEqual("1", i1);
            Assert.AreEqual("3", i2);
            Assert.AreEqual(BomItemNumber.None, i3);
            Assert.AreEqual("6", i4);
        }

        [Test]
        public void BomComponentsTest()
        {
            string refDocPath;
            string refConf;
            bool refDocComm;
            string[] c0;
            string[] c1;
            string[] c2;
            string[] c3;
            string[] c4;
            string[] c5;

            string workDir;

            using (var doc = OpenDataDocument(@"Assembly18\Draw18.slddrw"))
            {
                workDir = doc.WorkFolderPath;

                var drw = (IXDrawing)doc.Document;
                var bomTable = drw.Sheets.Active.Annotations.OfType<IXBomTable>().First();

                var refDoc = bomTable.ReferencedDocument;

                if (!refDoc.IsCommitted)
                {
                    refDoc.Commit();
                }

                refDocComm = refDoc.IsCommitted;

                refDocPath = refDoc.Path;
                refConf = bomTable.ReferencedConfiguration?.Name;

                var rowsRepo = bomTable.Rows;

                c0 = rowsRepo[0].Components?.Select(c => c.Name).ToArray();
                c1 = rowsRepo[1].Components?.Select(c => c.Name).ToArray();
                c2 = rowsRepo[2].Components?.Select(c => c.Name).ToArray();
                c3 = rowsRepo[3].Components?.Select(c => c.Name).ToArray();
                c4 = rowsRepo[4].Components?.Select(c => c.Name).ToArray();
                c5 = rowsRepo[5].Components?.Select(c => c.Name).ToArray();
            }

            Assert.That(string.Equals(refDocPath, Path.Combine(workDir, "Assembly18\\Assem18.sldasm"), StringComparison.CurrentCultureIgnoreCase));
            Assert.That(string.Equals(refConf, "Default", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(refDocComm);
            Assert.IsNull(c0);
            CollectionAssert.AreEqual(new string[] { "Part1-1" }, c1);
            Assert.IsNull(c2);
            CollectionAssert.AreEqual(new string[] { "Part2-1" }, c3);
            Assert.IsNull(c4);
            Assert.IsNull(c5);
        }

        [Test]
        public void BomUnknownComponentsTest()
        {
            string refDocPath1;
            string refConf1;
            bool refDocComm1;

            Tuple<string, Tuple<string, string, string, bool>[]>[] res1;

            string refDocPath2;
            string refConf2;
            bool refDocComm2;

            Tuple<string, Tuple<string, string, string, bool>[]>[] res2;

            string c1;
            string cp1;

            string workDir;

            using (var doc = OpenDataDocument(@"Drawing10\Drawing10.slddrw"))
            {
                workDir = doc.WorkFolderPath;

                var drw = (IXDrawing)doc.Document;

                var bomTable1 = drw.Sheets["Sheet1"].Annotations.OfType<IXBomTable>().First();

                var refDoc1 = bomTable1.ReferencedDocument;

                refDocComm1 = refDoc1.IsCommitted;

                refDocPath1 = refDoc1.Path;
                refConf1 = bomTable1.ReferencedConfiguration.Name;

                res1 = bomTable1.Rows.Select(r => new Tuple<string, Tuple<string, string, string, bool>[]>(
                    r.ItemNumber,
                    r.Components.Select(c => new Tuple<string, string, string, bool>(c.Name, c.ReferencedConfiguration.Name, c.Parent?.Name, c is IXUnknownComponent)).ToArray())).ToArray();

                var bomTable2 = drw.Sheets["Sheet2"].Annotations.OfType<IXBomTable>().First();

                var refDoc2 = bomTable2.ReferencedDocument;

                refDocComm2 = refDoc2.IsCommitted;

                refDocPath2 = refDoc2.Path;
                refConf2 = bomTable2.ReferencedConfiguration.Name;

                res2 = bomTable2.Rows.Select(r => new Tuple<string, Tuple<string, string, string, bool>[]>(
                    r.ItemNumber,
                    r.Components.Select(c => new Tuple<string, string, string, bool>(c.Name, c.ReferencedConfiguration.Name, c.Parent?.Name, c is IXUnknownComponent)).ToArray())).ToArray();

                var comp = (IXUnknownComponent)bomTable1.Rows.ElementAt(1).Components.First();
                var knownComp = (ISwDmComponent)comp.GetKnown();
                c1 = ((ISwDMComponent7)knownComp.Component).Name2;
                cp1 = ((ISwDMComponent7)((ISwDmComponent)knownComp.Parent).Component).Name2;
            }

            Assert.That(string.Equals(refDocPath1, Path.Combine(workDir, "Drawing10\\Assem1.sldasm"), StringComparison.CurrentCultureIgnoreCase));
            Assert.That(string.Equals(refConf1, "Default", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsFalse(refDocComm1);
            Assert.That(string.Equals(refDocPath2, Path.Combine(workDir, "Drawing10\\Assem1.sldasm"), StringComparison.CurrentCultureIgnoreCase));
            Assert.That(string.Equals(refConf2, "Default", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsFalse(refDocComm2);
            Assert.AreEqual(5, res1.Length);
            Assert.AreEqual("1", res1[0].Item1);
            Assert.AreEqual("1.1", res1[1].Item1);
            Assert.AreEqual("2", res1[2].Item1);
            Assert.AreEqual("3", res1[3].Item1);
            Assert.AreEqual("3.1", res1[4].Item1);
            Assert.AreEqual(1, res1[0].Item2.Length);
            Assert.AreEqual("SubAssem1-1", res1[0].Item2[0].Item1);
            Assert.AreEqual("Default", res1[0].Item2[0].Item2);
            Assert.IsNull(res1[0].Item2[0].Item3);
            Assert.IsTrue(res1[0].Item2[0].Item4);
            Assert.AreEqual(1, res1[1].Item2.Length);
            Assert.AreEqual("Part1-1", res1[1].Item2[0].Item1);
            Assert.AreEqual("Default", res1[1].Item2[0].Item2);
            Assert.AreEqual("SubAssem1-1", res1[1].Item2[0].Item3);
            Assert.IsTrue(res1[1].Item2[0].Item4);
            Assert.AreEqual(1, res1[2].Item2.Length);
            Assert.AreEqual("Part1-1", res1[2].Item2[0].Item1);
            Assert.AreEqual("Conf1", res1[2].Item2[0].Item2);
            Assert.IsNull(res1[2].Item2[0].Item3);
            Assert.IsTrue(res1[2].Item2[0].Item4);
            Assert.AreEqual(1, res1[3].Item2.Length);
            Assert.AreEqual("SubAssem1-2", res1[3].Item2[0].Item1);
            Assert.AreEqual("Conf1", res1[3].Item2[0].Item2);
            Assert.IsNull(res1[3].Item2[0].Item3);
            Assert.IsTrue(res1[3].Item2[0].Item4);
            Assert.AreEqual(1, res1[4].Item2.Length);
            Assert.AreEqual("Part1-1", res1[4].Item2[0].Item1);
            Assert.AreEqual("Conf1", res1[4].Item2[0].Item2);
            Assert.AreEqual("SubAssem1-2", res1[4].Item2[0].Item3);
            Assert.IsTrue(res1[4].Item2[0].Item4);

            Assert.AreEqual(3, res2.Length);
            Assert.AreEqual("1", res2[0].Item1);
            Assert.AreEqual("2", res2[1].Item1);
            Assert.AreEqual("3", res2[2].Item1);
            Assert.AreEqual(1, res2[0].Item2.Length);
            Assert.AreEqual("SubAssem1-1", res2[0].Item2[0].Item1);
            Assert.AreEqual("Default", res2[0].Item2[0].Item2);
            Assert.IsNull(res2[0].Item2[0].Item3);
            Assert.IsTrue(res2[0].Item2[0].Item4);
            Assert.AreEqual(1, res2[1].Item2.Length);
            Assert.AreEqual("SubAssem1-2", res2[1].Item2[0].Item1);
            Assert.AreEqual("Conf1", res2[1].Item2[0].Item2);
            Assert.IsNull(res2[1].Item2[0].Item3);
            Assert.IsTrue(res2[1].Item2[0].Item4);
            Assert.AreEqual(1, res2[2].Item2.Length);
            Assert.AreEqual("Part1-1", res2[2].Item2[0].Item1);
            Assert.AreEqual("Conf1", res2[2].Item2[0].Item2);
            Assert.IsNull(res2[2].Item2[0].Item3);
            Assert.IsTrue(res2[2].Item2[0].Item4);

            Assert.AreEqual("Part1-1", c1);
            Assert.AreEqual("SubAssem1-1", cp1);
        }

        [Test]
        public void BomUnknownComponentsDetachedTest()
        {
            string refDocFileName1;
            string refConf1;
            bool refDocComm1;

            Tuple<string, Tuple<string, string, string, bool>[]>[] res1;

            string workDir;

            using (var doc = OpenDataDocument(@"Drawing10-detached.SLDDRW"))
            {
                workDir = doc.WorkFolderPath;

                var drw = (IXDrawing)doc.Document;

                var bomTable1 = drw.Sheets["Sheet1"].Annotations.OfType<IXBomTable>().First();

                var refDoc1 = bomTable1.ReferencedDocument;

                refDocComm1 = refDoc1.IsCommitted;

                refDocFileName1 = Path.GetFileName(refDoc1.Path);
                refConf1 = bomTable1.ReferencedConfiguration.Name;

                res1 = bomTable1.Rows.Select(r => new Tuple<string, Tuple<string, string, string, bool>[]>(
                    r.ItemNumber,
                    r.Components.Select(c => new Tuple<string, string, string, bool>(c.Name, c.ReferencedConfiguration.Name, c.Parent?.Name, c is IXUnknownComponent)).ToArray())).ToArray();
            }

            Assert.That(string.Equals(refDocFileName1, "Assem1.sldasm", StringComparison.CurrentCultureIgnoreCase));
            Assert.That(string.Equals(refConf1, "Default", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsFalse(refDocComm1);

            Assert.AreEqual(1, res1[0].Item2.Length);
            Assert.AreEqual("SubAssem1-1", res1[0].Item2[0].Item1);
            Assert.AreEqual("Default", res1[0].Item2[0].Item2);
            Assert.IsNull(res1[0].Item2[0].Item3);
            Assert.IsTrue(res1[0].Item2[0].Item4);
            Assert.AreEqual(1, res1[1].Item2.Length);
            Assert.AreEqual("Part1-1", res1[1].Item2[0].Item1);
            Assert.AreEqual("Default", res1[1].Item2[0].Item2);
            Assert.AreEqual("SubAssem1-1", res1[1].Item2[0].Item3);
            Assert.IsTrue(res1[1].Item2[0].Item4);
            Assert.AreEqual(1, res1[2].Item2.Length);
            Assert.AreEqual("Part1-1", res1[2].Item2[0].Item1);
            Assert.AreEqual("Conf1", res1[2].Item2[0].Item2);
            Assert.IsNull(res1[2].Item2[0].Item3);
            Assert.IsTrue(res1[2].Item2[0].Item4);
            Assert.AreEqual(1, res1[3].Item2.Length);
            Assert.AreEqual("SubAssem1-2", res1[3].Item2[0].Item1);
            Assert.AreEqual("Conf1", res1[3].Item2[0].Item2);
            Assert.IsNull(res1[3].Item2[0].Item3);
            Assert.IsTrue(res1[3].Item2[0].Item4);
            Assert.AreEqual(1, res1[4].Item2.Length);
            Assert.AreEqual("Part1-1", res1[4].Item2[0].Item1);
            Assert.AreEqual("Conf1", res1[4].Item2[0].Item2);
            Assert.AreEqual("SubAssem1-2", res1[4].Item2[0].Item3);
            Assert.IsTrue(res1[4].Item2[0].Item4);
        }
    }
}
