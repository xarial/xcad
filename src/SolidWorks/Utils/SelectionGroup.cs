//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
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
        private ISelectionMgr m_SelMgr;

        internal SelectionGroup(ISelectionMgr selMgr)
        {
            if (selMgr == null)
            {
                throw new ArgumentNullException(nameof(selMgr));
            }

            m_SelMgr = selMgr;

            m_SelMgr.SuspendSelectionList();
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

            if (!m_SelMgr.AddSelectionListObject(new DispatchWrapper(disp), selData)) 
            {
                throw new Exception("Failed to add object to selection list");
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

            if (m_SelMgr.AddSelectionListObjects(dispWrappers, selData) != disps.Length) 
            {
                throw new Exception("Failed to add objects to selection list");
            }
        }

        public void Dispose()
        {
            m_SelMgr.ResumeSelectionList();
        }
    }
}