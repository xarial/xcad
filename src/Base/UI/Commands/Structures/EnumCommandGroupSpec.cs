//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.ComponentModel;
using static Xarial.XCad.UI.Commands.IXCommandManagerExtension;

namespace Xarial.XCad.UI.Commands.Structures
{
    /// <summary>
    /// Command group spec created from enumeration
    /// </summary>
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public class EnumCommandGroupSpec : CommandGroupSpec
    {
        /// <summary>
        /// Enumeration type associated with this command group
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public Type CmdGrpEnumType { get; }

        /// <summary>
        /// Commands of this command group
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new EnumCommandSpec[] Commands { get => (EnumCommandSpec[])base.Commands; set => base.Commands = value; }
        
        internal EnumCommandGroupSpec(Type cmdGrpEnumType, int id) : base(id)
        {
            CmdGrpEnumType = cmdGrpEnumType;
        }
    }
}