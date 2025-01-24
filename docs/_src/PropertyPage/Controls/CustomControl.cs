using System.ComponentModel;
using Xarial.XCad.Documentation.PropertyPage.Controls;
using Xarial.XCad.UI.PropertyPage.Attributes;

#region WinForms
public class CustomControlWinFormsModel
{
    public string Text { get; set; } = "Custom Control";
}

public class CustomWinFormsControlPage
{
    [CustomControl(typeof(CustomWinFormsControl))]
    public CustomControlWinFormsModel Model { get; set; } = new CustomControlWinFormsModel();
}
#endregion WinForms

#region Wpf
public class CustomControlWpfModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    private string m_Text = "Custom Control";

    public string Text
    {
        get => m_Text;
        set
        {
            m_Text = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
        }
    }
}

public class CustomWpfControlPage
{
    [CustomControl(typeof(CustomWpfControl))]
    public CustomControlWinFormsModel Model { get; set; } = new CustomControlWinFormsModel();
}
#endregion Wpf