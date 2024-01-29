//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Structures;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Documents
{
    internal class SwLayersCollection : IXLayerRepository
    {
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private readonly SwDocument m_Doc;
        private readonly SwApplication m_App;

        internal SwLayersCollection(SwDocument doc, SwApplication app) 
        {
            m_Doc = doc;
            m_App = app;
        }

        public IXLayer this[string name] => RepositoryHelper.Get(this, name);

        public int Count => m_Doc.Model.IGetLayerManager().GetCount();

        public IXLayer Active 
        {
            get
            {
                var currentLayerName = m_Doc.Model.IGetLayerManager().GetCurrentLayer();

                if (currentLayerName == "-None-") 
                {
                    currentLayerName = "";
                }

                if (!string.IsNullOrEmpty(currentLayerName))
                {
                    return this[currentLayerName];
                }
                else 
                {
                    return null;
                }
            }
            set
            {
                if (!Convert.ToBoolean(m_Doc.Model.IGetLayerManager().SetCurrentLayer(value?.Name ?? "")))
                {
                    throw new Exception("Failed to set current layer");
                }
            }
        }

        public void AddRange(IEnumerable<IXLayer> ents, CancellationToken cancellationToken) 
            => RepositoryHelper.AddRange(ents, cancellationToken);

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters) 
            => RepositoryHelper.FilterDefault(this, filters, reverseOrder);

        public IEnumerator<IXLayer> GetEnumerator()
        {
            var layerNames = (string[])m_Doc.Model.IGetLayerManager().GetLayerList();

            if (layerNames != null) 
            {
                foreach (var layerName in layerNames) 
                {
                    yield return this[layerName];
                }
            }
        }

        public T PreCreate<T>() where T : IXLayer
            => RepositoryHelper.PreCreate<IXLayer, T>(this,
                () => new SwLayer(null, m_Doc, m_App));

        public void RemoveRange(IEnumerable<IXLayer> ents, CancellationToken cancellationToken)
        {
            foreach (var layer in ents) 
            {
                if (!m_Doc.Model.IGetLayerManager().DeleteLayer(layer.Name)) 
                {
                    throw new Exception($"Failed to delete layer '{layer.Name}'");
                }
            }
        }

        public bool TryGet(string name, out IXLayer ent)
        {
            var layer = m_Doc.Model.IGetLayerManager().IGetLayer(name);

            if (layer != null)
            {
                ent = m_Doc.CreateObjectFromDispatch<ISwLayer>(layer);
                return true;
            }
            else 
            {
                ent = null;
                return false;
            }
        }
    }
}
