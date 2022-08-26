//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Documents;

namespace Xarial.XCad.Toolkit.Utils
{
    public class ViewFreezer : IDisposable
    {
        private readonly IXModelView m_ModelView;

        public ViewFreezer(IXDocument doc)
        {
            m_ModelView = doc.ModelViews.Active;
            m_ModelView.Freeze(true);
        }

        public void Dispose()
        {
            try
            {
                m_ModelView.Freeze(false);
                m_ModelView.Update();
            }
            catch 
            {
            }
        }
    }
}
