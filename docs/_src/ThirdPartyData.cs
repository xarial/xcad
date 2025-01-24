using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Serialization;
using Xarial.XCad.Data.Enums;
using Xarial.XCad.Documents;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;

namespace Xarial.XCad.Documentation
{
    [ComVisible(true), Guid("64684CEF-131C-4F08-88F7-B3C3BAA7004E")]
    public class ThirdPartyDataAddIn : SwAddInEx
    {
        #region Stream
        private const string STREAM_NAME = "CodeStackStream";

        public class StreamData
        {
            public string Prp1 { get; set; }
            public double Prp2 { get; set; }
        }

        private StreamData m_StreamData;

        #region StreamLoad
        private void LoadFromStream(ISwDocument model)
        {
            using (var str = model.TryOpenStream(STREAM_NAME, AccessType_e.Read))
            {
                if (str != null)
                {
                    var xmlSer = new XmlSerializer(typeof(StreamData));
                    m_StreamData = xmlSer.Deserialize(str) as StreamData;
                }
            }
        }
        #endregion StreamLoad
        #region StreamSave
        private void SaveToStream(ISwDocument model)
        {
            using (var str = model.OpenStream(STREAM_NAME, AccessType_e.Write))
            {
                var xmlSer = new XmlSerializer(typeof(StreamData));
                xmlSer.Serialize(str, m_StreamData);
            }
        }
        #endregion StreamSave
        #endregion Stream
        #region Storage
        private const string STORAGE_NAME = "CodeStackStorage";
        private const string STREAM1_NAME = "CodeStackStream1";
        private const string STREAM2_NAME = "CodeStackStream2";
        private const string SUB_STORAGE_NAME = "CodeStackSubStorage";

        public class StorageStreamData
        {
            public int Prp3 { get; set; }
            public bool Prp4 { get; set; }
        }

        private StorageStreamData m_StorageData;

        #region StorageLoad
        private void LoadFromStorageStore(ISwDocument model)
        {
            using (var storage = model.TryOpenStorage(STORAGE_NAME, AccessType_e.Read))
            {
                if (storage != null)
                {
                    using (var str = storage.TryOpenStream(STREAM1_NAME, false))
                    {
                        if (str != null)
                        {
                            var xmlSer = new XmlSerializer(typeof(StorageStreamData));
                            m_StorageData = xmlSer.Deserialize(str) as StorageStreamData;
                        }
                    }

                    using (var subStorage = storage.TryOpenStorage(SUB_STORAGE_NAME, false))
                    {
                        if (subStorage != null)
                        {
                            using (var str = subStorage.TryOpenStream(STREAM2_NAME, false))
                            {
                                if (str != null)
                                {
                                    var buffer = new byte[str.Length];
                                    str.Read(buffer, 0, buffer.Length);
                                    var dateStr = Encoding.UTF8.GetString(buffer);
                                    var date = DateTime.Parse(dateStr);
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion StorageLoad
        #region StorageSave
        private void SaveToStorageStore(ISwDocument model)
        {
            using (var storage = model.OpenStorage(STORAGE_NAME, AccessType_e.Write))
            {
                using (var str = storage.TryOpenStream(STREAM1_NAME, true))
                {
                    var xmlSer = new XmlSerializer(typeof(StorageStreamData));

                    xmlSer.Serialize(str, m_StorageData);
                }

                using (var subStorage = storage.TryOpenStorage(SUB_STORAGE_NAME, true))
                {
                    using (var str = subStorage.TryOpenStream(STREAM2_NAME, true))
                    {
                        var buffer = Encoding.UTF8.GetBytes(DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss"));
                        str.Write(buffer, 0, buffer.Length);
                    }
                }
            }
        }
        #endregion StorageSave
        #endregion Storage

        public override void OnConnect()
        {
            var doc = this.Application.Documents.Active;

            HandleStream(doc);
            HandleStorage(doc);
        }

        #region StreamHandler
        private void HandleStream(ISwDocument doc)
        {
            doc.StreamReadAvailable += OnStreamReadAvailable;
            doc.StreamWriteAvailable += OnStreamWriteAvailable;
        }

        private void OnStreamWriteAvailable(IXDocument doc)
        {
            SaveToStream(doc as ISwDocument);
        }

        private void OnStreamReadAvailable(IXDocument doc)
        {
            LoadFromStream(doc as ISwDocument);
        }
        #endregion StreamHandler

        #region StorageHandler
        private void HandleStorage(ISwDocument doc)
        {
            doc.StorageReadAvailable += OnStorageReadAvailable;
            doc.StorageWriteAvailable += OnStorageWriteAvailable;
        }

        private void OnStorageWriteAvailable(IXDocument doc)
        {
            SaveToStorageStore(doc as ISwDocument);
        }

        private void OnStorageReadAvailable(IXDocument doc)
        {
            LoadFromStorageStore(doc as ISwDocument);
        }
        #endregion StorageHandler
    }
}
