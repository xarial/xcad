//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.SolidWorks.UI.PropertyPage;

namespace Xarial.XCad.SolidWorks.Services
{
    public interface IPropertyPageHandlerProvider 
    {
        SwPropertyManagerPageHandler CreateHandler(ISldWorks app, Type handlerType);
    }

    internal class PropertyPageHandlerProvider : IPropertyPageHandlerProvider
    {
        public SwPropertyManagerPageHandler CreateHandler(ISldWorks app, Type handlerType)
            => (SwPropertyManagerPageHandler)Activator.CreateInstance(handlerType);
    }
}
