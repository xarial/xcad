using System;
using Xarial.XCad.Documents;

namespace Xarial.XCad.Inventor.Documents
{
    internal class AiStepSaveOptions : IXStepSaveOptions
    {
        public StepFormat_e Format { get; set; }
    }

    internal class AiSaveOptions : IXSaveOptions
    {
        public IXStepSaveOptions Step { get; }

        public IXPdfSaveOptions Pdf => throw new NotImplementedException();

        internal AiSaveOptions() 
        {
            Step = new AiStepSaveOptions();
        }
    }

    internal class AiDocumentOptions : IXDocumentOptions
    {
        public IXViewEntityKindVisibilityOptions ViewEntityKindVisibility => throw new NotImplementedException();

        public IXSaveOptions SaveOptions { get; }

        internal AiDocumentOptions() 
        {
            SaveOptions = new AiSaveOptions();
        }
    }
}
