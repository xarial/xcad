//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using Xarial.XCad.Documents;
using Xarial.XCad.UI.Commands.Enums;

namespace Xarial.XCad.UI.Commands.Structures
{
    /// <summary>
    /// Additional methods of <see cref="CommandState"/>
    /// </summary>
    public static class CommandStateExtension
    {
        /// <summary>
        /// Resolves the default state based on the workspace
        /// </summary>
        /// <param name="state">Current state</param>
        /// <param name="ws">Workspace</param>
        /// <param name="app">Application</param>
        public static void ResolveState(this CommandState state, WorkspaceTypes_e ws, IXApplication app)
        {
            bool enabled;

            if (ws == WorkspaceTypes_e.All)
            {
                enabled = true;
            }
            else 
            {
                enabled = false;

                var activeDoc = app.Documents.Active;

                if (activeDoc == null)
                {
                    enabled = ws.HasFlag(WorkspaceTypes_e.NoDocuments);
                }
                else
                {
                    switch (activeDoc)
                    {
                        case IXPart _:
                            enabled = ws.HasFlag(WorkspaceTypes_e.Part);
                            break;

                        case IXAssembly assm:
                            enabled = ws.HasFlag(WorkspaceTypes_e.Assembly);
                            if (!enabled)
                            {
                                if (ws.HasFlag(WorkspaceTypes_e.InContextPart))
                                {
                                    var editComp = assm.EditingComponent;
                                    enabled = editComp != null && editComp.ReferencedDocument is IXPart;
                                }
                            }
                            break;

                        case IXDrawing _:
                            enabled = ws.HasFlag(WorkspaceTypes_e.Drawing);
                            break;
                    }
                }
            }

            state.Enabled = enabled;
        }
    }
}