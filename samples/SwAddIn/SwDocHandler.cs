//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Xarial.XCad;
using Xarial.XCad.Data.Enums;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Services;

namespace SwAddInExample
{
    public class SwDocHandler : IDocumentHandler
    {
        public class RevData
        {
            public int Revision { get; set; }
            public Guid RevisionStamp { get; set; }
        }

        private const string STREAM_NAME = "_xCadStream_";
        private const string SUB_STORAGE_PATH = "_xCadStorage1_\\SubStorage2";
        private const string TIME_STAMP_STREAM_NAME = "TimeStampStream";
        private const string USER_NAME_STREAM_NAME = "UserName";

        private RevData m_RevData;

        private IXApplication m_App;
        private IXDocument m_Model;

        public void Init(IXApplication app, IXDocument model)
        {
            m_App = app;
            m_Model = model;

            m_Model.StreamReadAvailable += LoadFromStream;
            m_Model.StreamWriteAvailable += SaveToStream;
            m_Model.StorageReadAvailable += LoadFromStorage;
            m_Model.StorageWriteAvailable += SaveToStorage;

            m_App.ShowMessageBox($"Opened {model.Title}");
        }

        private void SaveToStream(IXDocument doc)
        {
            using (var stream = doc.OpenStream(STREAM_NAME, AccessType_e.Write))
            {
                var xmlSer = new XmlSerializer(typeof(RevData));

                if (m_RevData == null)
                {
                    m_RevData = new RevData();
                }

                m_RevData.Revision = m_RevData.Revision + 1;
                m_RevData.RevisionStamp = Guid.NewGuid();

                xmlSer.Serialize(stream, m_RevData);
            }
        }

        private void LoadFromStream(IXDocument doc)
        {
            using (var stream = doc.TryOpenStream(STREAM_NAME, AccessType_e.Read))
            {
                if (stream != null)
                {
                    var xmlSer = new XmlSerializer(typeof(RevData));
                    m_RevData = xmlSer.Deserialize(stream) as RevData;
                    //m_App.ShowMessageBox($"Revision data of {doc.Title}: {m_RevData.Revision} - {m_RevData.RevisionStamp}");
                }
                else
                {
                    //m_App.ShowMessageBox($"No revision data stored in {doc.Title}");
                }
            }
        }

        private void LoadFromStorage(IXDocument doc)
        {
            var path = SUB_STORAGE_PATH.Split('\\');

            using (var storage = doc.TryOpenStorage(path[0], AccessType_e.Read))
            {
                if (storage != null)
                {
                    using (var subStorage = storage.TryOpenStorage(path[1], false))
                    {
                        if (subStorage != null)
                        {
                            foreach (var subStreamName in subStorage.GetSubStreamNames())
                            {
                                using (var str = subStorage.TryOpenStream(subStreamName, false))
                                {
                                    if (str != null)
                                    {
                                        var buffer = new byte[str.Length];

                                        str.Read(buffer, 0, buffer.Length);

                                        var timeStamp = Encoding.UTF8.GetString(buffer);

                                        //m_App.ShowMessageBox($"Metadata stamp in {subStreamName} of {doc.Title}: {timeStamp}");
                                    }
                                    else
                                    {
                                        //m_App.ShowMessageBox($"No metadata stamp stream in {doc.Title}");
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    //m_App.ShowMessageBox($"No metadata storage in {doc.Title}");
                }
            }
        }

        private void SaveToStorage(IXDocument doc)
        {
            var path = SUB_STORAGE_PATH.Split('\\');

            using (var storage = doc.OpenStorage(path[0], AccessType_e.Write))
            {
                using (var subStorage = storage.TryOpenStorage(path[1], true))
                {
                    using (var str = subStorage.TryOpenStream(TIME_STAMP_STREAM_NAME, true))
                    {
                        var buffer = Encoding.UTF8.GetBytes(DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss"));
                        str.Write(buffer, 0, buffer.Length);
                    }

                    using (var str = subStorage.TryOpenStream(USER_NAME_STREAM_NAME, true))
                    {
                        var buffer = Encoding.UTF8.GetBytes(System.Environment.UserName);
                        str.Write(buffer, 0, buffer.Length);
                    }
                }
            }
        }

        public void Dispose()
        {
            m_Model.StreamReadAvailable -= LoadFromStream;
            m_Model.StreamWriteAvailable -= SaveToStream;
            m_Model.StorageReadAvailable -= LoadFromStorage;
            m_Model.StorageWriteAvailable -= SaveToStorage;

            m_App.ShowMessageBox($"Closed {m_Model.Title}");
        }
    }
}
