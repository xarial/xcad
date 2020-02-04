//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace Xarial.XCad.UI.PropertyPage.Enums
{
    [Flags]
    public enum PageOptions_e
    {
        OkayButton = 1,
        CancelButton = 2,
        LockedPage = 4,
        CloseDialogButton = 8,
        MultiplePages = 16,
        PushpinButton = 32,
        AllowHorizontalResize = 64,
        PreviewButton = 128,
        DisableSelection = 256,
        AbortCommands = 1024,
        UndoButton = 2048,
        CanEscapeCancel = 4096,
        HandleKeystrokes = 8192,
        RedoButton = 16384,
        DisablePageBuildDuringHandlers = 32768,
        GrayOutDisabledSelectionListboxes = 65536,
        SupportsChainSelection = 131072,
        SupportsIsolate = 262144
    }
}