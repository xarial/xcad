//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Documents;
using Xarial.XCad.SolidWorks.Documents.Exceptions;

namespace Xarial.XCad.SolidWorks.Documents
{
    public class SwComponent : SwSelObject, IXComponent
    {
        IXDocument3D IXComponent.Document => Document;
        IXComponentRepository IXComponent.Children => Children;

        public IComponent2 Component { get; }

        private readonly SwAssembly m_ParentAssembly;

        public SwComponentCollection Children { get; }

        internal SwComponent(IComponent2 comp, SwAssembly parentAssembly) : base(comp)
        {
            m_ParentAssembly = parentAssembly;
            Component = comp;
            Children = new SwComponentCollection(parentAssembly, comp);
        }

        public string Name 
        {
            get => Component.Name2;
            set => Component.Name2 = value;
        }

        public SwDocument3D Document 
        {
            get 
            {
                var compModel = Component.IGetModelDoc();

                if (compModel != null)
                {
                    return (SwDocument3D)m_ParentAssembly.App.Documents[compModel];
                }
                else 
                {
                    throw new ComponentNotLoadedException(Name);
                }
            }
        }

        public override void Select(bool append)
        {
            if(!Component.Select4(append, null, false)) 
            {
                throw new Exception("Failed to select component");
            }
        }
    }
}
