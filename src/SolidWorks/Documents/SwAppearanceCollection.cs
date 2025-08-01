//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Threading;
using Xarial.XCad.Documents;
using Xarial.XCad.Toolkit.Utils;
using System.Collections;
using Xarial.XCad.Base;

namespace Xarial.XCad.SolidWorks.Documents
{
    internal class SwAppearanceCollection : IXAppearanceRepository 
    {
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private RepositoryHelper<IXAppearance> m_RepoHelper;

        private readonly SwDisplayState m_DispState;

        private readonly SwConfiguration m_OwnerConf;
        private readonly SwDocument3D m_OwnerDoc;
        private readonly SwApplication m_OwnerApp;

        internal SwAppearanceCollection(SwDisplayState dispState, SwConfiguration ownerConf, SwDocument3D ownerDoc, SwApplication ownerApp) 
        {
            m_DispState = dispState;

            m_OwnerConf = ownerConf;

            m_OwnerDoc = ownerDoc;

            m_OwnerApp = ownerApp;

            m_RepoHelper = new RepositoryHelper<IXAppearance>(this,
                TransactionFactory<IXAppearance>.Create(() => new SwAppearance(null, m_DispState.DisplayState, null, ownerConf, ownerDoc, ownerApp)),
                TransactionFactory<IXAppearance>.Create(() => new SwRenderMaterial(null, m_DispState.DisplayState, ownerDoc, ownerApp)));
        }

        public int Count => m_OwnerConf.OwnerDocument.Model.Extension.GetRenderMaterialsCount2((int)swDisplayStateOpts_e.swSpecifyDisplayState, new string[] { m_DispState.DisplayState.Name });

        public IXAppearance this[string name] => m_RepoHelper.Get(name);

        public IXAppearance this[IHasColor[] objs] => SwAppearance.FromObjects(m_DispState.DisplayState, objs, AppearanceLevel_e.Component, m_OwnerConf, m_OwnerApp);

        public bool TryGet(string name, out IXAppearance ent)
            => throw new NotSupportedException("Getting appearance by name is not supported, get appearance by objects instead");

        public void AddRange(IEnumerable<IXAppearance> ents, CancellationToken cancellationToken) => m_RepoHelper.AddRange(ents, cancellationToken);

        public void RemoveRange(IEnumerable<IXAppearance> ents, CancellationToken cancellationToken)
        {
            foreach (ISwAppearanceBase app in ents)
            {
                app.Delete();
            }
        }

        public T PreCreate<T>() where T : IXAppearance
            => m_RepoHelper.PreCreate<T>();

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters)
            => m_RepoHelper.FilterDefault(this, filters, reverseOrder);

        public IEnumerator<IXAppearance> GetEnumerator()
        {
            var renderMaterials = (object[])m_OwnerConf.OwnerDocument.Model.Extension.GetRenderMaterials2((int)swDisplayStateOpts_e.swSpecifyDisplayState, new string[] { m_DispState.DisplayState.Name });

            if (renderMaterials != null)
            {
                foreach (IRenderMaterial renderMaterial in renderMaterials)
                {
                    yield return new SwRenderMaterial(renderMaterial, m_DispState.DisplayState, m_OwnerDoc, m_OwnerApp);
                }
            }
        }
    }
}
