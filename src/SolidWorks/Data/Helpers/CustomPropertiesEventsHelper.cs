//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.Toolkit.Services;

namespace Xarial.XCad.SolidWorks.Data.Helpers
{
    /// <summary>
    /// Type of the modification action on custom properties
    /// </summary>
    internal enum CustomPropertyChangeAction_e
    {
        /// <summary>
        /// New custom property is added
        /// </summary>
        Add,

        /// <summary>
        /// Custom property is removed
        /// </summary>
        Delete,

        /// <summary>
        /// Custom property value is changed
        /// </summary>
        Modify
    }

    /// <summary>
    /// Custom Property modification data
    /// </summary>
    internal class CustomPropertyModifyData
    {
        /// <summary>
        /// Type of the modification
        /// </summary>
        internal CustomPropertyChangeAction_e Action { get; private set; }

        /// <summary>
        /// Name of the custom property
        /// </summary>
        internal string Name { get; private set; }

        /// <summary>
        /// Configuration of custom property. Empty string for the file specific (generic) custom property
        /// </summary>
        internal string ConfigurationName { get; private set; }

        /// <summary>
        /// Value of the custom property
        /// </summary>
        internal string Value { get; private set; }

        internal CustomPropertyModifyData(CustomPropertyChangeAction_e type, string name, string conf, string val)
        {
            Action = type;
            Name = name;
            ConfigurationName = conf;
            Value = val;
        }
    }

    /// <summary>
    /// Delegate of <see cref="DocumentHandler.CustomPropertyModify"/> event
    /// </summary>
    /// <param name="docHandler">Document Handler which sends this notification</param>
    /// <param name="modifications">Array of all modifications in custom properties</param>
    internal delegate void CustomPropertyModifyDelegate(CustomPropertyModifyData[] modifications);

    internal class CustomPropertiesEventsHelper : EventsHandler<CustomPropertyModifyDelegate>
    {
        private class PropertiesList : Dictionary<string, string>
        {
            internal PropertiesList(ICustomPropertyManager prpsMgr) : base(StringComparer.CurrentCultureIgnoreCase)
            {
                var prpNames = prpsMgr.GetNames() as string[];

                if (prpNames != null)
                {
                    foreach (var prpName in prpNames)
                    {
                        string val;
                        string resVal;
                        bool wasRes;
                        prpsMgr.Get5(prpName, true, out val, out resVal, out wasRes);
                        Add(prpName, val);
                    }
                }
            }
        }

        private class PropertiesSet : Dictionary<string, PropertiesList>
        {
            internal PropertiesSet(IModelDoc2 model) : base(StringComparer.CurrentCultureIgnoreCase)
            {
                Add("", new PropertiesList(model.Extension.CustomPropertyManager[""]));

                var confNames = model.GetConfigurationNames() as string[];

                if (confNames != null)
                {
                    foreach (var confName in confNames)
                    {
                        Add(confName, new PropertiesList(model.Extension.CustomPropertyManager[confName]));
                    }
                }
            }
        }

        public event CustomPropertyModifyDelegate CustomPropertiesModified 
        {
            add 
            {
                Attach(value);
            }
            remove
            {
                Detach(value);
            }
        }

        #region WinAPI

