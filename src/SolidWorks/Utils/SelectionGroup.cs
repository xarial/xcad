//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using Xarial.XCad.SolidWorks.Documents;

namespace Xarial.XCad.SolidWorks.Utils
{
    internal class SelectionGroup : IDisposable
    {
        private readonly IModelDoc2 m_Model;
        private readonly ISelectionMgr m_SelMgr;

        private readonly bool m_IsSystemSelection;

        private readonly SwApplication m_App;

        internal SelectionGroup(SwDocument doc, bool systemSel)
        {
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            m_IsSystemSelection = systemSel;

            m_App = doc.OwnerApplication;

            m_Model = doc.Model;

            m_SelMgr = m_Model.ISelectionManager;

            if (m_IsSystemSelection)
            {
                m_SelMgr.SuspendSelectionList();
            }
            else 
            {
                m_Model.ClearSelection2(true);
            }
        }

        /// <summary>
        /// Add object to current selection list
        /// </summary>
        /// <param name="disp">Pointer to dispatch</param>
        /// <param name="selData">Optional selection data</param>
        /// <returns>Result of selection</returns>
        internal void Add(object disp, ISelectData selData = null)
        {
            if (disp == null)
            {
                throw new ArgumentNullException(nameof(disp));
            }

            //NOTE: ISelectionMgr::AddSelectionListObject fails, use the AddSelectionListObjects for system selection

            AddRange(new object[] { disp }, selData);
        }

        /// <summary>
        /// Adds multiple objects into selection list
        /// </summary>
        /// <param name="disps">Array of dispatches to select</param>
        /// <param name="selData">Optional selection data</param>
        /// <returns>Result of the selection</returns>
        internal void AddRange(object[] disps, ISelectData selData = null)
        {
            if (disps == null)
            {
                throw new ArgumentNullException(nameof(disps));
            }

            var dispWrappers = disps.Select(d => new DispatchWrapper(d)).ToArray();

            if (m_IsSystemSelection)
            {
                if (m_SelMgr.AddSelectionListObjects(dispWrappers, selData) != disps.Length)
                {
                    throw new Exception("Failed to add objects to selection list");
                }
            }
            else 
            {
                var cusrSelCount = m_SelMgr.GetSelectedObjectCount2(-1);

                if (m_Model.Extension.MultiSelect2(dispWrappers, true, selData) - cusrSelCount != disps.Length) 
                {
                    throw new Exception("Failed to select objects");
                }
            }
        }

        public void Dispose()
        {
            if (m_IsSystemSelection)
            {
                if (m_App.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2020))
                {
                    m_SelMgr.ResumeSelectionList2(false);
                }
                else 
                {
                    m_SelMgr.ResumeSelectionList();
                }
            }
            else 
            {
                m_Model.ClearSelection2(true);
            }
        }
    }
}