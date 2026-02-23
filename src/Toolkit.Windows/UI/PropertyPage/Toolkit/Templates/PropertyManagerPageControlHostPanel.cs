using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Controls;

namespace Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Templates
{
    public class PropertyManagerPageControlHostPanel : Panel
    {
        public static System.Windows.Thickness Margin { get; }

        static PropertyManagerPageControlHostPanel() 
        {
            Margin = new System.Windows.Thickness(2);
        }

        private class UIControl
        {
            internal UIElement Element { get; }
            internal Rect Measure { get; }

            internal UIControl(UIElement element, Rect measure)
            {
                Element = element;
                Measure = measure;
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var height = 0d;

            foreach (var row in IterateControlsByRow())
            {
                foreach (var ctrl in row)
                {
                    ctrl.Element.Measure(new Size(availableSize.Width, double.PositiveInfinity));
                }

                height += row.Max(c => c.Element.DesiredSize.Height);
            }

            return new Size(availableSize.Width, height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var y = 0d;

            foreach (var row in IterateControlsByRow())
            {
                var x = 0d;

                if (row.Any())
                {
                    var start = row.First().Measure.Left;

                    if (!double.IsNaN(start)) 
                    {
                        x = start;
                    }
                }

                foreach (var ctrl in row)
                {
                    var autoWidth = (finalSize.Width 
                        - row.Sum(c => !double.IsNaN(c.Measure.Width) ? c.Element.DesiredSize.Width : 0))
                        / row.Count(c => double.IsNaN(c.Measure.Width));

                    var height = ctrl.Element.DesiredSize.Height;
                    
                    var width = !double.IsNaN(ctrl.Measure.Width) ? ctrl.Element.DesiredSize.Width : autoWidth;

                    //var left = ctrl.Measure.Left;
                    //ctrl.Element.Arrange(new Rect(!double.IsNaN(left) ? left : x, y, width, height));

                    ctrl.Element.Arrange(new Rect(x, y, width, height));

                    x += width + Margin.Left;
                }

                y += row.Max(c => c.Element.DesiredSize.Height);
            }

            return finalSize;
        }

        private IReadOnlyList<IReadOnlyList<UIControl>> IterateControlsByRow()
        {
            var rows = new List<List<UIControl>>();
            var customRows = new Dictionary<int, List<UIControl>>();

            foreach (UIElement child in InternalChildren)
            {
                if (child != null)
                {
                    Rect? measure = null;
                    List<UIControl> row;

                    if (child is FrameworkElement elem && elem.DataContext is IWpfPropertyManagerPageControl wpfCtrl)
                    {
                        measure = new Rect()
                        {
                            Width = wpfCtrl.Width.HasValue ? wpfCtrl.Width.Value : double.NaN,
                            Height = wpfCtrl.Height.HasValue ? wpfCtrl.Height.Value : double.NaN,
                            Location = new System.Windows.Point(wpfCtrl.Left.HasValue ? wpfCtrl.Left.Value : double.NaN, wpfCtrl.Top.HasValue ? wpfCtrl.Top.Value : double.NaN)
                        };

                        if (wpfCtrl.Top.HasValue)
                        {
                            var top = Convert.ToInt32(wpfCtrl.Top.Value);

                            if (!customRows.TryGetValue(top, out row))
                            {
                                row = new List<UIControl>();
                                rows.Add(row);
                                customRows.Add(top, row);
                            }
                        }
                        else 
                        {
                            row = new List<UIControl>();
                            rows.Add(row);
                        }
                    }
                    else
                    {
                        row = new List<UIControl>();
                        rows.Add(row);
                    }

                    if (measure == null) 
                    {
                        measure = new Rect()
                        {
                            Width = double.NaN,
                            Height = double.NaN,
                            Location = new System.Windows.Point(double.NaN, double.NaN)
                        };
                    }

                    row.Add(new UIControl(child, measure.Value));
                }
            }

            return rows;
        }
    }
}
