//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using static System.Windows.Forms.AxHost;

namespace Xarial.XCad.SolidWorks.Documents
{
    internal class SwDocumentDependencies : IXDocumentDependencies
    {
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private readonly SwDocument m_Doc;
        private readonly IXLogger m_Logger;

        public IXDocument OwnerDocument => m_Doc;

        internal SwDocumentDependencies(SwDocument doc, IXLogger logger) 
        {
            m_Doc = doc;
            m_Logger = logger;
        }

        public IEnumerator<IXDocument3D> GetEnumerator() 
            => IterateDependencies().GetEnumerator();

        public void Replace(IXDocument3D source, IXDocument3D target)
        {
            if (!m_Doc.OwnerApplication.Sw.ReplaceReferencedDocument(m_Doc.Path, source.Path, target.Path)) 
            {
                throw new Exception("Failed to replace referenced document");
            }
        }

        private IEnumerable<SwDocument3D> IterateDependencies()
        {
            string[] depsData;

            if (m_Doc.IsCommitted && !m_Doc.Model.IsOpenedViewOnly())
            {
                depsData = m_Doc.Model.Extension.GetDependencies(false, true, false, true, true) as string[];
            }
            else
            {
                if (!string.IsNullOrEmpty(m_Doc.Path))
                {
                    depsData = m_Doc.OwnerApplication.Sw.GetDocumentDependencies2(m_Doc.Path, false, true, false) as string[];
                }
                else
                {
                    throw new Exception("Dependencies can only be extracted for the document with specified path");
                }
            }

            if (depsData?.Any() == true)
            {
                for (int i = 1; i < depsData.Length; i += 2)
                {
                    SwDocument3D refDoc;
                    var path = depsData[i];

                    path = ResolvePathIf3DInterconnect(path);

                    if (!((SwDocumentCollection)m_Doc.OwnerApplication.Documents).TryFindExistingDocumentByPath(path, out SwDocument existingRefDoc))
                    {
                        try
                        {
                            refDoc = (SwDocument3D)((SwDocumentCollection)m_Doc.OwnerApplication.Documents).PreCreateFromPath(path);
                        }
                        catch (Exception ex)//for 3D interconnect files the PreCreateFromPath can fail
                        {
                            m_Logger.Log(ex);
                            refDoc = m_Doc.OwnerApplication.Documents.PreCreate<SwDocument3D>();
                            refDoc.Path = path;
                        }

                        if (m_Doc.State.HasFlag(DocumentState_e.ReadOnly))
                        {
                            refDoc.State = DocumentState_e.ReadOnly;
                        }
                    }
                    else
                    {
                        refDoc = (SwDocument3D)existingRefDoc;
                    }

                    yield return refDoc;
                }
            }
        }

        private string ResolvePathIf3DInterconnect(string path)
        {
            if (path.Contains("|"))
            {
                path = path.Split('|').First();
            }

            return path;
        }
    }
}
