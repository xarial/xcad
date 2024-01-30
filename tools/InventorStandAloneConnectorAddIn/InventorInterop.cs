using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Inventor
{
    [ComImport]
    [TypeLibType(TypeLibTypeFlags.FDispatchable)]
    [Guid("E3571293-DB40-11D2-B783-0060B0F159EF")]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface ApplicationAddInServer
    {
        [DispId(50336260)]
        object Automation
        {
            [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
            [DispId(50336260)]
            [return: MarshalAs(UnmanagedType.IDispatch)]
            get;
        }

        [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
        [DispId(50336257)]
        void Activate([In][MarshalAs(UnmanagedType.Interface)] ApplicationAddInSite AddInSiteObject, [In] bool FirstTime);

        [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
        [DispId(50336258)]
        void Deactivate();

        [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
        [DispId(50336259)]
        void ExecuteCommand(int CommandID);
    }

    [ComImport]
    [TypeLibType(TypeLibTypeFlags.FDispatchable)]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    [Guid("E3571299-DB40-11D2-B783-0060B0F159EF")]
    [DefaultMember("Type")]
    public interface ApplicationAddInSite
    {
        [DispId(50336769)]
        Application Application
        {
            [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
            [DispId(50336769)]
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    [DefaultMember("Type")]
    [Guid("70109AA0-63C1-11D2-B78B-0060B0EC020B")]
    [TypeLibType(TypeLibTypeFlags.FDispatchable)]
    public interface Application
    {
    }
}