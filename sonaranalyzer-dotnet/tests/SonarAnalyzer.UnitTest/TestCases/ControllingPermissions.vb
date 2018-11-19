Imports System
Imports System.IdentityModel.Tokens
Imports System.Security.Permissions
Imports System.Security.Principal
Imports System.Threading
Imports System.Web

Namespace Tests.Diagnostics
    Class Program
        Class MyIdentity
            Implements IIdentity ' Noncompliant {{Make sure that permissions are controlled safely here.}}
'                      ^^^^^^^^^
            Public ReadOnly Property Name As String Implements IIdentity.Name
                Get
                    Throw New NotImplementedException()
                End Get
            End Property

            Public ReadOnly Property AuthenticationType As String Implements IIdentity.AuthenticationType
                Get
                    Throw New NotImplementedException()
                End Get
            End Property

            Public ReadOnly Property IsAuthenticated As Boolean Implements IIdentity.IsAuthenticated
                Get
                    Throw New NotImplementedException()
                End Get
            End Property
        End Class

        Class MyPrincipal
            Implements IPrincipal ' Noncompliant

            Public ReadOnly Property Identity As IIdentity Implements IPrincipal.Identity
                Get
                    Throw New NotImplementedException()
                End Get
            End Property

            Public Function IsInRole(ByVal role As String) As Boolean Implements IPrincipal.IsInRole
                Throw New NotImplementedException()
            End Function
        End Class

        Class MyWindowsIdentity
            Inherits WindowsIdentity ' Noncompliant

            Public Sub New()
                MyBase.New("")
            End Sub
        End Class

        <PrincipalPermission(SecurityAction.Demand, Role:="Administrators")>
        Private Sub SecuredMethod() ' Noncompliant, decorated with PrincipalPermission
'                   ^^^^^^^^^^^^^
        End Sub

        Private Sub ValidateSecurityToken(ByVal handler As SecurityTokenHandler, ByVal securityToken As SecurityToken)
            handler.ValidateToken(securityToken) ' Noncompliant
        End Sub

        Private Sub CreatingPermissions()
            WindowsIdentity.GetCurrent() ' Noncompliant
'           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^

            ' All instantiations of PrincipalPermission
            Dim principalPermission As PrincipalPermission
            principalPermission = New PrincipalPermission(PermissionState.None) ' Noncompliant
            principalPermission = New PrincipalPermission("", "") ' Noncompliant
            principalPermission = New PrincipalPermission("", "", True) ' Noncompliant
        End Sub

        Private Sub HttpContextUser(ByVal httpContext As HttpContext)
            Dim user = httpContext.User ' Noncompliant
            httpContext.User = user ' Noncompliant
        End Sub

        Private Sub AppDomainSecurity(ByVal appDomain As AppDomain, ByVal principal As IPrincipal) ' Noncompliant, IPrincipal parameter, see another section with tests
            appDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal) ' Noncompliant
            appDomain.SetThreadPrincipal(principal) ' Noncompliant
            appDomain.ExecuteAssembly("") ' Compliant, not one of the tracked methods
        End Sub

        Private Sub ThreadSecurity(ByVal principal As IPrincipal) ' Noncompliant, IPrincipal parameter, see another section with tests
            Thread.CurrentPrincipal = principal ' Noncompliant
            principal = Thread.CurrentPrincipal ' Noncompliant
        End Sub

        Private Sub CreatingPrincipalAndIdentity(ByVal windowsIdentity As WindowsIdentity) ' Noncompliant, IIdentity parameter, see another section with tests
            Dim identity As IIdentity
            identity = New MyIdentity() ' Noncompliant, creation of type that implements IIdentity
            identity = New WindowsIdentity("") ' Noncompliant
            Dim principal As IPrincipal
            principal = New MyPrincipal() ' Noncompliant, creation of type that implements IPrincipal
            principal = New WindowsPrincipal(windowsIdentity) ' Noncompliant
        End Sub

        ' Method declarations that accept IIdentity or IPrincipal
        Private Sub AcceptIdentity(ByVal identity As MyIdentity) ' Noncompliant
        End Sub

        Private Sub AcceptIdentity(ByVal identity As IIdentity) ' Noncompliant
        End Sub

        Private Sub AcceptPrincipal(ByVal principal As MyPrincipal) ' Noncompliant
        End Sub

        Private Sub AcceptPrincipal(ByVal principal As IPrincipal) ' Noncompliant
        End Sub
    End Class

    Public Class Properties
        Private id As IIdentity

        Public Property Identity As IIdentity
            Get
                Return id
            End Get
            Set(ByVal value As IIdentity) ' Compliant, we do not raise for property accessors
                id = value
            End Set
        End Property
    End Class

End Namespace
