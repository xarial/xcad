//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace Xarial.XCad.UI.Commands.Attributes
{
    /// <summary>
    /// Marks the command to be separated by the spacer (separator) in the menu and the toolbar
    /// </summary>
    /// <remarks>Spacer is added before the command marked with this attribute</remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public class CommandSpacerAttribute : Attribute
    {
    }
}