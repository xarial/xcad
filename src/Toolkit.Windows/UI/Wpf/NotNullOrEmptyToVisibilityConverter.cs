using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Xarial.XCad.Toolkit.Windows.UI.Wpf
{
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class NotNullOrEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isNullOrEmpty;

            if (value != null)
            {
                switch (value)
                {
                    case string str:
                        isNullOrEmpty = string.IsNullOrWhiteSpace(str);
                        break;

                    case IEnumerable enumer:
                        isNullOrEmpty = !enumer.GetEnumerator().MoveNext();
                        break;

                    default:
                        isNullOrEmpty = false;
                        break;
                }
            }
            else 
            {
                isNullOrEmpty = true;
            }

            return !isNullOrEmpty ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
