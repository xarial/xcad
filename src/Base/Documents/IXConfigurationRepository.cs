﻿//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Documents.Delegates;

namespace Xarial.XCad.Documents
{
    public interface IXConfigurationRepository : IXRepository<IXConfiguration>
    {
        event ConfigurationActivatedDelegate ConfigurationActivated;

        IXConfiguration Active { get; }
    }
}
