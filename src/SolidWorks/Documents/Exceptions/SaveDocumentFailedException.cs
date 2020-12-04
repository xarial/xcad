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
            switch (err)
            {
                case swFileSaveError_e.swFileLockError:
                    return "File lock error";
                case swFileSaveError_e.swFileNameContainsAtSign:
                    return "File name cannot contain the at symbol(@)";
                case swFileSaveError_e.swFileNameEmpty:
                    return "File name cannot be empty";
                case swFileSaveError_e.swFileSaveAsBadEDrawingsVersion:
                    return "Bad eDrawings data";
                case swFileSaveError_e.swFileSaveAsDoNotOverwrite:
                    return "Cannot overwrite an existing file";
                case swFileSaveError_e.swFileSaveAsInvalidFileExtension:
                    return "File name extension does not match the SOLIDWORKS document type";
                case swFileSaveError_e.swFileSaveAsNameExceedsMaxPathLength:
                    return "File name cannot exceed 255 characters";
                case swFileSaveError_e.swFileSaveAsNotSupported:
                    return "Save As operation is not supported in this environment";
                case swFileSaveError_e.swFileSaveFormatNotAvailable:
                    return "Save As file type is not valid";
                case swFileSaveError_e.swFileSaveRequiresSavingReferences:
                    return "Saving an assembly with renamed components requires saving the references";
                case swFileSaveError_e.swGenericSaveError:
                    return "Generic error";
                case swFileSaveError_e.swReadOnlySaveError:
                    return "File is readonly";
                default:
                    return "Unknown error";
            }
        }

        public swFileSaveError_e ErrorCode { get; }

        public SaveDocumentFailedException(swFileSaveError_e errCode) 
            : base($"Failed to save document: {ParseSaveError(errCode)}")
        {
            ErrorCode = errCode;
        }
    }
}
