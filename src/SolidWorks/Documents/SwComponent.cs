using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Documents;

namespace Xarial.XCad.SolidWorks.Documents
{
    public class SwComponent : SwSelObject, IXComponent
    {
        public IComponent2 Component { get; }

        internal SwComponent(IComponent2 comp) : base(comp)
        {
            Component = comp;
        }

        public string Name 
        {
            get => Component.Name2;
            set => Component.Name2 = value;
        }

        public IXDocument3D Document => null;

        public override void Select(bool append)
        {
            if(!Component.Select4(append, null, false)) 
            {
                throw new Exception("Failed to select component");
            }
        }
    }
}
