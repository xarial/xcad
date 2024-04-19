using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Annotations;
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

            using (var doc = OpenDataDocument(@"Assembly18\Draw18.slddrw"))
            {
                var drw = (IXDrawing)m_App.Documents.Active;
                var table = drw.Sheets.Active.Annotations.OfType<IXTable>().First();

                var rows = table.Rows.ToArray();

                rows[4].Index = 0;
                rows[1].Index = 7;
                rows[3].Index = 1;
                Assert.Throws<TableRowOperationException>(() => rows[6].Index = 7);
                
                indices = rows.Select(r => r.Index).ToArray();

                data = table.Read(false);
            }

            CollectionAssert.AreEqual(new int[] { 2, 7, 3, 1, 0, 4, 5, 6 }, indices);

            Assert.AreEqual(6, data.Columns.Count);
            Assert.AreEqual(8, data.Rows.Count);
            CollectionAssert.AreEqual(new string[] { "ITEM NO.", "PART NUMBER", "SW-Title(Title)", "DESCRIPTION", "INDEX", "QTY." }, data.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray());
            CollectionAssert.AreEqual(new string[] { "1", "Part2", "", "", "4", "1" }, data.Rows[0].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "2", "", "", "", "3", "" }, data.Rows[1].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "3", "Test1", "Test2", "", "0", "" }, data.Rows[2].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "", "", "", "Desc2", "2", "" }, data.Rows[3].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "", "", "", "", "5", "" }, data.Rows[4].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "4", "", "", "Desc3", "6", "" }, data.Rows[5].ItemArray.Select(i => i?.ToString()));
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

                i1 = bomTable.Rows[0].ItemNumber;
                i2 = bomTable.Rows[2].ItemNumber;
                i3 = bomTable.Rows[3].ItemNumber;
                i4 = bomTable.Rows[6].ItemNumber;
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
            string i2;
            string i3;

            using (var doc = OpenDataDocument(@"Assembly18\Draw18.slddrw"))
            {
                var drw = (IXDrawing)m_App.Documents.Active;
                var bomTable = drw.Sheets.Active.Annotations.OfType<IXBomTable>().First();

                bomTable.Rows[0].ItemNumber = BomItemNumber.NoNumber;
                Assert.Throws<TableRowOperationException>(() => bomTable.Rows[2].ItemNumber = BomItemNumber.Auto);
                bomTable.Rows[3].ItemNumber = BomItemNumber.NoNumber;
                bomTable.Rows[5].ItemNumber = BomItemNumber.Auto;
                Assert.Throws<TableRowOperationException>(() => bomTable.Rows[6].ItemNumber = 5);

                i1 = ((ISwTable)bomTable).TableAnnotation.Text2[1, 0, true];
                i2 = ((ISwTable)bomTable).TableAnnotation.Text2[4, 0, true];
                i3 = ((ISwTable)bomTable).TableAnnotation.Text2[6, 0, true];
            }

            Assert.AreEqual("", i1);
            Assert.AreEqual("", i2);
            Assert.AreEqual("3", i3);
        }

        [Test]
        public void AddRowTest()
        {
            int[] indices;

            DataTable data;

            using (var doc = OpenDataDocument(@"Assembly18\Draw18.slddrw"))
            {
                var drw = (IXDrawing)m_App.Documents.Active;
                var table = drw.Sheets.Active.Annotations.OfType<IXTable>().First();

                var rows = table.Rows.ToArray();

                var r1 = table.Rows.Insert(0);
                r1.Cells[1].Value = "A1";
                r1.Cells[3].Value = "A3";

                var r2 = table.Rows.Insert(9);
                r2.Cells[1].Value = "B1";
                r2.Cells[3].Value = "B3";

                var r3 = table.Rows.Insert(5);
                r3.Cells[1].Value = "C1";
                r3.Cells[3].Value = "C3";

                Assert.Throws<TableRowOperationException>(() => table.Rows.Insert(3));

                indices = rows.Select(r => r.Index).ToArray();

                data = table.Read(false);
            }

            CollectionAssert.AreEqual(new int[] { 1, 2, 3, 4, 6, 7, 8, 9 }, indices);

            Assert.AreEqual(6, data.Columns.Count);
            Assert.AreEqual(11, data.Rows.Count);

            CollectionAssert.AreEqual(new string[] { "ITEM NO.", "PART NUMBER", "SW-Title(Title)", "DESCRIPTION", "INDEX", "QTY." }, data.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray());
            CollectionAssert.AreEqual(new string[] { "1", "A1", "", "A3", "", "" }, data.Rows[0].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "2", "Test1", "Test2", "", "0", "" }, data.Rows[1].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "3", "Part1", "", "Desc1", "1", "1" }, data.Rows[2].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "", "", "", "Desc2", "2", "" }, data.Rows[3].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "4", "", "", "", "3", "" }, data.Rows[4].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "5", "C1", "", "C3", "", "" }, data.Rows[5].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "6", "Part2", "", "", "4", "1" }, data.Rows[6].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "", "", "", "", "5", "" }, data.Rows[7].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "7", "", "", "Desc3", "6", "" }, data.Rows[8].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "8", "", "", "Desc4", "7", "" }, data.Rows[9].ItemArray.Select(i => i?.ToString()));
            CollectionAssert.AreEqual(new string[] { "9", "B1", "", "B3", "", "" }, data.Rows[10].ItemArray.Select(i => i?.ToString()));
        }
    }
}
