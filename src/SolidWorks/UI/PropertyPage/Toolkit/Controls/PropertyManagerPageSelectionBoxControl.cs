//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Toolkit.PageBuilder.Constructors;
using Xarial.XCad.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage.Services;
using Xarial.XCad.UI.PropertyPage.Structures;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls
{
    //TODO: think of a better way to work with type instead of object (can be either SwSelObject or List). See how combo box is implemented
    internal class PropertyManagerPageSelectionBoxControl : PropertyManagerPageBaseControl<object, IPropertyManagerPageSelectionbox>
    {
        protected override event ControlValueChangedDelegate<object> ValueChanged;

        private readonly ISwApplication m_App;
        private readonly Type m_ObjType;
        private readonly Type m_ElementType;
        private readonly ISelectionCustomFilter m_CustomFilter;

        private readonly bool m_DefaultFocus;

        public PropertyManagerPageSelectionBoxControl(ISwApplication app, int id, object tag,
            IPropertyManagerPageSelectionbox selBox,
            SwPropertyManagerPageHandler handler, Type objType, ISelectionCustomFilter customFilter, bool defaultFocus,
            IPropertyManagerPageLabel label)
            : base(selBox, id, tag, handler, label)
        {
            m_App = app;
            m_ObjType = objType;
            m_ElementType = SelectionBoxConstructorHelper.GetElementType(m_ObjType);
            m_CustomFilter = customFilter;

            m_DefaultFocus = defaultFocus;

            m_Handler.SelectionChanged += OnSelectionChanged;

            if (m_DefaultFocus)
            {
                m_Handler.Opened += OnPageOpened;
            }

            m_Handler.SubmitSelection += OnSubmitSelection;
        }

        private void OnPageOpened()
        {
            if (m_DefaultFocus) 
            {
                if (Visible)
                {
                    SwSpecificControl.SetSelectionFocus();
                }
            }
        }

        internal IPropertyManagerPageSelectionbox SelectionBox => SwSpecificControl;

        private SwSelObject ToSelObject(object disp) => m_App.Documents.Active.CreateObjectFromDispatch<SwSelObject>(disp);

        private void OnSubmitSelection(int id, object selection, int selType, ref string itemText, ref bool res)
        {
            if (id == this.Id)
            {
                var selObj = ToSelObject(selection);

                if (m_ElementType.IsAssignableFrom(selObj.GetType()))
                {
                    if (m_CustomFilter != null)
                    {
                        var args = new SelectionCustomFilterArguments()
                        {
                            ItemText = itemText,
                            Filter = res
                        };

                        m_CustomFilter.Filter(this, selObj, args);

                        res = args.Filter;
                        itemText = args.ItemText;

                        if (!res && !string.IsNullOrEmpty(args.Reason)) 
                        {
                            SwControl.ShowBubbleTooltip("", args.Reason, "");
                        }
                    }
                }
                else 
                {
                    res = false;
                }
            }
        }

        private void OnSelectionChanged(int id, int count)
        {
            if (Id == id)
            {
                ValueChanged?.Invoke(this, GetSpecificValue());
            }
        }

        protected override object GetSpecificValue()
        {
            var selMgr = m_App.Sw.IActiveDoc2.ISelectionManager;

            if (SupportsMultiEntities)
            {
                var list = Activator.CreateInstance(m_ObjType) as IList;

                for (int i = 0; i < SwSpecificControl.ItemCount; i++)
                {
                    var selIndex = SwSpecificControl.SelectionIndex[i];
                    var obj = selMgr.GetSelectedObject6(selIndex, -1);
                    list.Add(ToSelObject(obj));
                }

                return list;
            }
            else
            {
                Debug.Assert(SwSpecificControl.ItemCount <= 1, "Single entity only");

                if (SwSpecificControl.ItemCount == 1)
                {
                    var selIndex = SwSpecificControl.SelectionIndex[0];
                    var obj = selMgr.GetSelectedObject6(selIndex, -1);
                    return ToSelObject(obj);
                }
                else
                {
                    return null;
                }
            }
        }

        protected override void SetSpecificValue(object value)
        {
            SwSpecificControl.SetSelectionFocus();

            var disps = new List<DispatchWrapper>();

            if (value != null)
            {
                if (SupportsMultiEntities)
                {
                    foreach (SwSelObject item in value as IList)
                    {
                        disps.Add(new DispatchWrapper(item.Dispatch));
                    }
                }
                else
                {
                    disps.Add(new DispatchWrapper((value as SwSelObject).Dispatch));
                }
            }

            var selMgr = m_App.Sw.IActiveDoc2.ISelectionManager;

            var indicesToDeselect = new List<int>();

            for (int i = 0; i < SwSpecificControl.ItemCount; i++)
            {
                var selIndex = SwSpecificControl.SelectionIndex[i];
                var obj = selMgr.GetSelectedObject6(selIndex, -1);

                var objIndex = disps.FindIndex(d => d.WrappedObject == obj);

                if (objIndex == -1)
                {
                    indicesToDeselect.Add(selIndex);
                }
                else 
                {
                    disps.RemoveAt(objIndex);
                }
            }

            if (indicesToDeselect.Any())
            {
                int SUCCESS = 1;

                m_Handler.SuspendSelectionRaise(Id, true);
                
                if (selMgr.DeSelect2(indicesToDeselect.ToArray(), -1) != SUCCESS)
                {
                    //TODO: add log
                }

                m_Handler.SuspendSelectionRaise(Id, false);
            }

            if (disps.Any())
            {
                var selData = selMgr.CreateSelectData();
                selData.Mark = SwSpecificControl.Mark;

                m_App.Sw.IActiveDoc2.Extension.MultiSelect2(disps.ToArray(), true, selData);
            }
        }

        private bool SupportsMultiEntities
        {
            get
            {
                return typeof(IList).IsAssignableFrom(m_ObjType);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_Handler.SelectionChanged -= OnSelectionChanged;
                m_Handler.SubmitSelection -= OnSubmitSelection;
            }
        }
    }
}