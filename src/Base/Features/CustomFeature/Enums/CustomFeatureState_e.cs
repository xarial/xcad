//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace Xarial.XCad.Features.CustomFeature.Enums
{
    /// <summary>
    /// State of the <see cref="IXCustomFeature"/> within the <see cref="IXCustomFeatureDefinition.OnUpdateState(IXApplication, Documents.IXDocument, IXCustomFeature)"/>
    /// </summary>
    [Flags]
    public enum CustomFeatureState_e
    {
        Default = 0,
        CannotBeDeleted = 1,
        NotEditable = 2,
        CannotBeSuppressed = 4,
        CannotBeReplaced = 8,
        EnableNote = 16,
        CannotBeRolledBack = 32
    }
}