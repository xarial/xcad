//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xarial.XCad.Data;
using Xarial.XCad.Documents;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit.Utils;
using System.Diagnostics;
using System.Collections;
using Xarial.XCad.Base;

namespace Xarial.XCad.SolidWorks.Documents
{
    /// <summary>
    /// SOLIDWORKS specific display state
    /// </summary>
    public interface ISwDisplayState : IXDisplayState 
    {
    }

    internal class SwDisplayStateDispatch 
    {
        internal string Name { get; set; }

        internal SwDisplayStateDispatch(string name) 
        {
            Name = name;
        }
    }

    [DebuggerDisplay("{" + nameof(Name) + "}")]
    internal class SwDisplayState : SwObject, ISwDisplayState
    {
        public string Name 
        {
            get
            {
                if (IsCommitted)
                {
                    return DisplayState.Name;
                }
                else 
                {
                    return m_Creator.CachedProperties.Get<string>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    m_OwnerConf.Configuration.RenameDisplayState(Name, value);
                    DisplayState.Name = value;
                }
                else 
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public override bool IsCommitted => m_Creator.IsCreated;

        internal SwDisplayStateDispatch DisplayState => m_Creator.Element;

        public IXAppearanceRepository Appearances { get; }

        private readonly ElementCreator<SwDisplayStateDispatch> m_Creator;

        private readonly SwConfiguration m_OwnerConf;

        internal SwDisplayState(SwDisplayStateDispatch dispState, SwConfiguration ownerConf, SwDocument3D ownerDoc, SwApplication ownerApp) : base(dispState, ownerDoc, ownerApp)
        {
            Appearances = new SwAppearanceCollection(this, ownerConf, ownerDoc, ownerApp);

            m_OwnerConf = ownerConf;
            m_Creator = new ElementCreator<SwDisplayStateDispatch>(CreateDisplayState, dispState, dispState != null);
        }

        public override void Commit(CancellationToken cancellationToken) => m_Creator.Create(cancellationToken);

        private SwDisplayStateDispatch CreateDisplayState(CancellationToken token)
        {
            if (m_OwnerConf.Configuration.CreateDisplayState(Name))
            {
                return new SwDisplayStateDispatch(Name);
            }
            else 
            {
                throw new Exception("Failed to created display state");
            }
        }
    }
}
