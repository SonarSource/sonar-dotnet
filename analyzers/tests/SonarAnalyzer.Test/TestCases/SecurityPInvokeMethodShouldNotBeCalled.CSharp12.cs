using System;
using System.Runtime.InteropServices;
using IntPtrAliasFromKeywork = nint;

class AliasAnyType
{
    [DllImport("OlE32.dll")]
    static extern int CoSetProxyBlanket(
        [MarshalAs(UnmanagedType.IUnknown)] object pProxy,
        uint dwAuthnSvc,
        uint dwAuthzSvc,
        [MarshalAs(UnmanagedType.LPWStr)] string pServerPrincName,
        uint dwAuthnLevel,
        IntPtrAliasFromKeywork dwImpLevel,
        IntPtrAliasFromKeywork pAuthInfo,
        uint dwCapabilities);

    void Test()
    {
        _ = CoSetProxyBlanket(null, 0, 0, null, 0, 0, 0, 0); // Noncompliant
        //  ^^^^^^^^^^^^^^^^^
    }
}
