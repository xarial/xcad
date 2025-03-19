//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
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
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Documents.Exceptions;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Geometry;
using Xarial.XCad.Inventor.Documents.Enums;
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

            m_Creator = new ElementCreator<bool?>(PerformSaveAs, null, false);
        }

        private bool? PerformSaveAs(CancellationToken cancellationToken)
        {
            DateTime? existingFileDate = null;

            if (System.IO.File.Exists(FilePath))
            {
                existingFileDate = System.IO.File.GetLastWriteTimeUtc(FilePath);
            }

            SaveAs(cancellationToken);

            if (System.IO.File.Exists(FilePath))
            {
                if (existingFileDate.HasValue)
                {
                    if (System.IO.File.GetLastWriteTimeUtc(FilePath) == existingFileDate)
                    {
                        throw new SaveDocumentFailedException(-1, "Failed to save as file (file is not overwritten)");
                    }
                }
            }
            else
            {
                throw new SaveDocumentFailedException(-1, "Failed to save as file (file does not exist)");
            }

            return true;
        }

        public void Commit(CancellationToken cancellationToken) => m_Creator.Create(cancellationToken);

        protected virtual void SaveAs(CancellationToken cancellationToken) => m_Doc.Document.SaveAs(FilePath, true);
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

        protected override void SaveAs(CancellationToken cancellationToken)
        {
            var context = m_Doc.OwnerApplication.Application.TransientObjects.CreateTranslationContext();

            var opts = m_Doc.OwnerApplication.Application.TransientObjects.CreateNameValueMap();

            SetSaveOptions(m_Translator, opts);

            if (m_Translator.HasSaveCopyAsOptions[m_Doc.Document, context, opts])
            {
                context.Type = IOMechanismEnum.kFileBrowseIOMechanism;

                var data = m_Doc.OwnerApplication.Application.TransientObjects.CreateDataMedium();
                data.FileName = FilePath;

                m_Translator.SaveCopyAs(m_Doc.Document, context, opts, data);
            }
            else
            {
                throw new SaveDocumentFailedException(-1, "Invalid options");
            }
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
        public AiDrawingSaveOperation(AiDrawing drw, string filePath) : base(drw, filePath)
        {
        }

        public IXSheet[] Sheets { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }

    internal class AiDrawingTranslatorSaveOperation : AiTranslatorSaveOperation, IXDrawingSaveOperation
    {
        public AiDrawingTranslatorSaveOperation(AiDrawing drw, TranslatorAddIn translator, string filePath) : base(drw, translator, filePath)
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

    internal class AiDxfDwgSaveOperation : AiDrawingTranslatorSaveOperation, IXDxfDwgSaveOperation
    {
        public AiDxfDwgSaveOperation(AiDrawing drw, TranslatorAddIn translator, string filePath) : base(drw, translator, filePath)
        {
        }

        public string LayersMapFilePath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool ExportHiddenLayers { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public SplineExportOptions_e SplineExportOptions { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        protected override void SetSaveOptions(TranslatorAddIn translator, NameValueMap opts)
        {
            opts.Value["Export_Acad_IniFile"] = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString() + ".ini");
        }
    }

    public interface IAiFlatPatternSaveOperation : IFlatPatternSaveOperation
    {
        FlatPatternAcadVersion_e Version { get; set; }
        double SplineTolerance { get; set; }
    }

    internal class AiFlatPatternSaveOperation : AiSaveOperation, IAiFlatPatternSaveOperation
    {
        public SplineExportOptions_e SplineExportOptions
        {
            get => m_Creator.CachedProperties.Get<SplineExportOptions_e>();
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
        
        public FlatPatternViewOptions_e ViewOptions
        {
            get => m_Creator.CachedProperties.Get<FlatPatternViewOptions_e>();
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

        public FlatPatternAcadVersion_e Version
        {
            get => m_Creator.CachedProperties.Get<FlatPatternAcadVersion_e>();
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

        public double SplineTolerance
        {
            get => m_Creator.CachedProperties.Get<double>();
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

        private readonly SheetMetalComponentDefinition m_SheetMetalCompDef;

        internal AiFlatPatternSaveOperation(AiDocument doc, SheetMetalComponentDefinition sheetMetalCompDef, string filePath) : base(doc, filePath)
        {
            m_SheetMetalCompDef = sheetMetalCompDef;

            Version = FlatPatternAcadVersion_e.Acad2004;
            SplineTolerance = 0.01;
        }

        protected override void SaveAs(CancellationToken cancellationToken)
        {
            var ext = System.IO.Path.GetExtension(FilePath).Trim('.').ToUpper();

            var vers = Version.ToString().Substring("Acad".Length);

            var simplifySplines = SplineExportOptions != SplineExportOptions_e.Splines;

            var format = new StringBuilder($"FLAT PATTERN {ext}?AcadVersion={vers}");
            format.Append($"&SimplifySplines={Convert.ToBoolean(simplifySplines)}");

            if (simplifySplines)
            {
                format.Append($"&SplineTolerance={SplineTolerance}");

                if (SplineExportOptions == SplineExportOptions_e.TangentArcs)
                {
                    format.Append("&SimplifyAsTangentArcs=True");
                }
            }

            if (!ViewOptions.HasFlag(FlatPatternViewOptions_e.BendLines)) 
            {
                format.Append("&BendUpLayer=IV_BEND&BendDownLayer=IV_BEND_DOWN");
                format.Append("&InvisibleLayers=IV_BEND;IV_BEND_DOWN");
            }

            if (ViewOptions.HasFlag(FlatPatternViewOptions_e.BendNotes))
            {
                throw new NotSupportedException("Bend notes option is not supported");
            }

            m_SheetMetalCompDef.DataIO.WriteDataToFile(format.ToString(), FilePath);
        }
    }
}
