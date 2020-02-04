//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Features;

namespace Xarial.XCad.Documents
{
    public interface IXDocument
    {
        event DocumentCloseDelegate Closing;

        string Title { get; }
        string Path { get; }

        void Close();

        IXFeatureRepository Features { get; }
    }
}