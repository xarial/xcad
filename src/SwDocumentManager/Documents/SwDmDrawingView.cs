//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.SwDocumentManager.Documents
{
    public interface ISwDmDrawingView : IXDrawingView, ISwDmObject
    {
        ISwDMView DrawingView { get; }
    }

    internal class SwDmDrawingView : SwDmObject, ISwDmDrawingView
    {
        #region Not Supported

        public Point Location { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public void Commit(CancellationToken cancellationToken)
            => throw new NotSupportedException();
        public void Select(bool append)
            => throw new NotSupportedException();
        TSelObject IXObjectContainer.ConvertObject<TSelObject>(TSelObject obj)
            => throw new NotSupportedException();
        public bool IsSelected => throw new NotSupportedException();

        #endregion

        public ISwDMView DrawingView { get; }

        public string Name 
        {
            get => DrawingView.Name;
            set => throw new NotSupportedException(); 
        }

        public IXDocument3D Document 
        {
            get 
            {
                if (m_CachedDocument == null || !m_CachedDocument.IsAlive)
                {
                    var fileName = DrawingView.ReferencedDocument;

                    if (!string.IsNullOrEmpty(fileName) && fileName != "-")
                    {
                        var refDoc = m_Drw.Dependencies.First(d => string.Equals(Path.GetFileName(d.Path), fileName, StringComparison.CurrentCultureIgnoreCase));

                        if (!refDoc.IsCommitted) 
                        {
                            var isReadOnly = m_Drw.State.HasFlag(DocumentState_e.ReadOnly);

                            if (isReadOnly) 
                            {
                                refDoc.State = DocumentState_e.ReadOnly;
                            }
                        }

                        m_CachedDocument = refDoc;
                    }
                }

                return m_CachedDocument;
            }
        }

        public IXConfiguration ReferencedConfiguration 
        {
            get 
            {
                var confName = DrawingView.ReferencedConfiguration;

                if (!string.IsNullOrEmpty(confName) && confName != "-")
                {
                    if (!Document.IsCommitted)
                    {
                        Document.Commit();
                    }

                    return Document.Configurations.FirstOrDefault(c => string.Equals(c.Name, confName, StringComparison.CurrentCultureIgnoreCase));
                }
                else 
                {
                    return null;
                }
            }
        }

        public bool IsCommitted => true;

        private readonly SwDmDrawing m_Drw;
        private IXDocument3D m_CachedDocument;

        internal SwDmDrawingView(ISwDMView view, SwDmDrawing drw) : base(view)
        {
            DrawingView = view;
            m_Drw = drw;
        }
    }
}
