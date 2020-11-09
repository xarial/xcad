https://xcad.net
Check what's new in this version (this may include breaking changes): https://xcad.xarial.com/changelog/

---Creating Add-Ins---

--.NET Framework--

-C#-

[System.Runtime.InteropServices.ComVisible(true)]
public class AddIn : Xarial.XCad.SolidWorks.SwAddInEx
{
    public override void OnConnect()
    {
        Application.ShowMessageBox("Hello xCAD");
    }
}

-VB.NET-

<System.Runtime.InteropServices.ComVisible(True)>
Public Class AddIn
    Inherits Xarial.XCad.SolidWorks.SwAddInEx

    Public Overrides Sub OnConnect()
        Application.ShowMessageBox("Hello xCAD")
    End Sub
End Class

Set the 'Embed Interop Types' option to False for SolidWorks.Interop.sldworks, SolidWorks.Interop.swconst and SolidWorks.Interop.swpublished references

--.NET Core--

-C#-

[System.Runtime.InteropServices.ComVisible(true)]
[System.Runtime.InteropServices.Guid("ADDIN_GUID")]
public class SwAddIn : Xarial.XCad.SolidWorks.SwAddInEx
{
    [System.Runtime.InteropServices.ComRegisterFunction]
    public static void RegisterFunction(Type t)
    {
        SwAddInEx.RegisterFunction(t);
    }

    [System.Runtime.InteropServices.ComUnregisterFunction]
    public static void UnregisterFunction(Type t)
    {
        SwAddInEx.UnregisterFunction(t);
    }

    public override void OnConnect()
    {
        Application.ShowMessageBox("Hello xCAD");
    }
}

Add the <EnableComHosting>true</EnableComHosting> into the <PropertyGroup> of the add-in's .csproj file