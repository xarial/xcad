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
        Application.ShowMessageBox("Hello xCAD (.NET Framework)");
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

Add the following property into the .csproj/.vbproj

<PropertyGroup>
    <EnableComHosting>true</EnableComHosting>
</PropertyGroup>

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
        Application.ShowMessageBox("Hello xCAD (.NET Core)");
    }
}

--.NET 6/.NET 7--

Add the following property into the .csproj/.vbproj

<PropertyGroup>
    <EnableComHosting>true</EnableComHosting>
</PropertyGroup>

-C#-

[System.Runtime.InteropServices.ComVisible(true)]
[System.Runtime.InteropServices.Guid("ADDIN_GUID")]
public class AddIn : Xarial.XCad.SolidWorks.SwAddInEx
{
    public override void OnConnect()
    {
        Application.ShowMessageBox("Hello xCAD (.NET 6/.NET 7)");
    }
}