using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Exceptions;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Enums;

namespace Xarial.XCad.SolidWorks.Documents
{
    internal class SwSaveOperation : IXSaveOperation
    {
        internal static string ParseSaveError(swFileSaveError_e err)
        {
            var errors = new List<string>();

            if (err.HasFlag(swFileSaveError_e.swFileLockError))
            {
                errors.Add("File lock error");
            }

            if (err.HasFlag(swFileSaveError_e.swFileNameContainsAtSign))
            {
                errors.Add("File name cannot contain the at symbol(@)");
            }

            if (err.HasFlag(swFileSaveError_e.swFileNameEmpty))
            {
                errors.Add("File name cannot be empty");
            }

            if (err.HasFlag(swFileSaveError_e.swFileSaveAsBadEDrawingsVersion))
            {
                errors.Add("Bad eDrawings data");
            }

            if (err.HasFlag(swFileSaveError_e.swFileSaveAsDoNotOverwrite))
            {
                errors.Add("Cannot overwrite an existing file");
            }

            if (err.HasFlag(swFileSaveError_e.swFileSaveAsInvalidFileExtension))
            {
                errors.Add("File name extension does not match the SOLIDWORKS document type");
            }

            if (err.HasFlag(swFileSaveError_e.swFileSaveAsNameExceedsMaxPathLength))
            {
                errors.Add("File name cannot exceed 255 characters");
            }

            if (err.HasFlag(swFileSaveError_e.swFileSaveAsNotSupported))
            {
                errors.Add("Save As operation is not supported in this environment");
            }

            if (err.HasFlag(swFileSaveError_e.swFileSaveFormatNotAvailable))
            {
                errors.Add("Save As file type is not valid");
            }

            if (err.HasFlag(swFileSaveError_e.swFileSaveRequiresSavingReferences))
            {
                errors.Add("Saving an assembly with renamed components requires saving the references");
            }

            if (err.HasFlag(swFileSaveError_e.swGenericSaveError))
            {
                errors.Add("Generic error");
            }

            if (err.HasFlag(swFileSaveError_e.swReadOnlySaveError))
            {
                errors.Add("File is readonly");
            }

            if (errors.Count == 0)
            {
                errors.Add("Unknown error");
            }

            return string.Join("; ", errors);
        }

        public string FilePath { get; }

        public bool IsCommitted => m_Creator.IsCreated;

        protected readonly ElementCreator<bool?> m_Creator;

        protected readonly SwDocument m_Doc;

        internal SwSaveOperation(SwDocument doc, string filePath)
        {
            m_Doc = doc;
            FilePath = filePath;

            m_Creator = new ElementCreator<bool?>(SaveAs, null, false);
        }

        private bool? SaveAs(CancellationToken cancellationToken)
        {
            int errs = -1;
            int warns = -1;

            bool res;

            SetSaveOptions(out var expData);

            try
            {
                if (m_Doc.OwnerApplication.IsVersionNewerOrEqual(SwVersion_e.Sw2019, 1))
                {
                    res = m_Doc.Model.Extension.SaveAs2(FilePath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion,
                        (int)swSaveAsOptions_e.swSaveAsOptions_Silent, expData, "", false, ref errs, ref warns);
                }
                else
                {
                    res = m_Doc.Model.Extension.SaveAs(FilePath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion,
                        (int)swSaveAsOptions_e.swSaveAsOptions_Silent, expData, ref errs, ref warns);
                }

                if (!res)
                {
                    throw new SaveDocumentFailedException(errs, ParseSaveError((swFileSaveError_e)errs));
                }

                return res;
            }
            finally
            {
                RestoreSaveOptions();
            }
        }

        public void Commit(CancellationToken cancellationToken) => m_Creator.Create(cancellationToken);

        protected virtual void SetSaveOptions(out object exportData)
        {
            exportData = null;
        }

        protected virtual void RestoreSaveOptions()
        {
        }
    }

    internal class SwStepSaveOperation : SwSaveOperation, IXStepSaveOperation
    {
        private int m_OriginalFormat;

        internal SwStepSaveOperation(SwDocument doc, string filePath) : base(doc, filePath)
        {
            var format = m_Doc.OwnerApplication.Sw.GetUserPreferenceIntegerValue((int)swUserPreferenceIntegerValue_e.swStepAP);

            StepFormat_e curFormat;

            switch (format) 
            {
                case 203:
                    curFormat = StepFormat_e.Ap203;
                    break;

                case 214:
                    curFormat = StepFormat_e.Ap214;
                    break;

                default:
                    curFormat = StepFormat_e.Ap203;
                    break;
            }

            Format = curFormat;
        }

