//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xarial.XCad.Data;
using Xarial.XCad.Documents;
using Xarial.XCad.SwDocumentManager.Documents;
using Xarial.XCad.SwDocumentManager.Features;
using Xarial.XCad.Toolkit.Data;

namespace Xarial.XCad.SwDocumentManager
{
    public interface ISwDmObject : IXObject
    {
        object Dispatch { get; }
    }

    internal class SwDmObject : ISwDmObject
    {
        // #region NotSuppoted

        public virtual bool IsAlive => throw new NotSupportedException();

        public virtual void Serialize(Stream stream)
            => throw new NotSupportedException();

        // #endregion

        IXApplication IXObject.OwnerApplication => OwnerApplication;
        IXDocument IXObject.OwnerDocument => OwnerDocument;

        internal SwDmApplication OwnerApplication { get; }
        internal SwDmDocument OwnerDocument { get; }

        public ITagsManager Tags => m_TagsLazy.Value;

        private readonly Lazy<ITagsManager> m_TagsLazy;

        public SwDmObject(object disp, SwDmApplication ownerApp, SwDmDocument ownerDoc)
        {
            Dispatch = disp;

            OwnerApplication = ownerApp;
            OwnerDocument = ownerDoc;

            m_TagsLazy = new Lazy<ITagsManager>(() => new LocalTagsManager());
        }

        public virtual object Dispatch { get; }

        public virtual bool Equals(IXObject other)
        {
            if (other is ISwDmObject)
            {
                return (other as ISwDmObject).Dispatch == Dispatch;
            }
            else
            {
                return false;
            }
        }
    }

    public static class SwDmObjectFactory
    {
        internal static TObj FromDispatch<TObj>(object disp, SwDmDocument doc)
            where TObj : ISwDmObject
        {
            return (TObj)FromDispatch(disp, doc, doc.OwnerApplication);
        }

        private static ISwDmObject FromDispatch(object disp, SwDmDocument doc, SwDmApplication app)
        {
            switch (disp)
            {
                case ISwDMConfiguration conf:
                    switch (doc)
                    {
                        case SwDmAssembly assm:
                            return new SwDmAssemblyConfiguration(conf, assm);

                        case SwDmPart part:
                            return new SwDmPartConfiguration(conf, part);

                        default:
                            throw new NotSupportedException("This document type is not supported for configuration");
                    }

                case ISwDMCutListItem cutList:
                    return new SwDmCutListItem((ISwDMCutListItem2)cutList, (SwDmPart)doc);

                case ISwDMComponent comp:
                    var ext = Path.GetExtension(((ISwDMComponent6)comp).PathName);
                    switch (ext.ToLower())
                    {
                        case ".sldprt":
                            return new SwDmPartComponent((SwDmAssembly)doc, comp);

                        case ".sldasm":
                            return new SwDmAssemblyComponent((SwDmAssembly)doc, comp);

                        default:
                            throw new NotSupportedException();
                    }

                case ISwDMSheet sheet:
                    return new SwDmSheet(sheet, (SwDmDrawing)doc);

                case ISwDMView view:
                    return new SwDmDrawingView(view, (SwDmDrawing)doc);

                default:
                    return new SwDmObject(disp, app, doc);
            }
        }
    }
}
