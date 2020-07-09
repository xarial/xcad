using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.UI.Commands;

namespace Xarial.XCad.Documentation
{
    [ComVisible(true), Guid("55651FB2-CE56-4904-8FF5-3D5D29BAC34D")]
    public class PmpPageOverviewAddIn : SwAddInEx
    {
        //--- Ignore
        public class DataModelIgnore
        {
            public string Text { get; set; }

            [ExcludeControl]
            public int CalculatedField { get; set; } //control will not be generated for this field
        }
        //---

        //--- Simple
        public class DataModelSimple
        {
            public string Text { get; set; }
            public int Size { get; set; } = 48;
            public double Number { get; set; } = 10.5;
        }
        //---


        //--- PMPageHandler
        [ComVisible(true)]
        public class MyPMPageData : SwPropertyManagerPageHandler
        {
            public DataModelSimple Simple { get; set; }
            public DataModelIgnore Ignore { get; set; }
        }
        //---

        //--- CreateInstance
        private SwPropertyManagerPage<MyPMPageData> m_Page;
        private MyPMPageData m_Data = new MyPMPageData();

        private enum Commands_e
        {
            ShowPmpPage
        }

        public override void OnConnect()
        {
            m_Page = this.CreatePage<MyPMPageData>();
            this.CommandManager.AddCommandGroup<Commands_e>().CommandClick += ShowPmpPage;
        }

        private void ShowPmpPage(Commands_e cmd)
        {
            m_Page.Closed += OnPageClosed;
            m_Page.Show(m_Data);
        }

        private void OnPageClosed(PageCloseReasons_e reason)
        {
            Debug.Print($"Text: {m_Data.Simple.Text}");
            Debug.Print($"Size: {m_Data.Simple.Size}");
            Debug.Print($"Number: {m_Data.Simple.Number}");
        }
        //---
    }
}
