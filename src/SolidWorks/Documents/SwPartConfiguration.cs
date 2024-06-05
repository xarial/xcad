//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
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

        internal SwPartConfiguration(IConfiguration conf, SwPart part, SwApplication app, bool created) 
            : base(conf, part, app, created)
        {
            m_Part = part;
            CutLists = new SwPartCutListItemCollection(this, part);
        }

        public IXCutListItemRepository CutLists { get; }

        public IXMaterial Material
        {
            get => m_Part.GetMaterial(Name);
            set => m_Part.SetMaterial(value, Name);
        }
    }
}