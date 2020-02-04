//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

namespace Xarial.XCad.Documents.Structures
{
    public class DocumentOpenArgs
    {
        public string Path { get; set; }
        public bool ReadOnly { get; set; }
        public bool ViewOnly { get; set; }
    }
}