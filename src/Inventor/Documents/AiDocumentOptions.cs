//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.Documents;

namespace Xarial.XCad.Inventor.Documents
{
    internal class AiDocumentOptions : IXDocumentOptions
    {
        public IXViewEntityKindVisibilityOptions ViewEntityKindVisibility => throw new NotImplementedException();
        public IXAnnotationsDraftingStandardOptions AnnotationsDraftingStandard => throw new NotImplementedException();
        public IXDimensionsDraftingStandardOptions DimensionsDraftingStandard => throw new NotImplementedException();
        public IXTablesDraftingStandardOptions TablesDraftingStandard => throw new NotImplementedException();
        public IXViewsDraftingStandardOptions ViewsDraftingStandard => throw new NotImplementedException();
        public IXSheetMetalDraftingStandardOptions SheetMetalDraftingStandard => throw new NotImplementedException();

        internal AiDocumentOptions() 
        {
        }
    }
}
