using Xarial.XCad.UI.Commands.Attributes;
using Xarial.XCad.UI.Commands.Enums;

public enum CommandsD_e
{
    [CommandItemInfo(WorkspaceTypes_e.Part)]
    CommandD1,

    [CommandItemInfo(WorkspaceTypes_e.Part | WorkspaceTypes_e.Assembly)]
    CommandD2
}