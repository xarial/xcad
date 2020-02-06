https://xcad.net

---Creating Add-Ins---

--C#--

[ComVisible(true)]
public class AddIn : Xarial.XCad.SolidWorks.SwAddInEx
{
    public override void OnConnect()
    {
        Application.ShowMessageBox("Hello xCAD");
    }
}

--VB.NET--

<ComVisible(True)>
Public Class AddIn
    Inherits Xarial.XCad.SolidWorks.SwAddInEx

    Public Overrides Sub OnConnect()
        Application.ShowMessageBox("Hello xCAD")
    End Sub
End Class