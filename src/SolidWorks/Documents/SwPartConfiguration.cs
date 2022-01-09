//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using Xarial.XCad.Documents;
using Xarial.XCad.Features;
using Xarial.XCad.SolidWorks.Documents.Exceptions;
using Xarial.XCad.SolidWorks.Features;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwPartConfiguration : ISwConfiguration, IXPartConfiguration
    {
    }

    internal class SwPartConfiguration : SwConfiguration, ISwPartConfiguration
    {
        private readonly SwPart m_Part;

        internal SwPartConfiguration(IConfiguration conf, SwPart part, ISwApplication app, bool created) 
            : base(conf, part, app, created)
        {
            m_Part = part;
        }

        public virtual IEnumerable<IXCutListItem> CutLists
        {
            get
            {
                var part = m_Part.Part;

                IEnumerable<IBody2> IterateBodies() =>
                    (part.GetBodies2((int)swBodyType_e.swSolidBody, false) as object[] ?? new object[0]).Cast<IBody2>();

                if (part.IsWeldment()
                    || IterateBodies().Any(b => b.IsSheetMetal()))
                {
                    var activeConf = m_Part.Configurations.Active;

                    var checkedConfigsConflict = false;

                    foreach (var cutList in ((SwFeatureManager)m_Part.Features).IterateCutLists(m_Part, activeConf))
                    {
                        if (!checkedConfigsConflict)
                        {
                            if (activeConf.Name != this.Configuration.Name)
                            {
                                throw new ConfigurationSpecificCutListNotSupportedException();
                            }

                            checkedConfigsConflict = true;
                        }

                        yield return cutList;
                    }
                }
            }
        }
    }
}