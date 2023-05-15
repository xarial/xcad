//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry;

namespace Xarial.XCad
{
    /// <summary>
    /// Allows to track objects across operations
    /// </summary>
    public interface IXObjectTracker : IDisposable
    {
        /// <summary>
        /// Tracks the specified object
        /// </summary>
        /// <param name="obj">Object to track</param>
        /// <param name="trackId">Tracking id</param>
        void Track(IXObject obj, int trackId);

        /// <summary>
        /// Stops tracking of the specified object
        /// </summary>
        /// <param name="obj">Object to untrack</param>
        void Untrack(IXObject obj);

        /// <summary>
        /// Checks if the object is currently being tracked
        /// </summary>
        /// <param name="obj">Object to check</param>
        /// <returns>True if object is tracked, False if not</returns>
        bool IsTracked(IXObject obj);

        /// <summary>
        /// Finds the tracked objects of this tracker in the specified context
        /// </summary>
        /// <param name="doc">Document where to find tracked objects</param>
        /// <param name="searchBody">Optional body where to find the tracked objects, null to search in all objects</param>
        /// <param name="searchFilter">Optional filters of the objects to find, null to find all</param>
        /// <param name="searchTrackIds">Optional ids to find, null to find all ids</param>
        /// <returns>Tracked object or empty if no objects found</returns>
        IXObject[] FindTrackedObjects(IXDocument doc, IXBody searchBody = null, Type[] searchFilter = null, int[] searchTrackIds = null);

        /// <summary>
        /// Finds the tracking id of the specified object
        /// </summary>
        /// <param name="obj">Object to find th tracking id</param>
        /// <returns>Tracking id</returns>
        int GetTrackingId(IXObject obj);
    }
}
