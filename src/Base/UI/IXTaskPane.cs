//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.UI.TaskPane;
using Xarial.XCad.UI.TaskPane.Delegates;

namespace Xarial.XCad.UI
{
    public interface IXTaskPane<TControl> : IXCustomPanel<TControl>
    {
        event TaskPaneButtonClickDelegate ButtonClick;
    }

    public interface IXEnumTaskPane<TControl, TCmdEnum> : IXTaskPane<TControl>
        where TCmdEnum : Enum
    {
        new event TaskPaneButtonEnumClickDelegate<TCmdEnum> ButtonClick;
    }

    internal class EnumTaskPane<TControl, TBtnEnum> : IXEnumTaskPane<TControl, TBtnEnum>, IDisposable
                where TBtnEnum : Enum
    {
        event TaskPaneButtonEnumClickDelegate<TBtnEnum> IXEnumTaskPane<TControl, TBtnEnum>.ButtonClick
        {
            add 
            {
                m_ButtonClick += value;
            }
            remove 
            {
                m_ButtonClick -= value;
            }
        }

        event TaskPaneButtonClickDelegate IXTaskPane<TControl>.ButtonClick
        {
            add
            {
                m_TaskPane.ButtonClick += value;
            }
            remove
            {
                m_TaskPane.ButtonClick -= value;
            }
        }

        public event ControlCreatedDelegate<TControl> ControlCreated;

        private TaskPaneButtonEnumClickDelegate<TBtnEnum> m_ButtonClick;
        private readonly IXTaskPane<TControl> m_TaskPane;

        internal EnumTaskPane(IXTaskPane<TControl> taskPane) 
        {
            m_TaskPane = taskPane;
            m_TaskPane.ButtonClick += OnButtonClick;
            ControlCreated?.Invoke(m_TaskPane.Control);
        }

        private void OnButtonClick(TaskPaneButtonSpec spec) 
        {
            m_ButtonClick?.Invoke((spec as TaskPaneEnumButtonSpec<TBtnEnum>).Value);
        }

        public bool IsActive 
        {
            get => m_TaskPane.IsActive;
            set => m_TaskPane.IsActive = value;
        }

        public TControl Control => m_TaskPane.Control;

        public void Dispose()
        {
            m_TaskPane.ButtonClick -= OnButtonClick;

            if (m_TaskPane is IDisposable)
            {
                (m_TaskPane as IDisposable).Dispose();
            }
        }

        public void Close() => m_TaskPane.Close();
    }
}
