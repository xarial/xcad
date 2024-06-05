//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using Xarial.XCad;
using Xarial.XCad.Data;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Attributes;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Documents.Services;
using Xarial.XCad.Extensions;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.UI;

namespace SwAddInExample
{
    //[DocumentHandlerFilter(typeof(ISwDocument3D))]
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

        private readonly IXExtension m_Ext;

        private IXCustomPanel<WpfUserControl> m_FeatMgrTab;

        private readonly CustomGraphicsToggle m_CustomGraphicsToggle;

        private IXCustomGraphicsRenderer m_RedRectRenderer;
        private IXCustomGraphicsRenderer m_GreenRectRenderer;

        private IXModelView m_ModelView;

        public SwDocHandler(IXExtension ext, CustomGraphicsToggle customGraphicsToggle) 
        {
            m_Ext = ext;
            m_CustomGraphicsToggle = customGraphicsToggle;
            m_CustomGraphicsToggle.EnabledChanged += OnCustomGraphicsToggleEnabledChanged;
        }

        private void OnCustomGraphicsToggleEnabledChanged(bool enabled)
        {
            if (m_ModelView != null)
            {
                if (enabled)
                {
                    AddRenderers();
                }
                else
                {
                    m_ModelView.CustomGraphicsContext.UnregisterRenderer(m_RedRectRenderer);
                    m_ModelView.CustomGraphicsContext.UnregisterRenderer(m_GreenRectRenderer);
                }
            }
        }

        public void Init(IXApplication app, IXDocument model)
        {
            m_App = app;
            m_Model = model;

            m_Model.StreamReadAvailable += LoadFromStream;
            m_Model.StreamWriteAvailable += SaveToStream;
            m_Model.StorageReadAvailable += LoadFromStorage;
            m_Model.StorageWriteAvailable += SaveToStorage;

            m_FeatMgrTab = m_Ext.CreateFeatureManagerTab<WpfUserControl>(model);

            m_ModelView = model.ModelViews.Active;

            if (m_CustomGraphicsToggle.Enabled)
            {
                AddRenderers();
            }

            //m_App.ShowMessageBox($"Opened {model.Title}");
        }

        private void AddRenderers()
        {
            if (!m_Model.State.HasFlag(DocumentState_e.Hidden))
            {
                m_RedRectRenderer = new OglRectangeRenderer(
                    new Xarial.XCad.Geometry.Structures.Rect2D(0.1, 0.5, new Xarial.XCad.Geometry.Structures.Point(0, 0, 0)),
                    System.Drawing.Color.Red);

                m_GreenRectRenderer = new OglRectangeRenderer(
                    new Xarial.XCad.Geometry.Structures.Rect2D(0.25, 0.25, new Xarial.XCad.Geometry.Structures.Point(0.5, 0.5, 0)),
                    System.Drawing.Color.Green);

                m_ModelView.CustomGraphicsContext.RegisterRenderer(m_RedRectRenderer);
                m_ModelView.CustomGraphicsContext.RegisterRenderer(m_GreenRectRenderer);
            }
        }

        private void SaveToStream(IXDocument doc)
        {
            using (var stream = doc.OpenStream(STREAM_NAME, true))
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
            using (var stream = doc.OpenStream(STREAM_NAME, false))
            {
                if (stream != Stream.Null)
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

            using (var storage = doc.OpenStorage(path[0], false))
            {
                if (storage != Storage.Null)
                {
                    using (var subStorage = storage.OpenStorage(path[1], false))
                    {
                        if (subStorage != Storage.Null)
                        {
                            foreach (var subStreamName in subStorage.SubStreamNames)
                            {
                                using (var str = subStorage.OpenStream(subStreamName, false))
                                {
                                    if (str != Stream.Null)
                                    {
                                        var buffer = new byte[str.Length];

                                        str.Read(buffer, 0, buffer.Length);

                                        var data = Encoding.UTF8.GetString(buffer);

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

            using (var storage = doc.OpenStorage(path[0], true))
            {
                using (var subStorage = storage.OpenStorage(path[1], true))
                {
                    using (var str = subStorage.OpenStream(TIME_STAMP_STREAM_NAME, true))
                    {
                        var buffer = Encoding.UTF8.GetBytes(DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss"));
                        str.Write(buffer, 0, buffer.Length);
                    }

                    using (var str = subStorage.OpenStream(USER_NAME_STREAM_NAME, true))
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

            m_CustomGraphicsToggle.EnabledChanged -= OnCustomGraphicsToggleEnabledChanged;

            System.Diagnostics.Debug.Print($"Closed {m_Model.Title}");

            m_FeatMgrTab.Close();

            //m_App.ShowMessageBox($"Closed {m_Model.Title}");
        }
    }
}
