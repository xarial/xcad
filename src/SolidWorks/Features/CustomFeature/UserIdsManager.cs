//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System.Linq;
using System;
using Xarial.XCad.Geometry;
using Xarial.XCad.SolidWorks.Geometry;

namespace Xarial.XCad.SolidWorks.Features.CustomFeature
{
    /// <summary>
    /// Service for managing user ids of the geometry of the <see cref="ISwMacroFeature"/>
    /// </summary>
    public interface IUserIdsManager 
    {
        /// <summary>
        /// Automatically assigns user ids for the entities of the body
        /// </summary>
        /// <param name="body">Body</param>
        void AssignUserIds(IXBody body);
    }

    /// <summary>
    /// Service to automatically assign user ids to bodies
    /// </summary>
    internal class UserIdsManager : IUserIdsManager
    {
        private readonly ISwMacroFeature m_Feat;
        private readonly IMacroFeatureData m_FeatData;

        private int m_CurFaceId;
        private int m_CurEdgeId;

        public UserIdsManager(SwMacroFeature feat)
        {
            m_Feat = feat;

            m_FeatData = feat.FeatureData;
        }

        public void AssignUserIds(IXBody body)
        {
            if (body is ISwBody)
            {
                var bodySw = ((ISwBody)body).Body;

                m_FeatData.GetEntitiesNeedUserId(bodySw, out var facesObj, out var edgesObj);

                var faces = (object[])facesObj;
                var edges = (object[])edgesObj;

                if (faces?.Any() == true)
                {
                    m_CurFaceId++;

                    for (int i = 0; i < faces.Length; i++)
                    {
                        var face = (Face2)faces[i];

                        if (!m_FeatData.SetFaceUserId(face, m_CurFaceId, i + 1))
                        {
                            throw new Exception("Failed to set face id");
                        }
                    }
                }

                if (edges?.Any() == true)
                {
                    m_CurEdgeId++;

                    for (int i = 0; i < edges.Length; i++)
                    {
                        var edge = (Edge)edges[i];

                        if (!m_FeatData.SetEdgeUserId(edge, m_CurEdgeId, i + 1))
                        {
                            throw new Exception("Failed to set edge id");
                        }
                    }
                }
            }
            else 
            {
                throw new InvalidCastException($"Only bodies of type '{nameof(ISwBody)}' are supported");
            }
        }
    }

    /// <summary>
    /// Empty user ids manager
    /// </summary>
    internal class EmptyUserIdsManager : IUserIdsManager
    {
        public void AssignUserIds(IXBody body)
        {
            //Do nothing
        }
    }
}