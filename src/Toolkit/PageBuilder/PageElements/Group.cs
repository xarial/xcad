//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Utils.PageBuilder.Base;

namespace Xarial.XCad.Utils.PageBuilder.PageElements
{
    public abstract class Group<TVal> : Control<TVal>, IGroup
    {
#pragma warning disable CS0067

        protected override event ControlValueChangedDelegate<TVal> ValueChanged;

#pragma warning restore CS0067

        protected Group(int id, object tag) : base(id, tag)
        {
        }

        protected override TVal GetSpecificValue()
        {
            return default(TVal);
        }

        protected override void SetSpecificValue(TVal value)
        {
        }
    }

    public abstract class Group : Group<object>
    {
        protected Group(int id, object tag) : base(id, tag)
        {
        }
    }
}