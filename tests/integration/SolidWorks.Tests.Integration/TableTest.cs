using Microsoft.SqlServer.Server;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Exceptions;
using Xarial.XCad.SolidWorks.Annotations;

namespace SolidWorks.Tests.Integration
{
    public class TableTest : IntegrationTests
    {
        [Test]
        public void ReadAllTest()
        {
            DataTable data;

            using (var doc = OpenDataDocument(@"Assembly18\Draw18.slddrw"))
            {
                var drw = (IXDrawing)m_App.Documents.Active;
                var table = drw.Sheets.Active.Annotations.OfType<IXTable>().First();

                data = table.Read(false);
            }

            Assert.AreEqual(6, data.Columns.Count);
            Assert.AreEqual(8, data.Rows.Count);
            CollectionAssert.AreEqual(new string[] { "ITEM NO.", "PART NUMBER", "SW-Title(Title)", "DESCRIPTION", "INDEX", "QTY." }, data.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray());
            CollectionAssert.AreEqual(new string[] { "1", "Test1", "Test2", "", "0", "" }, data.Rows[0].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "2", "Part1", "", "Desc1", "1", "1" }, data.Rows[1].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "", "", "", "Desc2", "2", "" }, data.Rows[2].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "3", "", "", "", "3", "" }, data.Rows[3].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "4", "Part2", "", "", "4", "1" }, data.Rows[4].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "", "", "", "", "5", "" }, data.Rows[5].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "5", "", "", "Desc3", "6", "" }, data.Rows[6].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "6", "", "", "Desc4", "7", "" }, data.Rows[7].ItemArray.Select(i => i?.ToString()));
        }

        [Test]
        public void ReadVisibleTest()
        {
            DataTable data;

            using (var doc = OpenDataDocument(@"Assembly18\Draw18.slddrw"))
            {
                var drw = (IXDrawing)m_App.Documents.Active;
                var table = drw.Sheets.Active.Annotations.OfType<IXTable>().First();

                data = table.Read(true);
            }

            Assert.AreEqual(5, data.Columns.Count);
            Assert.AreEqual(6, data.Rows.Count);
            CollectionAssert.AreEqual(new string[] { "ITEM NO.", "PART NUMBER", "DESCRIPTION", "INDEX", "QTY." }, data.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray());
            CollectionAssert.AreEqual(new string[] { "1", "Test1", "", "0", "" }, data.Rows[0].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "2", "Part1", "Desc1", "1", "1" }, data.Rows[1].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "3", "", "", "3", "" }, data.Rows[2].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "4", "Part2", "", "4", "1" }, data.Rows[3].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "", "", "", "5", "" }, data.Rows[4].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "6", "", "Desc4", "7", "" }, data.Rows[5].ItemArray.Select(i => i?.ToString()));
        }

        [Test]
        public void MoveRowsTest() 
        {
            DataTable data;

            int[] indices;
            bool[] v1;

            using (var doc = OpenDataDocument(@"Assembly18\Draw18.slddrw"))
            {
                var drw = (IXDrawing)m_App.Documents.Active;
                var table = drw.Sheets.Active.Annotations.OfType<IXTable>().First();

                var rowsRepo = table.Rows;

                var rows = rowsRepo.ToArray();

                rows[4].Index = 0;
                rows[1].Index = 7;
                rows[3].Index = 1;
                rows[2].Index = 5;

                indices = rows.Select(r => r.Index).ToArray();

                data = table.Read(false);

                v1 = rowsRepo.Select(r => r.Visible).ToArray();
            }

            CollectionAssert.AreEqual(new int[] { 2, 7, 5, 1, 0, 3, 4, 6 }, indices);

            CollectionAssert.AreEqual(new bool[] { true, true, true, true, false, false, true, true }, v1);

            Assert.AreEqual(6, data.Columns.Count);
            Assert.AreEqual(8, data.Rows.Count);
            CollectionAssert.AreEqual(new string[] { "ITEM NO.", "PART NUMBER", "SW-Title(Title)", "DESCRIPTION", "INDEX", "QTY." }, data.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray());
            CollectionAssert.AreEqual(new string[] { "1", "Part2", "", "", "4", "1" }, data.Rows[0].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "2", "", "", "", "3", "" }, data.Rows[1].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "3", "Test1", "Test2", "", "0", "" }, data.Rows[2].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "", "", "", "", "5", "" }, data.Rows[3].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "4", "", "", "Desc3", "6", "" }, data.Rows[4].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "", "", "", "Desc2", "2", "" }, data.Rows[5].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "5", "", "", "Desc4", "7", "" }, data.Rows[6].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "6", "Part1", "", "Desc1", "1", "1" }, data.Rows[7].ItemArray.Select(i => i?.ToString()));
        }

        [Test]
        public void BomGetItemNumberTest() 
        {
            int? i1;
            int? i2;
            int? i3;
            int? i4;

            using (var doc = OpenDataDocument(@"Assembly18\Draw18.slddrw"))
            {
                var drw = (IXDrawing)m_App.Documents.Active;
                var bomTable = drw.Sheets.Active.Annotations.OfType<IXBomTable>().First();

                var rowsRepo = bomTable.Rows;

                i1 = rowsRepo[0].ItemNumber;
                i2 = rowsRepo[2].ItemNumber;
                i3 = rowsRepo[3].ItemNumber;
                i4 = rowsRepo[6].ItemNumber;
            }

            Assert.AreEqual(1, i1);
            Assert.That(!i2.HasValue);
            Assert.AreEqual(3, i3);
            Assert.AreEqual(5, i4);
        }

        [Test]
        public void BomSetItemNumberTest()
        {
            string i1;
            string i3;
            string i4;
            string i6;

            bool[] v1;

            using (var doc = OpenDataDocument(@"Assembly18\Draw18.slddrw"))
            {
                var drw = (IXDrawing)m_App.Documents.Active;
                var bomTable = drw.Sheets.Active.Annotations.OfType<IXBomTable>().First();

                var rowsRepo = bomTable.Rows;

                rowsRepo[0].ItemNumber = BomItemNumber.None;
                rowsRepo[2].ItemNumber = BomItemNumber.Auto;
                rowsRepo[3].ItemNumber = BomItemNumber.None;
                rowsRepo[5].ItemNumber = BomItemNumber.Auto;
                Assert.Throws<TableElementOperationException>(() => rowsRepo[6].ItemNumber = 5);

                i1 = ((ISwTable)bomTable).TableAnnotation.Text2[1, 0, true];
                i3 = ((ISwTable)bomTable).TableAnnotation.Text2[3, 0, true];
                i4 = ((ISwTable)bomTable).TableAnnotation.Text2[4, 0, true];
                i6 = ((ISwTable)bomTable).TableAnnotation.Text2[6, 0, true];

                v1 = rowsRepo.Select(r => r.Visible).ToArray();
            }

            CollectionAssert.AreEqual(new bool[] { true, true, false, true, true, true, false, true }, v1);

            Assert.AreEqual("", i1);
            Assert.AreEqual("2", i3);
            Assert.AreEqual("", i4);
            Assert.AreEqual("4", i6);
        }

        [Test]
        public void AddRowTest()
        {
            int[] indices;
            bool[] v1;

            DataTable data;

            using (var doc = OpenDataDocument(@"Assembly18\Draw18.slddrw"))
            {
                var drw = (IXDrawing)m_App.Documents.Active;
                var table = drw.Sheets.Active.Annotations.OfType<IXTable>().First();

                var rowsRepo = table.Rows;

                var rows = rowsRepo.ToArray();

                var r1 = rowsRepo.Insert(0);
                r1.Cells[1].Value = "A1";
                r1.Cells[3].Value = "A3";

                var r2 = rowsRepo.Insert(9);
                r2.Cells[1].Value = "B1";
                r2.Cells[3].Value = "B3";

                var r3 = rowsRepo.Insert(5);
                r3.Cells[1].Value = "C1";
                r3.Cells[3].Value = "C3";

                var r4 = rowsRepo.Insert(3);
                r4.Cells[1].Value = "D1";
                r4.Cells[3].Value = "D3";

                indices = rows.Select(r => r.Index).ToArray();

                data = table.Read(false);

                v1 = rowsRepo.Select(r => r.Visible).ToArray();
            }

            CollectionAssert.AreEqual(new int[] { 1, 2, 4, 5, 7, 8, 9, 10 }, indices);

            CollectionAssert.AreEqual(new bool[] { true, true, true, true, false, true, true, true, true, false, true, true }, v1);

            Assert.AreEqual(6, data.Columns.Count);
            Assert.AreEqual(12, data.Rows.Count);

            CollectionAssert.AreEqual(new string[] { "ITEM NO.", "PART NUMBER", "SW-Title(Title)", "DESCRIPTION", "INDEX", "QTY." }, data.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray());
            CollectionAssert.AreEqual(new string[] { "1", "A1", "", "A3", "", "" }, data.Rows[0].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "2", "Test1", "Test2", "", "0", "" }, data.Rows[1].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "3", "Part1", "", "Desc1", "1", "1" }, data.Rows[2].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "4", "D1", "", "D3", "", "" }, data.Rows[3].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "", "", "", "Desc2", "2", "" }, data.Rows[4].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "5", "", "", "", "3", "" }, data.Rows[5].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "6", "C1", "", "C3", "", "" }, data.Rows[6].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "7", "Part2", "", "", "4", "1" }, data.Rows[7].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "", "", "", "", "5", "" }, data.Rows[8].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "8", "", "", "Desc3", "6", "" }, data.Rows[9].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "9", "", "", "Desc4", "7", "" }, data.Rows[10].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "10", "B1", "", "B3", "", "" }, data.Rows[11].ItemArray.Select(i => i?.ToString()));
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
            string[] c6;
            string[] c7;

            using (var doc = OpenDataDocument(@"Assembly18\Draw18.slddrw"))
            {
                var drw = (IXDrawing)m_App.Documents.Active;
                var bomTable = drw.Sheets.Active.Annotations.OfType<IXBomTable>().First();

                var refDoc = bomTable.ReferencedDocument;

                refDocPath = refDoc.Path;
                refDocComm = refDoc.IsCommitted;
                refConf = bomTable.ReferencedConfiguration?.Name;

                var rowsRepo = bomTable.Rows;

                c0 = rowsRepo[0].Components?.Select(c => c.Name).ToArray();
                c1 = rowsRepo[1].Components?.Select(c => c.Name).ToArray();
                c2 = rowsRepo[2].Components?.Select(c => c.Name).ToArray();
                c3 = rowsRepo[3].Components?.Select(c => c.Name).ToArray();
                c4 = rowsRepo[4].Components?.Select(c => c.Name).ToArray();
                c5 = rowsRepo[5].Components?.Select(c => c.Name).ToArray();
                c6 = rowsRepo[6].Components?.Select(c => c.Name).ToArray();
                c7 = rowsRepo[7].Components?.Select(c => c.Name).ToArray();
            }

            Assert.That(string.Equals(refDocPath, GetFilePath("Assembly18\\Assem18.sldasm"), StringComparison.CurrentCultureIgnoreCase));
            Assert.That(string.Equals(refConf, "Default", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(refDocComm);
            Assert.IsNull(c0);
            CollectionAssert.AreEqual(new string[] { "Part1-1" }, c1);
            Assert.IsNull(c2);
            Assert.IsNull(c3);
            CollectionAssert.AreEqual(new string[] { "Part2-1" }, c4);
            Assert.IsNull(c5);
            Assert.IsNull(c6);
            Assert.IsNull(c7);
        }

        [Test]
        public void DeleteRowsTest()
        {
            DataTable data;
            DataTable data1;

            bool[] v1;
            bool[] v2;
            bool[] v3;
            int i1;
            int i2;
            int i3;
            int i4;
            int i7;

            using (var doc = OpenDataDocument(@"Assembly18\Draw18.slddrw"))
            {
                var drw = (IXDrawing)m_App.Documents.Active;
                var table = drw.Sheets.Active.Annotations.OfType<IXTable>().First();
                
                var rowsRepo = table.Rows;

                var rows = rowsRepo.ToArray();

                v1 = rows.Select(r => r.Visible).ToArray();

                rowsRepo.RemoveRange(new IXTableRow[] { rows[0], rows[5], rows[6] });

                Assert.Throws<TableElementOperationException>(() => rowsRepo.RemoveRange(new IXTableRow[] { rows[1] }));

                Assert.Throws<TableElementDeletedException>(() => { var x = rows[0].Index; });
                i1 = rows[1].Index;
                i2 = rows[2].Index;
                i3 = rows[3].Index;
                i4 = rows[4].Index;
                Assert.Throws<TableElementDeletedException>(() => { var x = rows[5].Index; });
                Assert.Throws<TableElementDeletedException>(() => { var x = rows[6].Index; });
                i7 = rows[7].Index;

                v2 = rows.Except(new IXTableRow[] { rows[0], rows[5], rows[6] }).Select(r => r.Visible).ToArray();
                v3 = rowsRepo.Select(r => r.Visible).ToArray();

                data = table.Read(false);
                data1 = table.Read(true);
            }

            Assert.AreEqual(0, i1);
            Assert.AreEqual(1, i2);
            Assert.AreEqual(2, i3);
            Assert.AreEqual(3, i4);
            Assert.AreEqual(4, i7);

            Assert.AreEqual(6, data.Columns.Count);
            Assert.AreEqual(5, data.Rows.Count);

            Assert.AreEqual(5, data1.Columns.Count);
            Assert.AreEqual(4, data1.Rows.Count);

            CollectionAssert.AreEqual(new string[] { "1", "Part1", "", "Desc1", "1", "1" }, data.Rows[0].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "", "", "", "Desc2", "2", "" }, data.Rows[1].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "2", "", "", "", "3", "" }, data.Rows[2].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "3", "Part2", "", "", "4", "1" }, data.Rows[3].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "4", "", "", "Desc4", "7", "" }, data.Rows[4].ItemArray.Select(i => i?.ToString()));

            CollectionAssert.AreEqual(new string[] { "1", "Part1", "Desc1", "1", "1" }, data1.Rows[0].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "2", "", "", "3", "" }, data1.Rows[1].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "3", "Part2", "", "4", "1" }, data1.Rows[2].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "4", "", "Desc4", "7", "" }, data1.Rows[3].ItemArray.Select(i => i?.ToString()));

            CollectionAssert.AreEqual(new bool[] { true, true, false, true, true, true, false, true }, v1);
            CollectionAssert.AreEqual(new bool[] { true, false, true, true, true }, v2);
            CollectionAssert.AreEqual(new bool[] { true, false, true, true, true }, v3);
        }

        [Test]
        public void VisibleRowsTest()
        {
            bool[] v1;
            bool[] v2;

            using (var doc = OpenDataDocument(@"Assembly18\Draw18.slddrw"))
            {
                var drw = (IXDrawing)m_App.Documents.Active;
                var table = drw.Sheets.Active.Annotations.OfType<IXTable>().First();

                var rowsRepo = table.Rows;

                var rows = rowsRepo.ToArray();

                v1 = rows.Select(r => r.Visible).ToArray();

                rows[1].Visible = false;
                rows[2].Visible = true;
                rows[3].Visible = true;

                v2 = rows.Select(r => r.Visible).ToArray();
            }

            CollectionAssert.AreEqual(new bool[] { true, true, false, true, true, true, false, true }, v1);
            CollectionAssert.AreEqual(new bool[] { true, false, true, true, true, true, false, true }, v2);
        }
    }
}
