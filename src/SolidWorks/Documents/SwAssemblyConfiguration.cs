//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using Xarial.XCad.Documents;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwAssemblyConfiguration : ISwConfiguration, IXAssemblyConfiguration
    {
    }

    internal class SwAssemblyConfiguration : SwConfiguration, ISwAssemblyConfiguration
    {
        internal SwAssemblyConfiguration(SwAssembly assm, IConfiguration conf, bool created) : base(assm, conf, created)
        {
            Components = new SwAssemblyComponentCollection(assm, conf);
        }

        public IXComponentRepository Components { get; }
    }
}