        protected override void SetSaveOptions(out object exportData)
        {
            m_OriginalFormat = m_Doc.OwnerApplication.Sw.GetUserPreferenceIntegerValue((int)swUserPreferenceIntegerValue_e.swStepAP);

            exportData = null;

            int format;

            switch (Format)
            {
                case StepFormat_e.Ap203:
                    format = 203;
                    break;

                case StepFormat_e.Ap214:
                    format = 214;
                    break;

                default:
                    throw new NotSupportedException();
            }

            if (!m_Doc.OwnerApplication.Sw.SetUserPreferenceIntegerValue((int)swUserPreferenceIntegerValue_e.swStepAP, format))
            {
                throw new Exception("Failed to set the option");
            }
        }

        protected override void RestoreSaveOptions()
        {
            m_Doc.OwnerApplication.Sw.SetUserPreferenceIntegerValue((int)swUserPreferenceIntegerValue_e.swStepAP, m_OriginalFormat);
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
    }

    internal class SwPdfSaveOperation : SwSaveOperation, IXPdfSaveOperation
    {
        internal SwPdfSaveOperation(SwDocument doc, string filePath) : base(doc, filePath)
        {
        }

        protected override void SetSaveOptions(out object exportData)
        {
            var pdfExpData = (IExportPdfData)m_Doc.OwnerApplication.Sw.GetExportFileData((int)swExportDataFileType_e.swExportPdfData);
            pdfExpData.ExportAs3D = Pdf3D;
            pdfExpData.ViewPdfAfterSaving = false;
            exportData = pdfExpData;
        }

        public bool Pdf3D 
        {
            get => m_Creator.CachedProperties.Get<bool>();
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
    }

    internal class SwDxfDwgSaveOperation : SwSaveOperation, IXDxfDwgSaveOperation
    {
        private bool m_OrigDxfMapping;
        private string m_OrigDxfMappingFiles;
        private int m_OrigDxfMappingFileIndex;

        internal SwDxfDwgSaveOperation(SwDocument doc, string filePath) : base(doc, filePath)
        {
            var mapFilePath = "";

            if (m_Doc.OwnerApplication.Sw.GetUserPreferenceToggle((int)swUserPreferenceToggle_e.swDxfMapping)) 
            {
                var mapFiles = (m_Doc.OwnerApplication.Sw.GetUserPreferenceStringListValue((int)swUserPreferenceStringListValue_e.swDxfMappingFiles) ?? "")
                    .Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                var index = m_Doc.OwnerApplication.Sw.GetUserPreferenceIntegerValue((int)swUserPreferenceIntegerValue_e.swDxfMappingFileIndex);

                if (index < mapFiles.Length) 
                {
                    mapFilePath = mapFiles[index];
                }
            }

            LayersMapFilePath = mapFilePath;
        }

        protected override void SetSaveOptions(out object exportData)
        {
            exportData = null;

            m_OrigDxfMapping = m_Doc.OwnerApplication.Sw.GetUserPreferenceToggle((int)swUserPreferenceToggle_e.swDxfMapping);
            m_OrigDxfMappingFiles = m_Doc.OwnerApplication.Sw.GetUserPreferenceStringListValue((int)swUserPreferenceStringListValue_e.swDxfMappingFiles);
            m_OrigDxfMappingFileIndex = m_Doc.OwnerApplication.Sw.GetUserPreferenceIntegerValue((int)swUserPreferenceIntegerValue_e.swDxfMappingFileIndex);

            m_Doc.OwnerApplication.Sw.SetUserPreferenceToggle((int)swUserPreferenceToggle_e.swDxfMapping, !string.IsNullOrEmpty(LayersMapFilePath));

            if (!string.IsNullOrEmpty(LayersMapFilePath)) 
            {
                m_Doc.OwnerApplication.Sw.SetUserPreferenceStringListValue((int)swUserPreferenceStringListValue_e.swDxfMappingFiles, LayersMapFilePath);

                m_Doc.OwnerApplication.Sw.SetUserPreferenceIntegerValue((int)swUserPreferenceIntegerValue_e.swDxfMappingFileIndex, 0);
            }
        }

        protected override void RestoreSaveOptions()
        {
             m_Doc.OwnerApplication.Sw.SetUserPreferenceToggle((int)swUserPreferenceToggle_e.swDxfMapping, m_OrigDxfMapping);
             m_Doc.OwnerApplication.Sw.SetUserPreferenceStringListValue((int)swUserPreferenceStringListValue_e.swDxfMappingFiles, m_OrigDxfMappingFiles);
             m_Doc.OwnerApplication.Sw.SetUserPreferenceIntegerValue((int)swUserPreferenceIntegerValue_e.swDxfMappingFileIndex, m_OrigDxfMappingFileIndex);
        }

        public string LayersMapFilePath
        {
            get => m_Creator.CachedProperties.Get<string>();
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
    }
}
