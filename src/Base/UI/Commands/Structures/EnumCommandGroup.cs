//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.ComponentModel;
using Xarial.XCad.UI.Commands.Delegates;
using static Xarial.XCad.UI.Commands.IXCommandManagerExtension;

namespace Xarial.XCad.UI.Commands.Structures
{
    public interface IEnumCommandBar<TCmdEnum> : IXCommandGroup
        where TCmdEnum : Enum
    {
        new event CommandEnumClickDelegate<TCmdEnum> CommandClick;

        new event CommandEnumStateDelegate<TCmdEnum> CommandStateResolve;
    }

    internal class EnumCommandGroup<TCmdEnum> : IEnumCommandBar<TCmdEnum>
                where TCmdEnum : Enum
    {
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event CommandClickDelegate CommandClick
        {
            add
            {
                m_CmdBar.CommandClick += value;
            }
            remove
            {
                m_CmdBar.CommandClick -= value;
            }
        }

        event CommandEnumClickDelegate<TCmdEnum> IEnumCommandBar<TCmdEnum>.CommandClick
        {
            add
            {
                m_CommandClick += value;
            }
            remove
            {
                m_CommandClick -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event CommandStateDelegate CommandStateResolve
        {
            add
            {
                m_CmdBar.CommandStateResolve += value;
            }
            remove
            {
                m_CmdBar.CommandStateResolve -= value;
            }
        }

        event CommandEnumStateDelegate<TCmdEnum> IEnumCommandBar<TCmdEnum>.CommandStateResolve
        {
            add
            {
                m_CommandState += value;
            }
            remove
            {
                m_CommandState -= value;
            }
        }

        private readonly IXCommandGroup m_CmdBar;
        private CommandEnumClickDelegate<TCmdEnum> m_CommandClick;
        private CommandEnumStateDelegate<TCmdEnum> m_CommandState;

        public CommandGroupSpec Spec => m_CmdBar.Spec;

        internal EnumCommandGroup(IXCommandGroup cmdBar)
        {
            m_CmdBar = cmdBar;

            CommandClick += OnCommandClick;
            CommandStateResolve += OnCommandStateResolve;
        }

        private void OnCommandClick(CommandSpec spec)
        {
            if (spec is EnumCommandSpec<TCmdEnum>)
            {
                m_CommandClick?.Invoke((spec as EnumCommandSpec<TCmdEnum>).Value);
            }
        }

        private void OnCommandStateResolve(CommandSpec spec, ref CommandState state)
        {
            if (spec is EnumCommandSpec<TCmdEnum>)
            {
                m_CommandState?.Invoke((spec as EnumCommandSpec<TCmdEnum>).Value, ref state);
            }
        }
    }
}