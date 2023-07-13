//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Markup;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Exceptions;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Geometry;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Enums;
using Xarial.XCad.SolidWorks.Utils;

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
                if (m_Doc.OwnerApplication.IsVersionNewerOrEqual(SwVersion_e.Sw2020, 2))
                {
                    res = m_Doc.Model.Extension.SaveAs3(FilePath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion,
                        (int)swSaveAsOptions_e.swSaveAsOptions_Silent, expData, null, ref errs, ref warns);
                }
                else if (m_Doc.OwnerApplication.IsVersionNewerOrEqual(SwVersion_e.Sw2019, 1))
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

    internal class SwDocument3DSaveOperation : SwSaveOperation, IXDocument3DSaveOperation
    {
        internal SwDocument3DSaveOperation(SwDocument3D doc, string filePath) : base(doc, filePath)
        {
        }

        protected override void SetSaveOptions(out object exportData)
        {
            base.SetSaveOptions(out exportData);

            if (Bodies?.Any() == true)
            {
                m_Doc.Selections.ReplaceRange(Bodies);
            }
            else 
            {
                m_Doc.Selections.Clear();
            }
        }

        public IXBody[] Bodies
        {
            get => m_Creator.CachedProperties.Get<IXBody[]>();
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

    internal class SwDrawingSaveOperation : SwSaveOperation, IXDrawingSaveOperation
    {
        public IXSheet[] Sheets
        {
            get => m_Creator.CachedProperties.Get<IXSheet[]>();
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

        internal SwDrawingSaveOperation(SwDrawing drw, string filePath) : base(drw, filePath)
        {
        }
    }

    internal class SwStepSaveOperation : SwDocument3DSaveOperation, IXStepSaveOperation
    {
        private int m_OriginalFormat;

        internal SwStepSaveOperation(SwDocument3D doc, string filePath) : base(doc, filePath)
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
            base.SetSaveOptions(out exportData);

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

    internal class SwDocument3DPdfSaveOperation : SwDocument3DSaveOperation, IXDocument3DPdfSaveOperation
    {
        internal SwDocument3DPdfSaveOperation(SwDocument3D doc, string filePath) : base(doc, filePath)
        {
        }

        protected override void SetSaveOptions(out object exportData)
        {
            base.SetSaveOptions(out exportData);

            var pdfExpData = (IExportPdfData)m_Doc.OwnerApplication.Sw.GetExportFileData((int)swExportDataFileType_e.swExportPdfData);
            pdfExpData.ViewPdfAfterSaving = false;
            pdfExpData.ExportAs3D = Pdf3D;

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

    internal class SwDrawingPdfSaveOperation : SwDrawingSaveOperation, IXDrawingPdfSaveOperation
    {
        internal SwDrawingPdfSaveOperation(SwDrawing doc, string filePath) : base(doc, filePath)
        {
        }

        protected override void SetSaveOptions(out object exportData)
        {
            base.SetSaveOptions(out exportData);

            var pdfExpData = (IExportPdfData)m_Doc.OwnerApplication.Sw.GetExportFileData((int)swExportDataFileType_e.swExportPdfData);
            pdfExpData.ViewPdfAfterSaving = false;

            var sheets = Sheets;

            pdfExpData.SetSheets(sheets?.Any() == true
                ? (int)swExportDataSheetsToExport_e.swExportData_ExportSpecifiedSheets
                : (int)swExportDataSheetsToExport_e.swExportData_ExportAllSheets,
                sheets?.Select(s => s.Name)?.ToArray());

            exportData = pdfExpData;
        }
    }

    internal class SwDxfDwgSaveOperation : SwDrawingSaveOperation, IXDxfDwgSaveOperation
    {
        private bool m_OrigDxfMapping;
        private string m_OrigDxfMappingFiles;
        private int m_OrigDxfMappingFileIndex;
        private int m_OrigDxfMultiSheetOption;

        private readonly SwDrawing m_Draw;
        private SheetActivator m_SheetActivator;

        internal SwDxfDwgSaveOperation(SwDrawing doc, string filePath) : base(doc, filePath)
        {
            m_Draw = doc;

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
            base.SetSaveOptions(out exportData);

            exportData = null;

            m_OrigDxfMapping = m_Doc.OwnerApplication.Sw.GetUserPreferenceToggle((int)swUserPreferenceToggle_e.swDxfMapping);
            m_OrigDxfMappingFiles = m_Doc.OwnerApplication.Sw.GetUserPreferenceStringListValue((int)swUserPreferenceStringListValue_e.swDxfMappingFiles);
            m_OrigDxfMappingFileIndex = m_Doc.OwnerApplication.Sw.GetUserPreferenceIntegerValue((int)swUserPreferenceIntegerValue_e.swDxfMappingFileIndex);
            m_OrigDxfMultiSheetOption = m_Doc.OwnerApplication.Sw.GetUserPreferenceIntegerValue((int)swUserPreferenceIntegerValue_e.swDxfMultiSheetOption);

            m_Doc.OwnerApplication.Sw.SetUserPreferenceToggle((int)swUserPreferenceToggle_e.swDxfMapping, !string.IsNullOrEmpty(LayersMapFilePath));

            if (!string.IsNullOrEmpty(LayersMapFilePath)) 
            {
                m_Doc.OwnerApplication.Sw.SetUserPreferenceStringListValue((int)swUserPreferenceStringListValue_e.swDxfMappingFiles, LayersMapFilePath);

                m_Doc.OwnerApplication.Sw.SetUserPreferenceIntegerValue((int)swUserPreferenceIntegerValue_e.swDxfMappingFileIndex, 0);
            }

            if (Sheets?.Length == 1)
            {
                m_SheetActivator = new SheetActivator((SwSheet)Sheets.First());

                m_Doc.OwnerApplication.Sw.SetUserPreferenceIntegerValue(
                    (int)swUserPreferenceIntegerValue_e.swDxfMultiSheetOption, (int)swDxfMultisheet_e.swDxfActiveSheetOnly);
            }
            else if (Sheets == null || m_Draw.Sheets.OrderBy(s => s.Name).SequenceEqual(Sheets.OrderBy(s => s.Name), new XObjectEqualityComparer<IXSheet>()))
            {
                m_Doc.OwnerApplication.Sw.SetUserPreferenceIntegerValue(
                    (int)swUserPreferenceIntegerValue_e.swDxfMultiSheetOption, (int)swDxfMultisheet_e.swDxfMultiSheet);
            }
            else 
            {
                throw new NotSupportedException("Only single or all sheets can be exported to DXF/DWG");
            }
        }

        protected override void RestoreSaveOptions()
        {
            m_Doc.OwnerApplication.Sw.SetUserPreferenceToggle((int)swUserPreferenceToggle_e.swDxfMapping, m_OrigDxfMapping);
            m_Doc.OwnerApplication.Sw.SetUserPreferenceStringListValue((int)swUserPreferenceStringListValue_e.swDxfMappingFiles, m_OrigDxfMappingFiles);
            m_Doc.OwnerApplication.Sw.SetUserPreferenceIntegerValue((int)swUserPreferenceIntegerValue_e.swDxfMappingFileIndex, m_OrigDxfMappingFileIndex);
            m_Doc.OwnerApplication.Sw.SetUserPreferenceIntegerValue((int)swUserPreferenceIntegerValue_e.swDxfMultiSheetOption, m_OrigDxfMultiSheetOption);

            m_SheetActivator?.Dispose();
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
