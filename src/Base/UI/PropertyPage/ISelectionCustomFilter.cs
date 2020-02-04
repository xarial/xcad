//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Base.Enums;
using Xarial.XCad.UI.PropertyPage.Base;

namespace Xarial.XCad.UI.PropertyPage
{
    public interface ISelectionCustomFilter
    {
        bool Filter(IControl selBox, IXSelObject selection, SelectType_e selType, ref string itemText);
    }
}