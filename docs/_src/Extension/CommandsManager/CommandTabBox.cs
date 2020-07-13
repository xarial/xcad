using Xarial.XCad.UI.Commands.Attributes;
using Xarial.XCad.UI.Commands.Enums;

public enum CommandsC_e
{
    [CommandItemInfo(true, true, WorkspaceTypes_e.Assembly,
        true, RibbonTabTextDisplay_e.NoText)]
    CommandC1,

    [CommandItemInfo(true, true, WorkspaceTypes_e.AllDocuments,
        true, RibbonTabTextDisplay_e.TextBelow)]
    CommandC2,

    [CommandItemInfo(true, true, WorkspaceTypes_e.AllDocuments,
        true, RibbonTabTextDisplay_e.TextHorizontal)]
    CommandC3,
}