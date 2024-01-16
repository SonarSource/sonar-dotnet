Imports System
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Runtime.Serialization

Class UnamangedException ' Compliant - allows access to System.Exception members from unmanaged code
    Implements System.Runtime.InteropServices._Exception

    Public ReadOnly Property Message As String Implements _Exception.Message
        Get
            Return ""
        End Get
    End Property

    Public ReadOnly Property StackTrace As String Implements _Exception.StackTrace
        Get
            Return ""
        End Get
    End Property

    Public Property HelpLink As String Implements _Exception.HelpLink
        Get
            Return ""
        End Get
        Set(value As String)
        End Set
    End Property

    Public Property Source As String Implements _Exception.Source
        Get
            Return ""
        End Get
        Set(value As String)
        End Set
    End Property

    Public ReadOnly Property InnerException As Exception Implements _Exception.InnerException
        Get
            Return Nothing
        End Get
    End Property

    Public ReadOnly Property TargetSite As MethodBase Implements _Exception.TargetSite
        Get
            Return Nothing
        End Get
    End Property

    Public Sub GetObjectData(info As SerializationInfo, context As StreamingContext) Implements _Exception.GetObjectData
    End Sub

    Public Function GetBaseException() As Exception Implements _Exception.GetBaseException
        Return Nothing
    End Function

    Private Function _Exception_ToString() As String Implements _Exception.ToString
        Return ""
    End Function

    Private Function _Exception_Equals(obj As Object) As Boolean Implements _Exception.Equals
        Return False
    End Function

    Private Function _Exception_GetHashCode() As Integer Implements _Exception.GetHashCode
        Return 0
    End Function

    Private Function _Exception_GetType() As Type Implements _Exception.GetType
        Return Nothing
    End Function
End Class
