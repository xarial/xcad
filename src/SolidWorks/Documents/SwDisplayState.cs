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
        internal SwObject Owner { get; }

        internal SwDisplayStateDispatch(string name, SwObject owner) 
        {
            Name = name;
            Owner = owner;
        }

        internal virtual void GetDisplayStateOptions(out swDisplayStateOpts_e dispStateOpts, out string[] dispStateNames)
        {
            dispStateOpts = swDisplayStateOpts_e.swSpecifyDisplayState;
            dispStateNames = new string[] { Name };
        }
    }

    internal class SwDisplayStatePlaceholderDispatch : SwDisplayStateDispatch
    {
        internal SwDisplayStatePlaceholderDispatch(string name, SwObject owner) : base(name, owner)
        {
        }
    }

    internal class SwDocumentLevelDisplayStateDispatch : SwDisplayStateDispatch
    {
        internal SwDocumentLevelDisplayStateDispatch(SwDocument3D doc) : base("", doc)
        {
        }

        internal override void GetDisplayStateOptions(out swDisplayStateOpts_e dispStateOpts, out string[] dispStateNames)
        {
            dispStateOpts = swDisplayStateOpts_e.swAllDisplayState;
            dispStateNames = null;
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
                    if (DisplayState is SwDocumentLevelDisplayStateDispatch) 
                    {
                        
                    }

                    if (DisplayState.Owner is ISwConfiguration)
                    {
                        var ownerConf = (ISwConfiguration)DisplayState.Owner;

                        ownerConf.Configuration.RenameDisplayState(Name, value);
                        DisplayState.Name = value;
                    }
                    else 
                    {
                        throw new NotSupportedException("Renaming of document level display state is not supported");
                    }
                    
                }
                else 
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public override bool IsCommitted => m_Creator.IsCreated;

        public override object Dispatch => DisplayState;

        internal SwDisplayStateDispatch DisplayState => m_Creator.Element;

        public IXAppearanceRepository Appearances { get; }

        private readonly ElementCreator<SwDisplayStateDispatch> m_Creator;

        private readonly SwDisplayStatePlaceholderDispatch m_PlaceholderDispState;

        internal SwDisplayState(SwDisplayStateDispatch dispState, SwDocument3D ownerDoc, SwApplication ownerApp) : base(dispState, ownerDoc, ownerApp)
        {
            Appearances = new SwAppearanceCollection(this, ownerDoc, ownerApp);

            if (dispState is SwDisplayStatePlaceholderDispatch) 
            {
                m_PlaceholderDispState = (SwDisplayStatePlaceholderDispatch)dispState;
                dispState = null;
            }

            m_Creator = new ElementCreator<SwDisplayStateDispatch>(CreateDisplayState, dispState, dispState != null);
        }

        public override void Commit(CancellationToken cancellationToken) => m_Creator.Create(cancellationToken);

        private SwDisplayStateDispatch CreateDisplayState(CancellationToken token)
        {
            if (m_PlaceholderDispState?.Owner is SwConfiguration)
            {
                var ownerConf = (SwConfiguration)m_PlaceholderDispState.Owner;

                if (ownerConf.Configuration.CreateDisplayState(Name))
                {
                    return new SwDisplayStateDispatch(Name, ownerConf);
                }
                else
                {
                    throw new Exception("Failed to created display state");
                }
            }
            else 
            {
                throw new NotSupportedException("Document level display state creation is not supported");
            }
        }
    }
}
