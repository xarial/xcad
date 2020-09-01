//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.IO;
using Xarial.XCad.Annotations;
using Xarial.XCad.Data;
using Xarial.XCad.Data.Enums;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Features;

namespace Xarial.XCad.Documents
{
    public interface IXDocument
    {
        event DataStoreAvailableDelegate StreamReadAvailable;
        event DataStoreAvailableDelegate StorageReadAvailable;
        event DataStoreAvailableDelegate StreamWriteAvailable;
        event DataStoreAvailableDelegate StorageWriteAvailable;

        event DocumentRebuildDelegate Rebuild;
        event DocumentSaveDelegate Saving;
        event DocumentCloseDelegate Closing;

        string Title { get; }
        string Path { get; }

        bool IsDirty { get; set; }

        bool Visible { get; set; }

        void Close();

        IXFeatureRepository Features { get; }

        IXSelectionRepository Selections { get; }

        IXDimensionRepository Dimensions { get; }

        IXPropertyRepository Properties { get; }

        Stream OpenStream(string name, AccessType_e access);
        IStorage OpenStorage(string name, AccessType_e access);
    }
}