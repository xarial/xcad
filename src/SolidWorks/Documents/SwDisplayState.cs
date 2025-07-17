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
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IXAppearance this[IHasColor[] objs] => SwAppearance.FromObjects(DisplayState, objs, AppearanceLevel_e.Component, OwnerDocument, OwnerApplication);

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

        public int Count => throw new NotSupportedException();

        public IXAppearance this[string name] => m_RepoHelper.Get(name);

        internal SwDisplayStateDispatch DisplayState => m_Creator.Element;


        private readonly ElementCreator<SwDisplayStateDispatch> m_Creator;

        private readonly SwConfiguration m_OwnerConf;

        private RepositoryHelper<IXAppearance> m_RepoHelper;

        internal SwDisplayState(SwDisplayStateDispatch dispState, SwConfiguration ownerConf, SwDocument3D ownerDoc, SwApplication ownerApp) : base(dispState, ownerDoc, ownerApp)
        {
            m_OwnerConf = ownerConf;
            m_Creator = new ElementCreator<SwDisplayStateDispatch>(CreateDisplayState, dispState, dispState != null);

            m_RepoHelper = new RepositoryHelper<IXAppearance>(this, 
                TransactionFactory<IXAppearance>.Create(() => new SwAppearance(null, DisplayState, null, ownerDoc, ownerApp)));
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

        public bool TryGet(string name, out IXAppearance ent)
            => throw new NotSupportedException("Getting appearance by name is not supported, get appearance by objects instead");

        public void AddRange(IEnumerable<IXAppearance> ents, CancellationToken cancellationToken) => m_RepoHelper.AddRange(ents, cancellationToken);

        public void RemoveRange(IEnumerable<IXAppearance> ents, CancellationToken cancellationToken)
        {
            foreach (SwAppearance app in ents) 
            {
                app.Delete();
            }
        }

        public T PreCreate<T>() where T : IXAppearance
            => m_RepoHelper.PreCreate<T>();

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters)
            => m_RepoHelper.FilterDefault(this, filters, reverseOrder);

        public IEnumerator<IXAppearance> GetEnumerator() => throw new NotSupportedException();
    }
}
