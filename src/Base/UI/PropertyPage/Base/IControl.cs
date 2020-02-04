//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace Xarial.XCad.UI.PropertyPage.Base
{
    public delegate void ControlObjectValueChangedDelegate(IControl sender, object newValue);

    public interface IControl : IDisposable
    {
        event ControlObjectValueChangedDelegate ValueChanged;

        int Id { get; }

        object Tag { get; }

        object GetValue();

        void SetValue(object value);
    }
}