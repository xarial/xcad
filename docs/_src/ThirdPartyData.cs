using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Serialization;
using Xarial.XCad.Data;
using Xarial.XCad.Documents;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;

namespace Xarial.XCad.Documentation
{
    [ComVisible(true), Guid("64684CEF-131C-4F08-88F7-B3C3BAA7004E")]
    public class ThirdPartyDataAddIn : SwAddInEx
    {
        //--- Stream
        private const string STREAM_NAME = "CodeStackStream";

        public class StreamData
        {
            public string Prp1 { get; set; }
            public double Prp2 { get; set; }
        }

        private StreamData m_StreamData;

        //--- StreamLoad
        private void LoadFromStream(ISwDocument model)
        {
            using (var str = model.OpenStream(STREAM_NAME, false))
            {
                if (str != Stream.Null)
                {
                    var xmlSer = new XmlSerializer(typeof(StreamData));
                    m_StreamData = xmlSer.Deserialize(str) as StreamData;
                }
            }
        }
        //---
        //--- StreamSave
        private void SaveToStream(ISwDocument model)
        {
            using (var str = model.OpenStream(STREAM_NAME, true))
            {
                var xmlSer = new XmlSerializer(typeof(StreamData));
                xmlSer.Serialize(str, m_StreamData);
            }
        }
        //---
        //---
        //--- Storage
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

        //--- StorageLoad
        private void LoadFromStorageStore(ISwDocument model)
        {
            using (var storage = model.OpenStorage(STORAGE_NAME, false))
            {
                if (storage != Storage.Null)
                {
                    using (var str = storage.OpenStream(STREAM1_NAME, false))
                    {
                        if (str != Stream.Null)
                        {
                            var xmlSer = new XmlSerializer(typeof(StorageStreamData));
                            m_StorageData = xmlSer.Deserialize(str) as StorageStreamData;
                        }
                    }

                    using (var subStorage = storage.OpenStorage(SUB_STORAGE_NAME, false))
                    {
                        if (subStorage != Storage.Null)
                        {
                            using (var str = subStorage.OpenStream(STREAM2_NAME, false))
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
        //---
        //--- StorageSave
        private void SaveToStorageStore(ISwDocument model)
        {
            using (var storage = model.OpenStorage(STORAGE_NAME, true))
            {
                using (var str = storage.OpenStream(STREAM1_NAME, true))
                {
                    var xmlSer = new XmlSerializer(typeof(StorageStreamData));

                    xmlSer.Serialize(str, m_StorageData);
                }

                using (var subStorage = storage.OpenStorage(SUB_STORAGE_NAME, true))
                {
                    using (var str = subStorage.OpenStream(STREAM2_NAME, true))
                    {
                        var buffer = Encoding.UTF8.GetBytes(DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss"));
                        str.Write(buffer, 0, buffer.Length);
                    }
                }
            }
        }
        //---
        //---
        
        public override void OnConnect()
        {
            var doc = this.Application.Documents.Active;

            HandleStream(doc);
            HandleStorage(doc);
        }

        //--- StreamHandler
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
        //---

        //--- StorageHandler
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
        //---
    }
}
