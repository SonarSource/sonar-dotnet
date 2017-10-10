using System;
using System.Runtime.InteropServices;

namespace Tests.Diagnostics
{
    class Program
    {
        [DllImport("ole32.dll")]
        static extern int CoSetProxyBlanket([MarshalAs(UnmanagedType.IUnknown)]object pProxy, uint dwAuthnSvc,
            uint dwAuthzSvc, [MarshalAs(UnmanagedType.LPWStr)] string pServerPrincName, uint dwAuthnLevel, uint dwImpLevel,
            IntPtr pAuthInfo, uint dwCapabilities);

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

        [DllImport("ole32.dll")]
        public static extern int CoInitializeSecurity(IntPtr pVoid, int cAuthSvc, IntPtr asAuthSvc, IntPtr pReserved1,
            RpcAuthnLevel level, RpcImpLevel impers, IntPtr pAuthList, EoAuthnCap dwCapabilities, IntPtr pReserved3);

        static void Main(string[] args)
        {
            var hres1 = CoSetProxyBlanket(null, 0, 0, null, 0, 0, IntPtr.Zero, 0); // Noncompliant
//                      ^^^^^^^^^^^^^^^^^

            var hres2 = CoInitializeSecurity(IntPtr.Zero, -1, IntPtr.Zero, IntPtr.Zero, RpcAuthnLevel.None, // Noncompliant
//                      ^^^^^^^^^^^^^^^^^^^^
                RpcImpLevel.Impersonate, IntPtr.Zero, EoAuthnCap.None, IntPtr.Zero);
        }
    }
}
