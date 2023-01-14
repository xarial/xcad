//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xarial.XCad.Base;
using Xarial.XCad.Services;
using Xarial.XCad.Toolkit.Base;
using Xarial.XCad.Toolkit.Exceptions;

namespace Xarial.XCad.SolidWorks.Base
{
    internal class SwWorkUnit : IXWorkUnit
    {
        public WorkUnitOperationDelegate Operation
        {
            get => m_Creator.CachedProperties.Get<WorkUnitOperationDelegate>();
            set
            {
                if (!IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value);
                }
                else
                {
                    throw new CommittedElementPropertyChangeNotSupported();
                }
            }
        }

        public bool IsCommitted => m_Creator.IsCreated;

        public IXWorkUnitResult Result => m_Creator.Element;

        private readonly SwApplication m_App;
        private readonly IElementCreator<IXWorkUnitResult> m_Creator;

        internal SwWorkUnit(SwApplication app) 
        {
            m_App = app;
            m_Creator = new ElementCreator<IXWorkUnitResult>(RunWorkUnit, null, false);
        }

        public void Commit(CancellationToken cancellationToken)
            => m_Creator.Create(cancellationToken);

        private IXWorkUnitResult RunWorkUnit(CancellationToken cancellationToken) 
        {
            if (Operation != null)
            {
                using (var prg = m_App.CreateProgress()) 
                {
                    try
                    {
                        return Operation.Invoke(prg, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        return new XWorkUnitErrorResult(ex);
                    }
                }
            }
            else 
            {
                throw new Exception("Operation is not specified");
            }
        }
    }
}
