//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Documents.Delegates
{
    /// <summary>
    /// Delegate for <see cref="IXConfigurationRepository.ConfigurationActivated"/> event
    /// </summary>
    /// <param name="doc">Document owner of this configuration</param>
    /// <param name="newConf">Configuration which is activated</param>
    public delegate void ConfigurationActivatedDelegate(IXDocument3D doc, IXConfiguration newConf);
}