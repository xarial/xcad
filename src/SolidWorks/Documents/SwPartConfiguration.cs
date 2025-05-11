﻿//*********************************************************************
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
            get 
            {
                var materialName = m_Part.Part.GetMaterialPropertyName2(Name, out var database);

                if (!string.IsNullOrEmpty(materialName))
                {
                    return new SwMaterial(materialName, OwnerApplication.MaterialDatabases.GetOrTemp(database));
                }
                else 
                {
                    return null;
                }
            }
            set 
            {
                if (value != null)
                {
                    m_Part.Part.SetMaterialPropertyName2(Name, value.Database.Name, value.Name);
                }
                else 
                {
                    m_Part.Part.SetMaterialPropertyName2(Name, "", "");
                }
            }
        }
    }
}