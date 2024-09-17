using System;
using System.Runtime.InteropServices;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.UI.PropertyPage.Structures;

namespace Xarial.XCad.Documentation
{
    [ComVisible(true), Guid("6A9790AB-983D-40F7-B2DA-B84411A39694")]
    public class PmpEventsAddIn : SwAddInEx
    {
        [ComVisible(true)]
        public class DataModel : SwPropertyManagerPageHandler
        {
            public string Text { get; set; }
        }

        #region Main

        private DataModel m_Data;

        private ISwPropertyManagerPage<DataModel> m_Page;

        public override void OnConnect()
        {
            m_Data = new DataModel();
            m_Page = this.CreatePage<DataModel>();

            #region DataChanged
            m_Page.DataChanged += OnDataChanged;
            #endregion DataChanged
            #region Closing
            m_Page.Closing += OnClosing;
            #endregion Closing
            #region Closed
            m_Page.Closed += OnClosed;
            #endregion Closed
        }

        #region DataChanged
        private void OnDataChanged()
        {
            var text = m_Data.Text;
            //handle the data changing, e.g. update preview
        }
        #endregion DataChanged
        #region Closing
        private void OnClosing(PageCloseReasons_e reason, PageClosingArg arg)
        {
            if (reason == PageCloseReasons_e.Okay)
            {
                if (string.IsNullOrEmpty(m_Data.Text))
                {
                    arg.Cancel = true;
                    arg.ErrorTitle = "Insert Note Error";
                    arg.ErrorMessage = "Please specify the note text";
                }
            }
        }
        #endregion Closing
        #region Closed
        private void OnClosed(PageCloseReasons_e reason)
        {
            if (reason == PageCloseReasons_e.Okay)
            {
                //do work
            }
            else
            {
                //release resources
            }
        }
        #endregion Closed
        #endregion Main
    }
}
