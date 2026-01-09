using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Templates
{
    internal static class ControlTemplates
    {
        private const string BASE_PATH = "UI/PropertyPage/Toolkit/Templates/";
        private const string DICT_NAME = "PropertyManagerPageControls.xaml";

        internal static DataTemplate Group { get; }
        internal static DataTemplate ComboBox { get; }
        internal static DataTemplate TextBox { get; }
        internal static DataTemplate TextBlock { get; }
        internal static DataTemplate CheckBox { get; }
        internal static DataTemplate Button { get; }
        internal static DataTemplate Bitmap { get; }
        internal static DataTemplate BitmapButton { get; }
        internal static DataTemplate CheckBoxList { get; }
        internal static DataTemplate CustomControl { get; }
        internal static DataTemplate ListBox { get; }
        internal static DataTemplate NumberBox { get; }
        internal static DataTemplate OptionBox { get; }
        internal static DataTemplate SelectionBox { get; }
        internal static DataTemplate Tab { get; }

        static ControlTemplates() 
        {
            Group = LoadFromResources(BASE_PATH + DICT_NAME, "PropertyManagerPageGroupTemplate");
            ComboBox = LoadFromResources(BASE_PATH + DICT_NAME, "PropertyManagerPageComboBoxTemplate");
            TextBox = LoadFromResources(BASE_PATH + DICT_NAME, "PropertyManagerPageTextBoxTemplate");
            TextBlock = LoadFromResources(BASE_PATH + DICT_NAME, "PropertyManagerPageTextBlockTemplate");
            CheckBox = LoadFromResources(BASE_PATH + DICT_NAME, "PropertyManagerPageCheckBoxTemplate");
            Button = LoadFromResources(BASE_PATH + DICT_NAME, "PropertyManagerPageButtonTemplate");
            Bitmap = LoadFromResources(BASE_PATH + DICT_NAME, "PropertyManagerPageBitmapTemplate");
            Bitmap = LoadFromResources(BASE_PATH + DICT_NAME, "PropertyManagerPageBitmapButtonTemplate");
            CheckBoxList = LoadFromResources(BASE_PATH + DICT_NAME, "PropertyManagerPageCheckBoxListTemplate");
            CustomControl = LoadFromResources(BASE_PATH + DICT_NAME, "PropertyManagerPageCustomControlTemplate");
            ListBox = LoadFromResources(BASE_PATH + DICT_NAME, "PropertyManagerPageListBoxTemplate");
            NumberBox = LoadFromResources(BASE_PATH + DICT_NAME, "PropertyManagerPageNumberBoxTemplate");
            OptionBox = LoadFromResources(BASE_PATH + DICT_NAME, "PropertyManagerPageOptionBoxTemplate");
            SelectionBox = LoadFromResources(BASE_PATH + DICT_NAME, "PropertyManagerPageSelectionBoxTemplate");
            Tab = LoadFromResources(BASE_PATH + DICT_NAME, "PropertyManagerPageTabTemplate");
        }

        private static DataTemplate LoadFromResources(string path, object key)
        {
            var assm = typeof(ControlTemplates).Assembly;

            var assmName = assm.GetName();

            var dictionary = new ResourceDictionary
            {
                Source = new Uri($"pack://application:,,,/{assmName.Name};v{assmName.Version};component/{path}", UriKind.Absolute)
            };

            var template = (DataTemplate)dictionary[key];

            if (template != null)
            {
                return template;
            }
            else 
            {
                throw new NullReferenceException();
            }
        }
    }
}
