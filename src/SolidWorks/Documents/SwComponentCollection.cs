//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks.Documents.Services;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwComponentCollection : IXComponentRepository
    {
        new ISwComponent this[string name] { get; }
    }

    internal abstract class SwComponentCollection : ISwComponentCollection
    {
        private static readonly BatchComponentsInserter m_BatchCompsInserter = new BatchComponentsInserter();

        internal static void BatchAdd(SwAssembly assm, SwComponent[] comps, bool commitComps)
            => m_BatchCompsInserter.BatchAdd(assm, comps, commitComps);

        IXComponent IXRepository<IXComponent>.this[string name] => this[name];

        public ISwComponent this[string name] => (SwComponent)RepositoryHelper.Get(this, name);

        public bool TryGet(string name, out IXComponent ent)
        {
            var comp = GetChildren().FirstOrDefault(c => string.Equals(GetRelativeName(c), name, StringComparison.CurrentCultureIgnoreCase));

            if (comp != null)
            {
                ent = RootAssembly.CreateObjectFromDispatch<SwComponent>(comp);
                return true;
            }
            else
            {
                ent = null;
                return false;
            }
        }

        public int Count
        {
            get 
            {
                if (RootAssembly.IsCommitted)
                {
                    if (RootAssembly.Model.IsOpenedViewOnly())
                    {
                        throw new Exception("Components count is inaccurate in Large Design Review assembly");
                    }

                    return GetChildrenCount();
                }
                else
                {
                    throw new Exception("Assembly is not committed");
                }
            }
        }

        public int TotalCount 
        {
            get 
            {
                if (RootAssembly.IsCommitted)
                {
                    if (RootAssembly.Model.IsOpenedViewOnly())
                    {
                        throw new Exception("Total components count is inaccurate in Large Design Review assembly");
                    }

                    return GetTotalChildrenCount();
                }
                else
                {
                    throw new Exception("Assembly is not committed");
                }
            }
        }

        internal SwAssembly RootAssembly { get; }

        internal SwComponentCollection(SwAssembly assm)
        {
            RootAssembly = assm;
        }

        public void AddRange(IEnumerable<IXComponent> ents, CancellationToken cancellationToken)
            => BatchAdd(RootAssembly, ents.Cast<SwComponent>().ToArray(), true);

        protected abstract IEnumerable<IComponent2> GetChildren();
        protected abstract int GetChildrenCount();
        protected abstract int GetTotalChildrenCount();

        public IEnumerator<IXComponent> GetEnumerator()
        {
            if (RootAssembly.IsCommitted)
            {
                if (RootAssembly.Model.IsOpenedViewOnly())
                {
                    throw new Exception("Components cannot be extracted for the Large Design Review assembly");
                }

                return (GetChildren() ?? new IComponent2[0])
                    .Select(c => RootAssembly.CreateObjectFromDispatch<SwComponent>(c)).GetEnumerator();
            }
            else 
            {
                throw new Exception("Assembly is not committed");
            }
        }

        public void RemoveRange(IEnumerable<IXComponent> ents, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private string GetRelativeName(IComponent2 comp)
        {
            var parentComp = comp.GetParent();

            if (parentComp == null)
            {
                return comp.Name2;
            }
            else
            {
                if (comp.Name2.StartsWith(parentComp.Name2, StringComparison.CurrentCultureIgnoreCase))
                {
                    return comp.Name2.Substring(parentComp.Name2.Length + 1);
                }
                else
                {
                    throw new Exception("Invalid component name");
                }
            }
        }

        public T PreCreate<T>() where T : IXComponent
            => RepositoryHelper.PreCreate<IXComponent, T>(this,
                () => new SwPartComponent(null, RootAssembly, RootAssembly.OwnerApplication),
                () => new SwAssemblyComponent(null, RootAssembly, RootAssembly.OwnerApplication));
    }

    public static class SwComponentCollectionExtension
    {
        /// <summary>
        /// Pre creates new component from path
        /// </summary>
        /// <param name="docsColl">Documents collection</param>
        /// <param name="path"></param>
        /// <returns>Pre-created document</returns>
        public static ISwComponent PreCreateFromPath(this ISwComponentCollection compsColl, string path)
        {
            var ext = Path.GetExtension(path);

            ISwComponent comp;

            switch (ext.ToLower())
            {
                case ".sldprt":
                    comp = compsColl.PreCreate<ISwPartComponent>();
                    
                    break;

                case ".sldasm":
                    comp = compsColl.PreCreate<ISwAssemblyComponent>();
                    break;
                    
                default:
                    throw new NotSupportedException("Only parts and assemblies are supported");
            }

            var app = ((SwComponentCollection)compsColl).RootAssembly.OwnerApplication;

            comp.ReferencedDocument = (ISwDocument3D)app.Documents.PreCreateFromPath(path);

            return comp;
        }
    }
}