        private delegate bool EnumWindowProc(IntPtr handle, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool EnumThreadWindows(uint threadId, EnumWindowProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindow(IntPtr hWnd);

        #endregion

        private readonly SldWorks m_App;
        private readonly ISwDocument m_Doc;

        private IModelDoc2 Model => m_Doc.Model;

        private IntPtr m_CurrentSummaryHandle;

        private PropertiesSet m_CurPrpsSet;

        public CustomPropertiesEventsHelper(ISldWorks app, ISwDocument doc)
        {
            m_App = (SldWorks)app;
            m_Doc = doc;
        }

        private int OnIdleNotify()
        {
            if (m_App.ActiveDoc == Model)
            {
                if (m_CurrentSummaryHandle != IntPtr.Zero)
                {
                    if (!IsWindow(m_CurrentSummaryHandle))
                    {
                        FindDifferences(m_CurPrpsSet, new PropertiesSet(Model));
                        m_CurrentSummaryHandle = IntPtr.Zero;
                        m_CurPrpsSet = null;
                    }
                }
            }

            return 0;
        }

        private void FindDifferences(PropertiesSet oldSet, PropertiesSet newSet)
        {
            var modData = new List<CustomPropertyModifyData>();

            foreach (var conf in oldSet.Keys)
            {
                var oldPrsList = oldSet[conf];
                var newPrsList = newSet[conf];

                var addedPrpNames = newPrsList.Keys.Except(oldPrsList.Keys);

                modData.AddRange(addedPrpNames
                    .Select(newPrpName => new CustomPropertyModifyData(
                        CustomPropertyChangeAction_e.Add, newPrpName, conf, newPrsList[newPrpName])));

                var removedPrpNames = oldPrsList.Keys.Except(newPrsList.Keys);

                modData.AddRange(removedPrpNames
                    .Select(deletedPrpName => new CustomPropertyModifyData(
                        CustomPropertyChangeAction_e.Delete, deletedPrpName, conf, oldPrsList[deletedPrpName])));

                var commonPrpNames = oldPrsList.Keys.Intersect(newPrsList.Keys);

                modData.AddRange(commonPrpNames.Where(prpName => newPrsList[prpName] != oldPrsList[prpName])
                    .Select(prpName => new CustomPropertyModifyData(
                        CustomPropertyChangeAction_e.Modify, prpName, conf, newPrsList[prpName])));
            }

            if (modData.Any())
            {
                Delegate?.Invoke(modData.ToArray());
            }
        }

        private int OnCommandCloseNotify(int Command, int reason)
        {
            if (m_App.ActiveDoc == Model)
            {
                const int swCommands_File_Summaryinfo = 963;

                if (Command == swCommands_File_Summaryinfo)
                {
                    if (!CaptureCurrentProperties())
                    {
                        throw new Exception("Failed to find the summary information dialog");
                    }
                }
            }

            return 0;
        }

        private bool CaptureCurrentProperties()
        {
            var handle = GetSummaryInfoDialogHandle();

            if (handle != IntPtr.Zero)
            {
                m_CurPrpsSet = new PropertiesSet(Model);
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool FindSymmaryInfoDialog(IntPtr handle, IntPtr lParam)
        {
            var captionLength = GetWindowTextLength(handle) + 1;
            var caption = new StringBuilder(captionLength);

            if (GetWindowText(handle, caption, captionLength) > 0)
            {
                //TODO: implement support for other languages
                if (caption.ToString() == "Summary Information")
                {
                    var clsName = new StringBuilder(260);

                    GetClassName(handle, clsName, clsName.Capacity);

                    if (clsName.ToString() == "#32770")
                    {
                        m_CurrentSummaryHandle = handle;
                    }
                }
            }

            return true;
        }

        private IntPtr GetSummaryInfoDialogHandle()
        {
            m_CurrentSummaryHandle = IntPtr.Zero;

            var prc = Process.GetProcessById(m_App.GetProcessID());

            for (int i = 0; i < prc.Threads.Count; i++)
            {
                var threadId = (uint)prc.Threads[i].Id;
                EnumThreadWindows(threadId, FindSymmaryInfoDialog, IntPtr.Zero);
            }

            return m_CurrentSummaryHandle;
        }
        
        protected override void SubscribeEvents()
        {
            m_App.CommandCloseNotify += OnCommandCloseNotify;
            m_App.OnIdleNotify += OnIdleNotify;

            CaptureCurrentProperties();
        }

        protected override void UnsubscribeEvents()
        {
            m_App.CommandCloseNotify -= OnCommandCloseNotify;
            m_App.OnIdleNotify -= OnIdleNotify;
        }
    }
}
