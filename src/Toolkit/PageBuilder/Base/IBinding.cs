//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.UI.PropertyPage.Base;

namespace Xarial.XCad.Utils.PageBuilder.Base
{
    public interface IBinding
    {
        bool Silent { get; }
        IMetadata[] Metadata { get; }
        event Action<IBinding> Changed;

        event Action<IBinding> ModelUpdated;
        event Action<IBinding> ControlUpdated;

        IControl Control { get; }

        void UpdateControl();

        void UpdateDataModel();
    }
}