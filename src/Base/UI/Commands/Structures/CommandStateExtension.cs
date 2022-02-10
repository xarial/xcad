//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.Documents;
using Xarial.XCad.UI.Commands.Enums;

namespace Xarial.XCad.UI.Commands.Structures
{
    public static class CommandStateExtension
    {
        public static void ResolveState(this CommandState state, WorkspaceTypes_e ws, IXApplication app)
        {
            var activeDoc = app.Documents.Active;

            WorkspaceTypes_e curSpace;

            if (activeDoc == null)
            {
                curSpace = WorkspaceTypes_e.NoDocuments;
            }
            else
            {
                if (activeDoc is IXPart)
                {
                    curSpace = WorkspaceTypes_e.Part;
                }
                else if (activeDoc is IXAssembly)
                {
                    curSpace = WorkspaceTypes_e.Assembly;
                }
                else if (activeDoc is IXDrawing)
                {
                    curSpace = WorkspaceTypes_e.Drawing;
                }
                else
                {
                    throw new NotSupportedException();
                }
            }

            state.Enabled = ws.HasFlag(curSpace);
        }
    }
}