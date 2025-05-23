﻿' <Your-Product-Name>
' Copyright (c) <Year-From>-<Year-To> <Your-Company-Name>
'
' Please configure this header in your SonarCloud/SonarQube quality profile.
' You can also set it in SonarLint.xml additional file for SonarLint or standalone NuGet analyzer.

Imports System.Runtime.InteropServices

Public Class S3884

    <DllImport("ole32.dll")>
    Public Shared Function CoSetProxyBlanket(<MarshalAs(UnmanagedType.IUnknown)> pProxy As Object, dwAuthnSvc as UInt32, dwAuthzSvc As UInt32, <MarshalAs(UnmanagedType.LPWStr)> pServerPrincName As String, dwAuthnLevel As UInt32, dwImpLevel As UInt32, pAuthInfo As IntPtr, dwCapabilities As UInt32) As Integer
    End Function

    Public Enum RpcAuthnLevel
        [Default] = 0
        None = 1
        Connect = 2
        [Call] = 3
        Pkt = 4
        PktIntegrity = 5
        PktPrivacy = 6
    End Enum

    Public Enum RpcImpLevel
        [Default] = 0
        Anonymous = 1
        Identify = 2
        Impersonate = 3
        [Delegate] = 4
    End Enum

    Public Enum EoAuthnCap
        None = &H00
        MutualAuth = &H01
        StaticCloaking = &H20
        DynamicCloaking = &H40
        AnyAuthority = &H80
        MakeFullSIC = &H100
        [Default] = &H800
        SecureRefs = &H02
        AccessControl = &H04
        AppID = &H08
        Dynamic = &H10
        RequireFullSIC = &H200
        AutoImpersonate = &H400
        NoCustomMarshal = &H2000
        DisableAAA = &H1000
    End Enum

    <DllImport("ole32.dll")>
    Public Shared Function CoInitializeSecurity(pVoid As IntPtr, cAuthSvc As Integer, asAuthSvc As IntPtr, pReserved1 As IntPtr, level As RpcAuthnLevel, impers As RpcImpLevel, pAuthList As IntPtr, dwCapabilities As EoAuthnCap, pReserved3 As IntPtr) As Integer
    End Function

    Public Sub DoSomething()
        Dim Hres1 As Integer = CoSetProxyBlanket(Nothing, 0, 0, Nothing, 0, 0, IntPtr.Zero, 0) ' Noncompliant (S3884)
        '                      ^^^^^^^^^^^^^^^^^
        Dim Hres2 As Integer = CoInitializeSecurity(IntPtr.Zero, -1, IntPtr.Zero, IntPtr.Zero, RpcAuthnLevel.None, RpcImpLevel.Impersonate, IntPtr.Zero, EoAuthnCap.None, IntPtr.Zero) ' Noncompliant
        '                      ^^^^^^^^^^^^^^^^^^^^
    End Sub

End Class
