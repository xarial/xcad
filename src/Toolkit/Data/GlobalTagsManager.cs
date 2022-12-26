//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Xarial.XCad.Data;
using Xarial.XCad.Documents;
using Xarial.XCad.Services;

namespace Xarial.XCad.Toolkit.Data
{
    /// <summary>
    /// Golbal tags registry used in <see cref="GlobalTagsManager"/>
    /// </summary>
    /// <remarks>Store a singleton instance of this service and pass to all instances of <see cref="GlobalTagsManager"/></remarks>
    public class GlobalTagsRegistry : IDisposable
    {
        private class DocumentObjectTags : Dictionary<IXObject, LocalTagsManager>
        {
            internal DocumentObjectTags() : base(new XObjectEqualityComparer<IXObject>()) 
            {
            }
        }

        private readonly Dictionary<IXDocument, DocumentObjectTags> m_Tags;

        public GlobalTagsRegistry()
        {
            m_Tags = new Dictionary<IXDocument, DocumentObjectTags>(new XObjectEqualityComparer<IXDocument>());
        }

        public int Count => m_Tags.Count;

        public bool IsEmpty(IXObject owner) 
        {
            if (TryGetLocalTagsManager(owner, out var tagsMgr, out var docsTag))
            {
                return tagsMgr.IsEmpty;
            }
            else
            {
                return true;
            }
        }

        public bool Contains(IXObject owner, string name)
        {
            if (TryGetLocalTagsManager(owner, out var tagsMgr, out _))
            {
                return tagsMgr.Contains(name);
            }

            return false;
        }

        public T Get<T>(IXObject owner, string name)
        {
            if (TryGetLocalTagsManager(owner, out var tagsMgr, out _))
            {
                return tagsMgr.Get<T>(name);
            }
            else 
            {
                throw new KeyNotFoundException("Specified object does not have registered tags");
            }
        }

        public T Pop<T>(IXObject owner, string name)
        {
            if (!TryGetLocalTagsManager(owner, out var tagsMgr, out var docTags)) 
            {
                throw new KeyNotFoundException("Specified object does not have registered tags");
            }

            var val = tagsMgr.Pop<T>(name);

            if (tagsMgr.IsEmpty) 
            {
                docTags.Remove(owner);
            }

            if (!docTags.Any())
            {
                var ownerDoc = owner.OwnerDocument;

                if (ownerDoc != null)
                {
                    ownerDoc.Destroyed -= OnOwnerDocumentDestroyed;
                }

                m_Tags.Remove(ownerDoc);
            }

            return val;
        }

        public void Put<T>(IXObject owner, string name, T value)
        {
            TryGetLocalTagsManager(owner, out var tagsMgr, out var docTags);

            if (docTags == null) 
            {
                docTags = new DocumentObjectTags();

                var ownerDoc = owner.OwnerDocument;
                
                if (ownerDoc != null)
                {
                    ownerDoc.Destroyed += OnOwnerDocumentDestroyed;
                }

                m_Tags.Add(ownerDoc, docTags);
            }

            if (tagsMgr == null) 
            {
                tagsMgr = new LocalTagsManager();
                docTags.Add(owner, tagsMgr);
            }

            tagsMgr.Put<T>(name, value);
        }

        private void OnOwnerDocumentDestroyed(IXDocument doc)
        {
            if (m_Tags.TryGetValue(doc, out var docTags))
            {
                doc.Destroyed -= OnOwnerDocumentDestroyed;
                docTags.Clear();
                m_Tags.Remove(doc);
            }
            else 
            {
                System.Diagnostics.Debug.Assert(false, "Document without tags should have been disconnected before");
            }
        }

        private bool TryGetLocalTagsManager(IXObject owner, out LocalTagsManager tagsMgr, out DocumentObjectTags docTags)
        {
            if (m_Tags.TryGetValue(owner.OwnerDocument, out docTags))
            {
                if (docTags.TryGetValue(owner, out tagsMgr))
                {
                    return true;
                }
            }

            tagsMgr = null;
            return false;
        }

        public void Dispose()
        {
            foreach (var tag in m_Tags) 
            {
                var ownerDoc = tag.Key;

                if (ownerDoc != null)
                {
                    ownerDoc.Destroyed -= OnOwnerDocumentDestroyed;
                }

                tag.Value.Clear();
            }

            m_Tags.Clear();
        }
    }

    /// <summary>
    /// Manages tags with global registry
    /// </summary>
    /// <remarks>Use this service instead of <see cref="LocalTagsManager"/> when instance of <see cref="IXObject"/>
    /// is not guaranteed to be the same for the native (underline) object</remarks>
    public class GlobalTagsManager : ITagsManager
    {
        private readonly IXObject m_Owner;
        private readonly GlobalTagsRegistry m_Tags;

        public GlobalTagsManager(IXObject owner, GlobalTagsRegistry globalTagsReg)
        {
            m_Owner = owner;
            m_Tags = globalTagsReg;
        }

        public bool IsEmpty => m_Tags.IsEmpty(m_Owner);
        public bool Contains(string name) => m_Tags.Contains(m_Owner, name);
        public T Get<T>(string name) => m_Tags.Get<T>(m_Owner, name);
        public T Pop<T>(string name) => m_Tags.Pop<T>(m_Owner, name);
        public void Put<T>(string name, T value) => m_Tags.Put<T>(m_Owner, name, value);
    }
}
