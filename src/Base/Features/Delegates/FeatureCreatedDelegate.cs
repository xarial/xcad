//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Documents;

namespace Xarial.XCad.Features.Delegates
{
    /// <summary>
    /// Delegate of <see cref="IXFeatureRepository.FeatureCreated"/> notification
    /// </summary>
    /// <param name="doc">Document where new feature is added</param>
    /// <param name="feature">Feature which is added to the document</param>
    public delegate void FeatureCreatedDelegate(IXDocument doc, IXFeature feature);
}
