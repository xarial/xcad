//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
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
        {
            if (handlerType.GetConstructor(Type.EmptyTypes) != null)
            {
                var handler = Activator.CreateInstance(handlerType);

                if (handler is SwPropertyManagerPageHandler)
                {
                    return (SwPropertyManagerPageHandler)handler;
                }
                else
                {
                    throw new InvalidCastException($"{handlerType.FullName} must be COM-visible and inherit {typeof(SwPropertyManagerPageHandler).FullName}");
                }
            }
            else 
            {
                throw new Exception($"{handlerType.FullName} must have a public parameterless constructor");
            }
        }
    }
}
