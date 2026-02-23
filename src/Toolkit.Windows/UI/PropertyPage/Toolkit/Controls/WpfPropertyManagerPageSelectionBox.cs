//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Xarial.XCad.Documents;
using Xarial.XCad.Toolkit.PageBuilder.Constructors;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Templates;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Controls
{
    public class WpfPropertyManagerPageSelectionBoxItemNameConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var item = (IXSelObject)values[0];
            var items = (IReadOnlyList<IXSelObject>)values[1];

            string name;

            if (item is IHasName)
            {
                name = ((IHasName)item).Name;
            }
            else if (item == null)
            {
                name = "{Null}";
            }
            else
            {
                name = "";
            }

            if (string.IsNullOrEmpty(name)) 
            {
                if (items != null) 
                {
                    for (int i = 0; i < items.Count; i++) 
                    {
                        if (items[i] != null && item.Equals(items[i])) 
                        {
                            name = $"Entity-{i + 1}";
                            break;
                        }
                    }
                }
            }

            return name;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class WpfPropertyManagerPageSelectionBox : WpfPropertyManagerPageControl<object>
    {
        public override DataTemplate Template { get; }

        public IReadOnlyList<IXSelObject> Items 
        {
            get => m_Items;
            private set 
            {
                m_Items = value;
                this.NotifyPropertyChanged();
            }
        }

        protected override event ControlValueChangedDelegate<object> ValueChanged;

        private readonly IXApplication m_App;
        private readonly WpfPropertyManagerPagePage m_ParentPage;

        private Type m_ObjType;
        private Type m_ElementType;

        private IXSelectionRepository m_CurrentSelection;

        private IReadOnlyList<IXSelObject> m_Items;

        private object m_CurValue;

        public WpfPropertyManagerPageSelectionBox(IGroup parentGroup, IIconsCreator iconConv,
            IXApplication app, IAttributeSet atts, IMetadata[] metadata)
            : base(parentGroup, atts, metadata, iconConv)
        {
            m_ParentPage = parentGroup.FindParentPage();
            m_ParentPage.PageShowing += OnPageShowing;
            m_ParentPage.PageClosed += OnPageClosed;

            m_App = app;
            Template = ControlTemplates.SelectionBox;
        }

        protected override void InitData(IControlOptionsAttribute opts, IAttributeSet atts)
        {
            m_ObjType = atts.ContextType;
            m_ElementType = SelectionBoxConstructorHelper.GetElementType(m_ObjType);
        }

        private void OnPageShowing(WpfPropertyManagerPagePage page)
        {
            m_CurrentSelection = m_App.Documents.Active?.Selections;

            if (m_CurrentSelection != null)
            {
                m_CurrentSelection.NewSelection += OnNewSelection;
                m_CurrentSelection.ClearSelection += OnClearSelection;
            }
        }

        private void OnPageClosed(WpfPropertyManagerPagePage page)
        {
            if (m_CurrentSelection != null)
            {
                m_CurrentSelection.NewSelection -= OnNewSelection;
                m_CurrentSelection.ClearSelection -= OnClearSelection;

                m_CurrentSelection = null;
                m_CurValue = null;
            }
        }

        private bool SupportsMultiEntities => typeof(IList).IsAssignableFrom(m_ObjType);

        private void OnNewSelection(IXDocument doc, IXSelObject selObject)
            => UpdateItems(doc);

        private void OnClearSelection(IXDocument doc)
            => UpdateItems(doc);

        private void UpdateItems(IXDocument doc) 
        {
            var sels = doc.Selections.Where(Filter).ToArray();

            if (SupportsMultiEntities)
            {
                Items = sels;

                var list = Activator.CreateInstance(m_ObjType) as IList;

                foreach (var sel in sels) 
                {
                    list.Add(sel);
                }

                m_CurValue = list;
            }
            else
            {
                if (sels.Any())
                {
                    var sel = sels.Last();

                    Items = new IXSelObject[] { sel };
                    m_CurValue = sel;
                }
                else 
                {
                    Items = Array.Empty<IXSelObject>();
                }
            }

            ValueChanged?.Invoke(this, m_CurValue);
        }

        protected override object GetSpecificValue() => m_CurValue;

        protected override void SetSpecificValue(object value)
        {
            m_CurValue = value;

            if (value != null) 
            {
                if (SupportsMultiEntities)
                {
                    Items = ((IList)value).Cast<IXSelObject>().ToArray();
                }
                else 
                {
                    Items = new IXSelObject[] { (IXSelObject)value };
                }
            }
        }

        private bool Filter(IXSelObject sel)
        {
            return m_ElementType.IsAssignableFrom(sel.GetType());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_ParentPage.PageShowing -= OnPageShowing;

                base.Dispose(disposing);
            }
        }
    }
}