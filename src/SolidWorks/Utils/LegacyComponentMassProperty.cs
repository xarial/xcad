//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Documents;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry.Exceptions;

namespace Xarial.XCad.SolidWorks.Utils
{
    /// <summary>
    /// Component specific mass property information
    /// </summary>
    internal class LegacyComponentMassProperty
    {
        internal IXDocument3D Document { get; }
        internal IXComponent Component { get; }
        internal IMassProperty MassProperty { get; }
        internal IXUnits UserUnit { get; }

        internal LegacyComponentMassProperty(IXDocument3D doc, IXComponent component, IMassProperty massProperty, IXUnits userUnit)
        {
            Document = doc;
            Component = component;
            MassProperty = massProperty;
            UserUnit = userUnit;
        }
    }

    /// <summary>
    /// Helper class to retrieve the IMassProperty for the specific component's model for workarounds purposes
    /// </summary>
    internal class LegacyComponentMassPropertyLazy : Lazy<LegacyComponentMassProperty>
    {
        internal LegacyComponentMassPropertyLazy(Func<IXComponent[]> compsFunc, Func<IXUnits> unitsFunc = null) 
            : base(() => CreateComponentMassProperty(compsFunc.Invoke(), unitsFunc?.Invoke()))
        {
        }

        private static LegacyComponentMassProperty CreateComponentMassProperty(IXComponent[] comps, IXUnits units) 
        {
            if (comps?.Length == 1)
            {
                var comp = comps.First();
                var refDoc = (ISwDocument3D)comp.ReferencedDocument;

                if (!refDoc.IsCommitted)
                {
                    throw new NotLoadedMassPropertyComponentException(comp);
                }

                var massPrps = refDoc.Model.Extension.CreateMassProperty();

                //NOTE: always resolving the system units as it is requried to get units from the assembly (not the component) for the units and also by some reasons incorrect COG is returned for the user units
                massPrps.UseSystemUnits = true;

                return new LegacyComponentMassProperty(refDoc, comp, massPrps, units);
            }
            else
            {
                throw new NotSupportedException("Only single component is supported for scope in the assembly");
            }
        }
    }
}
