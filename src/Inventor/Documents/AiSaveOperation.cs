//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Inventor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Exceptions;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Geometry;
using Xarial.XCad.Services;

namespace Xarial.XCad.Inventor.Documents
{
    internal abstract class AiSaveOperation : IXSaveOperation
    {
        public string FilePath { get; }

        public bool IsCommitted => m_Creator.IsCreated;

        protected readonly ElementCreator<bool?> m_Creator;

        protected readonly AiDocument m_Doc;

        internal AiSaveOperation(AiDocument doc, string filePath) 
        {
            m_Doc = doc;

            FilePath = filePath;

            m_Creator = new ElementCreator<bool?>(SaveAs, null, false);
        }

        protected virtual bool? SaveAs(CancellationToken cancellationToken)
        {
            m_Doc.Document.SaveAs(FilePath, true);
            return true;
        }

        public void Commit(CancellationToken cancellationToken) => m_Creator.Create(cancellationToken);
    }

    internal class AiDocument3DSaveOperation : AiSaveOperation, IXDocument3DSaveOperation
    {
        public AiDocument3DSaveOperation(AiDocument doc, string filePath) : base(doc, filePath)
        {
        }

        public IXBody[] Bodies { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }

    internal abstract class AiTranslatorSaveOperation : AiSaveOperation
    {
        private readonly TranslatorAddIn m_Translator;

        internal AiTranslatorSaveOperation(AiDocument doc, TranslatorAddIn translator, string filePath) : base(doc, filePath)
        {
            m_Translator = translator;
        }

        protected override bool? SaveAs(CancellationToken cancellationToken)
        {
            var context = m_Doc.OwnerApplication.Application.TransientObjects.CreateTranslationContext();

            var opts = m_Doc.OwnerApplication.Application.TransientObjects.CreateNameValueMap();

            SetSaveOptions(m_Translator, opts);

            if (m_Translator.HasSaveCopyAsOptions[m_Doc.Document, context, opts])
            {
                context.Type = IOMechanismEnum.kFileBrowseIOMechanism;

                var data = m_Doc.OwnerApplication.Application.TransientObjects.CreateDataMedium();
                data.FileName = FilePath;

                DateTime? existingFileDate = null;

                if (System.IO.File.Exists(FilePath))
                {
                    existingFileDate = System.IO.File.GetLastWriteTimeUtc(FilePath);
                }

                m_Translator.SaveCopyAs(m_Doc.Document, context, opts, data);

                if (System.IO.File.Exists(FilePath))
                {
                    if (existingFileDate.HasValue)
                    {
                        if (System.IO.File.GetLastWriteTimeUtc(FilePath) == existingFileDate)
                        {
                            throw new SaveDocumentFailedException(-1, "Failed to export file (file is not overwritten)");
                        }
                    }
                }
                else
                {
                    throw new SaveDocumentFailedException(-1, "Failed to export file (file does not exist)");
                }
            }
            else
            {
                throw new SaveDocumentFailedException(-1, "Invalid options");
            }

            return true;
        }

        protected virtual void SetSaveOptions(TranslatorAddIn translator, NameValueMap opts) 
        {
        }
    }

    internal class AiDocument3DTranslatorSaveOperation : AiTranslatorSaveOperation, IXDocument3DSaveOperation
    {
        public AiDocument3DTranslatorSaveOperation(AiDocument doc, TranslatorAddIn translator, string filePath) : base(doc, translator, filePath)
        {
        }

        public IXBody[] Bodies { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }

    internal class AiDrawingSaveOperation : AiSaveOperation, IXDrawingSaveOperation
    {
        public AiDrawingSaveOperation(AiDocument doc, string filePath) : base(doc, filePath)
        {
        }

        public IXSheet[] Sheets { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }

    internal class AiDrawingTranslatorSaveOperation : AiTranslatorSaveOperation, IXDrawingSaveOperation
    {
        public AiDrawingTranslatorSaveOperation(AiDocument doc, TranslatorAddIn translator, string filePath) : base(doc, translator, filePath)
        {
        }

        public IXSheet[] Sheets { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }

    internal class AiStepSaveOperation : AiDocument3DTranslatorSaveOperation, IXStepSaveOperation
    {
        internal AiStepSaveOperation(AiDocument doc, TranslatorAddIn translator, string filePath) : base(doc, translator, filePath)
        {
        }

        public StepFormat_e Format
        {
            get => m_Creator.CachedProperties.Get<StepFormat_e>();
            set
            {
                if (!IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value);
                }
                else
                {
                    throw new CommitedElementReadOnlyParameterException();
                }
            }
        }

        protected override void SetSaveOptions(TranslatorAddIn translator, NameValueMap opts)
        {
            int protocolType;

            switch (Format)
            {
                case StepFormat_e.Ap203:
                    protocolType = 2;
                    break;

                case StepFormat_e.Ap214:
                    protocolType = 3;
                    break;

                case StepFormat_e.Ap242:
                    protocolType = 5;
                    break;

                default:
                    throw new NotSupportedException();
            }

            opts.Value["ApplicationProtocolType"] = protocolType;
        }
    }
}
