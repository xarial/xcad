//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry;

namespace Xarial.XCad.SwDocumentManager.Documents
{
    public interface ISwDmAssembly : ISwDmDocument3D, IXAssembly
    {
        new ISwDmAssemblyConfigurationCollection Configurations { get; }
    }

    internal class SwDmAssembly : SwDmDocument3D, ISwDmAssembly
    {
        #region Not Supported
        
        IXAssemblyBoundingBox IXAssembly.PreCreateBoundingBox() => throw new NotSupportedException();
        IXAssemblyMassProperty IXAssembly.PreCreateMassProperty() => throw new NotSupportedException();

        #endregion

        private readonly Lazy<SwDmAssemblyConfigurationCollection> m_LazyConfigurations;

        public SwDmAssembly(ISwDmApplication dmApp, ISwDMDocument doc, bool isCreated,
            Action<ISwDmDocument> createHandler, Action<ISwDmDocument> closeHandler, bool? isReadOnly)
            : base(dmApp, doc, isCreated, createHandler, closeHandler, isReadOnly)
        {
            m_LazyConfigurations = new Lazy<SwDmAssemblyConfigurationCollection>(() => new SwDmAssemblyConfigurationCollection(this));
        }

        IXAssemblyConfigurationRepository IXAssembly.Configurations => ((ISwDmAssembly)this).Configurations;

        ISwDmAssemblyConfigurationCollection ISwDmAssembly.Configurations => m_LazyConfigurations.Value;

        public override ISwDmConfigurationCollection Configurations => ((ISwDmAssembly)this).Configurations;
    }

    internal class SwDmVirtualAssembly : SwDmAssembly
    {
        private readonly SwDmDocument m_Owner;

        public SwDmVirtualAssembly(ISwDmApplication dmApp, ISwDMDocument doc, SwDmDocument owner, bool isCreated,
            Action<ISwDmDocument> createHandler, Action<ISwDmDocument> closeHandler, bool? isReadOnly) 
            : base(dmApp, doc, isCreated, createHandler, closeHandler, isReadOnly)
        {
            m_Owner = owner;
            m_Owner.Disposed += OnOwnerDisposed;
        }

        private void OnOwnerDisposed(SwDmDocument owner)
        {
            this.Close();
        }

        public override string Title
        {
            get => SwDmVirtualDocumentHelper.GetTitle(base.Title);
            set => base.Title = value; 
        }

        public override bool IsDirty
        {
            get => base.IsDirty;
            set
            {
                base.IsDirty = value;

                if (value)
                {
                    m_Owner.IsDirty = true;
                }
            }
        }
    }
}
