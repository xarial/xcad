//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
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
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Services;
using Xarial.XCad.UI.PropertyPage.Structures;
using Xarial.XCad.Utils.PageBuilder.PageElements;
using Xarial.XCad.Extensions;
using Xarial.XCad.Base;

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
        private bool m_HasMissingItems;

        private bool m_IsPageActive;

        private object m_CurValue;

        private readonly IXLogger m_Logger;

        public PropertyManagerPageSelectionBoxControl(ISwApplication app, int id, object tag,
            IPropertyManagerPageSelectionbox selBox,
            SwPropertyManagerPageHandler handler, Type objType, ISelectionCustomFilter customFilter, bool defaultFocus,
            IPropertyManagerPageLabel label, IMetadata[] metadata, IXLogger logger)
            : base(selBox, id, tag, handler, label, metadata)
        {
            m_App = app;
            m_ObjType = objType;
            m_ElementType = SelectionBoxConstructorHelper.GetElementType(m_ObjType);
            m_CustomFilter = customFilter;

            m_Logger = logger;

            m_DefaultFocus = defaultFocus;

            m_Handler.SelectionChanged += OnSelectionChanged;

            m_Handler.Opened += OnPageOpened;
            m_Handler.Closed += OnPageClosed;
            m_Handler.Applied += OnPageApplied;

            m_Handler.SubmitSelection += OnSubmitSelection;
        }

        internal IPropertyManagerPageSelectionbox SelectionBox => SwSpecificControl;

        public override void Focus()
        {
            if (Visible)
            {
                SwSpecificControl.SetSelectionFocus();
            }
        }

        private SwSelObject ToSelObject(object disp) => m_App.Documents.Active.CreateObjectFromDispatch<SwSelObject>(disp);

        private void OnSubmitSelection(int id, object selection, int selType, ref string itemText, ref bool res)
        {
            if (id == this.Id)
            {
                if (m_IsPageActive)
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
                else
                {
                    res = true;
                }
            }
        }

        private void OnSelectionChanged(int id, int count)
        {
            if (Id == id)
            {
                m_CurValue = ResolveCurrentValue();
                ValueChanged?.Invoke(this, GetSpecificValue());
            }
        }

        protected override object GetSpecificValue() => m_CurValue;

        /// <summary>
        /// Resolves the current values based on the selection
        /// </summary>
        /// <returns>List of objects or selection object</returns>
        /// <remarks>This methdo is called when selection is changed. The value is cached because ItemCount is 0 within the SubmitSelection notification
        /// which is causing the issue in the custom selection filter as the value returned is empty</remarks>
        private object ResolveCurrentValue()
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
            m_CurValue = value;

            m_HasMissingItems = false;

            SwSpecificControl.SetSelectionFocus();

            var disps = new List<DispatchWrapper>();

            if (value != null)
            {
                if (SupportsMultiEntities)
                {
                    foreach (SwSelObject item in value as IList)
                    {
                        if (item != null)
                        {
                            disps.Add(new DispatchWrapper(item.Dispatch));
                        }
                        else
                        {
                            m_HasMissingItems = true;
                        }
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

                foreach (var indToDeselect in indicesToDeselect.OrderByDescending(i => i))
                {
                    if (selMgr.DeSelect2(indToDeselect, -1) != SUCCESS)
                    {
                        m_Logger.Log($"Failed to deselect item at index {indToDeselect} of selection box ({Id})", LoggerMessageSeverity_e.Error);
                    }
                }
                
                m_Handler.SuspendSelectionRaise(Id, false);
            }

            if (disps.Any())
            {
                var selData = selMgr.CreateSelectData();
                selData.Mark = SwSpecificControl.Mark;

                if (m_App.Sw.IActiveDoc2.Extension.MultiSelect2(disps.ToArray(), true, selData) != disps.Count) 
                {
                    m_Logger.Log($"Failed to select {disps.Count} items");
                }
            }

            if (m_HasMissingItems)
            {
                ProcessMissingItems();
            }
        }

        private void ProcessMissingItems()
        {
            if (m_IsPageActive)//tooltip is not visible until page is opened
            {
                ShowTooltip("Missing Items", "Some of the items were missing and excluded from the selection box");
                m_HasMissingItems = false;

                m_CurValue = ResolveCurrentValue();

                ValueChanged?.Invoke(this, GetSpecificValue());
            }
        }

        private bool SupportsMultiEntities
            => typeof(IList).IsAssignableFrom(m_ObjType);

        private void OnPageOpened()
        {
            m_IsPageActive = true;

            if (m_DefaultFocus)
            {
                Focus();
            }

            if (m_HasMissingItems)
            {
                m_HasMissingItems = false;
                ProcessMissingItems();
            }
        }

        private void OnPageApplied()
        {
            if (m_DefaultFocus)
            {
                Focus();
            }
        }

        private void OnPageClosed(swPropertyManagerPageCloseReasons_e reason)
        {
            m_IsPageActive = false;
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