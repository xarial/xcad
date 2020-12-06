using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.SolidWorks.Documents.Exceptions
{
    public class SaveDocumentFailedException : Exception
    {
        private static string ParseSaveError(swFileSaveError_e err)
        {
            var errors = new StringBuilder();

            if (err.HasFlag(swFileSaveError_e.swFileLockError))
            {
                errors.AppendLine("File lock error");
            }

            if (err.HasFlag(swFileSaveError_e.swFileNameContainsAtSign))
            {
                errors.AppendLine("File name cannot contain the at symbol(@)");
            }

            if (err.HasFlag(swFileSaveError_e.swFileNameEmpty))
            {
                errors.AppendLine("File name cannot be empty");
            }

            if (err.HasFlag(swFileSaveError_e.swFileSaveAsBadEDrawingsVersion))
            {
                errors.AppendLine("Bad eDrawings data");
            }

            if (err.HasFlag(swFileSaveError_e.swFileSaveAsDoNotOverwrite))
            {
                errors.AppendLine("Cannot overwrite an existing file");
            }

            if (err.HasFlag(swFileSaveError_e.swFileSaveAsInvalidFileExtension))
            {
                errors.AppendLine("File name extension does not match the SOLIDWORKS document type");
            }

            if (err.HasFlag(swFileSaveError_e.swFileSaveAsNameExceedsMaxPathLength))
            {
                errors.AppendLine("File name cannot exceed 255 characters");
            }

            if (err.HasFlag(swFileSaveError_e.swFileSaveAsNotSupported))
            {
                errors.AppendLine("Save As operation is not supported in this environment");
            }

            if (err.HasFlag(swFileSaveError_e.swFileSaveFormatNotAvailable))
            {
                errors.AppendLine("Save As file type is not valid");
            }

            if (err.HasFlag(swFileSaveError_e.swFileSaveRequiresSavingReferences))
            {
                errors.AppendLine("Saving an assembly with renamed components requires saving the references");
            }

            if (err.HasFlag(swFileSaveError_e.swGenericSaveError))
            {
                errors.AppendLine("Generic error");
            }

            if (err.HasFlag(swFileSaveError_e.swReadOnlySaveError))
            {
                errors.AppendLine("File is readonly");
            }

            if (errors.Length == 0) 
            {
                errors.Append("Unknown error");
            }

            return errors.ToString();
        }

        public swFileSaveError_e ErrorCode { get; }

        public SaveDocumentFailedException(swFileSaveError_e errCode) 
            : base($"Failed to save document: {ParseSaveError(errCode)}")
        {
            ErrorCode = errCode;
        }
    }
}
