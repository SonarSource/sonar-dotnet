using System;
using System.Runtime.InteropServices;

namespace Tests.Diagnostics
{
    public enum RpcAuthnLevel
    {
        Default = 0,
        None = 1,
        Connect = 2,
        Call = 3,
        Pkt = 4,
        PktIntegrity = 5,
        PktPrivacy = 6
    }

    public enum RpcImpLevel
    {
        Default = 0,
        Anonymous = 1,
        Identify = 2,
        Impersonate = 3,
        Delegate = 4
    }

    public enum EoAuthnCap
    {
        None = 0x00,
        MutualAuth = 0x01,
        StaticCloaking = 0x20,
        DynamicCloaking = 0x40,
        AnyAuthority = 0x80,
        MakeFullSIC = 0x100,
        Default = 0x800,
        SecureRefs = 0x02,
        AccessControl = 0x04,
        AppID = 0x08,
        Dynamic = 0x10,
        RequireFullSIC = 0x200,
        AutoImpersonate = 0x400,
        NoCustomMarshal = 0x2000,
        DisableAAA = 0x1000
    }

    class Program
    {
        [DllImport("OlE32.dll")]
        static extern int CoSetProxyBlanket([MarshalAs(UnmanagedType.IUnknown)]object pProxy, uint dwAuthnSvc, uint dwAuthzSvc, [MarshalAs(UnmanagedType.LPWStr)] string pServerPrincName, uint dwAuthnLevel, uint dwImpLevel, IntPtr pAuthInfo, uint dwCapabilities);

        [DllImport("ole32", BestFitMapping = false, CallingConvention = CallingConvention.FastCall)]
        public static extern int CoInitializeSecurity(IntPtr pVoid, int cAuthSvc, IntPtr asAuthSvc, IntPtr pReserved1, RpcAuthnLevel level, RpcImpLevel impers, IntPtr pAuthList, EoAuthnCap dwCapabilities, IntPtr pReserved3);

        public static void CoInitializeSecurity(int param) { } // Compliant non extern
        public extern void CoInitializeSecurity(string param); // Compliant non static
        public static extern int CoInitializeSecurity(int param1, string param2); // Compliant no DllImport
        [DllImport] // Error [CS7036]
        public static extern int CoInitializeSecurity(int param1, string param2, string param3); // Compliant DllImport argument invalid
        [DllImport(null)] // Error [CS0591]
        public static extern int CoInitializeSecurity(int param1, string param2, string param3, string param4); // Compliant DllImport argument invalid
        [DllImport(wrongParameterName: "ole32")] // Error [CS1739] the right name is "dllName"
        public static extern int CoInitializeSecurity(int param1, string param2, string param3, string param4, string param5); // Compliant DllImport parameter name wrong
        [DllImport("noOle32")]
        public static extern int CoInitializeSecurity(int param1, string param2, string param3, string param4, string param5, string param6); // Compliant DllImport argument not "ole32"

        static void Main(string[] args)
        {
            var hres1 = CoSetProxyBlanket(null, 0, 0, null, 0, 0, IntPtr.Zero, 0); // Noncompliant
//                      ^^^^^^^^^^^^^^^^^

            var hres2 = CoInitializeSecurity(IntPtr.Zero, -1, IntPtr.Zero, IntPtr.Zero, RpcAuthnLevel.None, RpcImpLevel.Impersonate, IntPtr.Zero, EoAuthnCap.None, IntPtr.Zero); // Noncompliant
//                      ^^^^^^^^^^^^^^^^^^^^

            CoSetProxyBlanket();                             // Error [CS7036]
            CoInitializeSecurity(5);                         // Compliant
            var p = new Program();
            p.CoInitializeSecurity("");                      // Compliant
            CoInitializeSecurity(5, "");                     // Compliant
            CoInitializeSecurity(5, "", "");                 // Compliant
            CoInitializeSecurity(5, "", "", "");             // Compliant
            CoInitializeSecurity(5, "", "", "", "");         // Compliant
            CoInitializeSecurity(5, "", "", "", "", "");     // Compliant
        }
    }

    public class CompliantWithCompilationErrors
    {
        [DllImport(BestFitMapping = false)] // Error [CS7036]
        static extern int CoSetProxyBlanket([MarshalAs(UnmanagedType.IUnknown)] object pProxy, uint dwAuthnSvc, uint dwAuthzSvc, [MarshalAs(UnmanagedType.LPWStr)] string pServerPrincName, uint dwAuthnLevel, uint dwImpLevel, IntPtr pAuthInfo, uint dwCapabilities);

        [DllImport(BestFitMapping = false, EntryPoint = "ole32.dll", ExactSpelling = true)] // Error [CS7036]
        public static extern int CoInitializeSecurity(IntPtr pVoid, int cAuthSvc, IntPtr asAuthSvc, IntPtr pReserved1, RpcAuthnLevel level, RpcImpLevel impers, IntPtr pAuthList, EoAuthnCap dwCapabilities, IntPtr pReserved3);

        public void Somemethod()
        {
            var hres1 = CoSetProxyBlanket(null, 0, 0, null, 0, 0, IntPtr.Zero, 0);

            var hres2 = CoInitializeSecurity(IntPtr.Zero, -1, IntPtr.Zero, IntPtr.Zero, RpcAuthnLevel.None, RpcImpLevel.Impersonate, IntPtr.Zero, EoAuthnCap.None, IntPtr.Zero);
        }
    }
}
