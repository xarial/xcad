//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.SolidWorks.Geometry;
using SolidWorks.Interop.swconst;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry;
using Xarial.XCad.Base.Enums;
using System.Linq;
using Xarial.XCad.SolidWorks.Documents;

namespace Xarial.XCad.SolidWorks
{
    internal class SwObjectTracker : IXObjectTracker
    {
        private readonly string m_TrackDefName;

        private readonly int m_TrackDefId;

        private readonly ISwApplication m_SwApp;

        private readonly List<ISwObject> m_TrackedObjects;

        private readonly List<IXDocument> m_TrackedDocuments;

        internal SwObjectTracker(ISwApplication app, string name)
        {
            m_TrackDefName = name;
            m_SwApp = app;

            m_TrackDefId = m_SwApp.Sw.RegisterTrackingDefinition(m_TrackDefName);

            if (m_TrackDefId == -1)
            {
                throw new Exception("Failed to register tracking definition");
            }

            m_TrackedObjects = new List<ISwObject>();
            m_TrackedDocuments = new List<IXDocument>();
        }

        public void Track(IXObject obj, int trackId)
        {
            int trackRes;

            switch (obj)
            {
                case ISwFace face:
                    trackRes = face.Face.SetTrackingID(m_TrackDefId, trackId);
                    break;

                case ISwEdge edge:
                    trackRes = edge.Edge.SetTrackingID(m_TrackDefId, trackId);
                    break;

                case ISwVertex vertex:
                    trackRes = vertex.Vertex.SetTrackingID(m_TrackDefId, trackId);
                    break;

                case ISwBody body:
                    trackRes = body.Body.SetTrackingID(m_TrackDefId, trackId);
                    break;

                default:
                    throw new NotSupportedException("Cannot track this type of object (supported types: faces, edges, vertices and bodies)");
            }

            if (trackRes != (int)swTrackingIDError_e.swTrackingIDError_NoError)
            {
                throw new Exception($"Failed to track object. Error code: {(swTrackingIDError_e)trackRes}");
            }

            m_TrackedObjects.Add((ISwObject)obj);

            if (obj is SwObject) 
            {
                var ownerDoc = ((SwObject)obj).OwnerDocument;

                if (ownerDoc != null) 
                {
                    if (!m_TrackedDocuments.Any(d => d.Equals(ownerDoc))) 
                    {
                        m_TrackedDocuments.Add(ownerDoc);
                    }
                }
            }
        }

        public void Untrack(IXObject obj)
        {
            if (obj.IsAlive)
            {
                switch (obj)
                {
                    case ISwFace face:
                        face.Face.RemoveTrackingID(m_TrackDefId);
                        break;

                    case ISwEdge edge:
                        edge.Edge.RemoveTrackingID(m_TrackDefId);
                        break;

                    case ISwVertex vertex:
                        vertex.Vertex.RemoveTrackingID(m_TrackDefId);
                        break;

                    case ISwBody body:
                        body.Body.RemoveTrackingID(m_TrackDefId);
                        break;

                    default:
                        throw new NotSupportedException("Cannot track this type of object (supported types: faces, edges, vertices and bodies)");
                }
            }

            if (m_TrackedObjects.Contains(obj))
            {
                m_TrackedObjects.Remove((ISwObject)obj);
            }
        }

        public bool IsTracked(IXObject obj)
        {
            switch (obj)
            {
                case ISwFace face:
                    return face.Face.GetTrackingIDsCount(m_TrackDefId) > 0;

                case ISwEdge edge:
                    return edge.Edge.GetTrackingIDsCount(m_TrackDefId) > 0;

                case ISwVertex vertex:
                    return vertex.Vertex.GetTrackingIDsCount(m_TrackDefId) > 0;

                case ISwBody body:
                    return body.Body.GetTrackingIDsCount(m_TrackDefId) > 0;

                default:
                    throw new NotSupportedException("Cannot track this type of object (supported types: faces, edges, vertices and bodies)");
            }
        }

        public IXObject[] FindTrackedObjects(IXDocument doc, IXBody searchBody = null, Type[] searchFilter = null, int[] searchTrackIds = null)
        {
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            List<int> filters = null;

            if (searchFilter?.Any() == true)
            {
                filters = new List<int>();

                foreach (var filter in searchFilter)
                {
                    if (IsOfType<IXFace>(filter))
                    {
                        filters.Add((int)swTopoEntity_e.swTopoFace);
                    }
                    else if (IsOfType<IXEdge>(filter))
                    {
                        filters.Add((int)swTopoEntity_e.swTopoEdge);
                    }
                    else if (IsOfType<IXVertex>(filter))
                    {
                        filters.Add((int)swTopoEntity_e.swTopoVertex);
                    }
                    else if (IsOfType<IXBody>(filter))
                    {
                        filters.Add((int)swTopoEntity_e.swTopoBody);
                    }
                }
            }

            var ents = (object[])((ISwDocument)doc).Model.Extension.FindTrackedObjects(m_TrackDefId,
                        ((ISwBody)searchBody)?.Body, filters?.ToArray(), searchTrackIds);

            var res = new List<ISwObject>();

            if (ents?.Any() == true)
            {
                foreach (var ent in ents)
                {
                    res.Add(((ISwDocument)doc).CreateObjectFromDispatch<ISwObject>(ent));
                }
            }

            return res.ToArray();
        }

        private bool IsOfType<T>(Type t)
            => typeof(T).IsAssignableFrom(t);

        public int GetTrackingId(IXObject obj)
        {
            object trackIds;
            int trackRes;

            switch (obj)
            {
                case ISwFace face:
                    trackRes = face.Face.GetTrackingIDs(m_TrackDefId, out trackIds);
                    break;

                case ISwEdge edge:
                    trackRes = edge.Edge.GetTrackingIDs(m_TrackDefId, out trackIds);
                    break;

                case ISwVertex vertex:
                    trackRes = vertex.Vertex.GetTrackingIDs(m_TrackDefId, out trackIds);
                    break;

                case ISwBody body:
                    trackRes = body.Body.GetTrackingIDs(m_TrackDefId, out trackIds);
                    break;

                default:
                    throw new NotSupportedException("Cannot track this type of object (supported types: faces, edges, vertices and bodies)");
            }

            if (trackRes != (int)swTrackingIDError_e.swTrackingIDError_NoError)
            {
                throw new Exception($"Failed to find tracking id. Error code: {(swTrackingIDError_e)trackRes}");
            }

            if (((int[])trackIds).Any() != true)
            {
                throw new Exception("No tracking ids found");
            }

            return ((int[])trackIds).First();
        }

        public void Dispose()
        {
            foreach (var trackedObj in m_TrackedObjects.ToArray())
            {
                try
                {
                    Untrack(trackedObj);
                }
                catch 
                {
                }
            }

            m_TrackedObjects.Clear();

            foreach (var trackedDoc in m_TrackedDocuments) 
            {
                foreach (var trackedObj in FindTrackedObjects(trackedDoc)) 
                {
                    Untrack(trackedObj);
                }
            }

            m_TrackedDocuments.Clear();
        }
    }
}
