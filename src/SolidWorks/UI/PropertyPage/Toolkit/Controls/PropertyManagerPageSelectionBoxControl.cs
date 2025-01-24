//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
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
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Toolkit.PageBuilder.Constructors;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Services;
using Xarial.XCad.UI.PropertyPage.Structures;
using Xarial.XCad.Utils.PageBuilder.PageElements;
using Xarial.XCad.Base;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Toolkit;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.Geometry;
using Xarial.XCad.Features;
using Xarial.XCad.Sketch;
using Xarial.XCad.Documents;
using Xarial.XCad.Annotations;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Attributes;
using Xarial.XCad.SolidWorks.Utils;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls
{
    //TODO: think of a better way to work with type instead of object (can be either SwSelObject or List). See how combo box is implemented
    internal class PropertyManagerPageSelectionBoxControl : PropertyManagerPageBaseControl<object, IPropertyManagerPageSelectionbox>
    {
        private const int DEFAULT_SEL_HEIGHT = -1;
        private const int DEFAULT_SINGLE_SEL_HEIGHT = 12;
        private const int DEFAULT_MULTI_SEL_HEIGHT = 50;

        protected override event ControlValueChangedDelegate<object> ValueChanged;

        private Type m_ObjType;
        private Type m_ElementType;
        private ISelectionCustomFilter m_CustomFilter;

        private bool m_DefaultFocus;
        private bool m_HasMissingItems;

        private bool m_IsPageActive;

        private object m_CurValue;

        private IXLogger m_Logger;

        private static readonly int[] m_AllFilters;

        static PropertyManagerPageSelectionBoxControl()
        {
            m_AllFilters = Enum.GetValues(typeof(swSelectType_e))
                .Cast<int>().Where(f => f > 0).ToArray();
        }

        public PropertyManagerPageSelectionBoxControl(SwApplication app, IGroup parentGroup, IIconsCreator iconConv,
            IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            : base(app, parentGroup, iconConv, atts, metadata, swPropertyManagerPageControlType_e.swControlType_Selectionbox, ref numberOfUsedIds)
        {
            m_Handler.SelectionChanged += OnSelectionChanged;

            m_Handler.Opened += OnPageOpened;
            m_Handler.PreClosed += OnPageClosed;
            m_Handler.Applied += OnPageApplied;

            m_Handler.SubmitSelection += OnSubmitSelection;
        }

        protected override void InitData(IControlOptionsAttribute opts, IAttributeSet atts)
        {
            m_ObjType = atts.ContextType;
            m_ElementType = SelectionBoxConstructorHelper.GetElementType(m_ObjType);

            m_Logger = m_App.Services.GetService<IXLogger>();
        }

        protected override void SetOptions(IPropertyManagerPageSelectionbox ctrl, IControlOptionsAttribute opts, IAttributeSet atts)
        {
            var isMultiSel = typeof(IList).IsAssignableFrom(atts.ContextType);

            ctrl.SingleEntityOnly = !isMultiSel;

            var height = opts.Height;

            if (height == DEFAULT_SEL_HEIGHT)
            {
                if (isMultiSel)
                {
                    height = DEFAULT_MULTI_SEL_HEIGHT;
                }
                else
                {
                    height = DEFAULT_SINGLE_SEL_HEIGHT;
                }
            }

            ctrl.Height = height;

            var filters = GetDefaultFilters(atts);

            if (atts.Has<SelectionBoxOptionsAttribute>())
            {
                var selAtt = atts.Get<SelectionBoxOptionsAttribute>();

                if (selAtt.Style != 0)
                {
                    ctrl.Style = (int)selAtt.Style;
                }

                if (selAtt.SelectionColor != 0)
                {
                    ctrl.SetSelectionColor(true, (int)selAtt.SelectionColor);
                }

                ctrl.AllowMultipleSelectOfSameEntity = selAtt.AllowDuplicateEntity;
                ctrl.AllowSelectInMultipleBoxes = selAtt.AllowSharedEntity;

                if (selAtt is SwSelectionBoxOptionsAttribute)
                {
                    var swSelAtt = (SwSelectionBoxOptionsAttribute)selAtt;

                    if (swSelAtt.Filters?.Any() == true)
                    {
                        filters = swSelAtt.Filters;
                    }
                }
                else 
                {
                    if (selAtt.Filters?.Any() == true)
                    {
                        filters = selAtt.Filters
                            .SelectMany(f => SwSelectionHelper.GetSelectionType(f) ?? new swSelectType_e[0])
                            .Distinct().ToArray();
                    }
                }

                ctrl.Mark = selAtt.SelectionMark;

                m_DefaultFocus = selAtt.Focused;

                if (selAtt.CustomFilter != null)
                {
                    m_CustomFilter = Activator.CreateInstance(selAtt.CustomFilter) as ISelectionCustomFilter;

                    if (m_CustomFilter == null)
                    {
                        throw new InvalidCastException(
                            $"Specified custom filter of type {selAtt.CustomFilter.FullName} cannot be cast to {typeof(ISelectionCustomFilter).FullName}");
                    }
                }
            }

            if (filters != null && !filters.Contains(swSelectType_e.swSelEVERYTHING))
            {
                ctrl.SetSelectionFilters(filters);
            }
            else
            {
                ctrl.SetSelectionFilters(m_AllFilters);
            }
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

        private swSelectType_e[] GetDefaultFilters(IAttributeSet atts)
        {
            if (atts == null)
            {
                throw new ArgumentNullException(nameof(atts));
            }

            var type = SelectionBoxConstructorHelper.GetElementType(atts.ContextType);

            return SwSelectionHelper.GetSelectionType(type)?.ToArray();
        }

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

        protected override BitmapLabelType_e? GetDefaultBitmapLabel(IAttributeSet atts)
            => SelectionBoxConstructorHelper.GetDefaultBitmapLabel(atts);

        /// <summary>
        /// Resolves the current values based on the selection
        /// </summary>
        /// <returns>List of objects or selection object</returns>
        /// <remarks>This method is called when selection is changed. The value is cached because ItemCount is 0 within the SubmitSelection notification
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
                    foreach (var item in (IList)value)
                    {
                        if (item != null && !(item is IFaultObject))
                        {
                            disps.Add(new DispatchWrapper(((SwSelObject)item).Dispatch));
                        }
                        else
                        {
                            m_HasMissingItems = true;
                        }
                    }
                }
                else
                {
                    if (!(value is IFaultObject))
                    {
                        disps.Add(new DispatchWrapper((value as SwSelObject).Dispatch));
                    }
                    else
                    {
                        value = null;
                        m_HasMissingItems = true;
                    }
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
            m_CurValue = null;
            m_IsPageActive = false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                base.Dispose(disposing);

                m_Handler.SelectionChanged -= OnSelectionChanged;
                m_Handler.SubmitSelection -= OnSubmitSelection;
            }
        }
    }
}