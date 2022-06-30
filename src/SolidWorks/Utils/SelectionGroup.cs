//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Xarial.XCad.SolidWorks.Utils
{
    internal class SelectionGroup : IDisposable
    {
        private readonly IModelDoc2 m_Model;
        private readonly ISelectionMgr m_SelMgr;

        private readonly bool m_IsSystemSelection;

        internal SelectionGroup(IModelDoc2 model, bool systemSel)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            m_IsSystemSelection = systemSel;

            m_Model = model;

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

            if (m_IsSystemSelection)
            {
                if (!m_SelMgr.AddSelectionListObject(new DispatchWrapper(disp), selData))
                {
                    throw new Exception("Failed to add object to selection list");
                }
            }
            else 
            {
                AddRange(new object[] { disp }, selData);
            }
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

                if (m_Model.Extension.MultiSelect2(dispWrappers, true, null) - cusrSelCount != disps.Length) 
                {
                    throw new Exception("Failed to select objects");
                }
            }
        }

        public void Dispose()
        {
            if (m_IsSystemSelection)
            {
                m_SelMgr.ResumeSelectionList();
            }
            else 
            {
                m_Model.ClearSelection2(true);
            }
        }
    }
}