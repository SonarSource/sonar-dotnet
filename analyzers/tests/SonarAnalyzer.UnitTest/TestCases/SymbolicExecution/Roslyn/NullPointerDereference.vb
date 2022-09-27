Imports System
Imports System.Threading.Tasks

Public Class Program

    Public Condition As Boolean

    Public Sub RspecNoncompliant()
        Dim O As Object = Nothing
        Console.WriteLine(O.ToString)   ' Noncompliant {{'O' is Nothing on at least one execution path.}}
        '                 ^
    End Sub

    Public Sub RSpecCompliant()
        Dim O As New Object
        Console.WriteLine(O.ToString)
    End Sub

    Public Sub Compliant(Arg As Object)
        Dim i As Integer = Nothing
        i.ToString()    ' Compliant, returns "0"
        Arg.ToString()  ' Compliant, we have no information about Arg
    End Sub

    Public Sub NonInitialized()
        Dim O As Object
        Console.WriteLine(O.ToString)   ' Noncompliant
    End Sub

    Public Sub TestForEach()
        Dim Lst As IEnumerable(Of Integer) = Nothing
        For Each Item In Lst    ' Noncompliant
        Next
    End Sub

    Public Function ElementAccess() As Integer
        Dim Arr() As Integer = Nothing
        Return Arr(42)             ' Noncompliant
    End Function

    Public Sub Conditional(Arg As Object)
        If Arg Is Nothing Then Console.WriteLine("Learn that is can be null")
        Arg.ToString()  'Noncompliant
    End Sub

    Public Sub LearnNotNullFromInvocation(Arg As Object)
        Arg.ToString()  ' Compliant, we learn that it cannot be null
        If Arg Is Nothing Then
            Arg.ToString()    ' Compliant, unreachable
        End If
    End Sub

    Public Function UninitializedArray_NoReDim() As Integer
        Dim Arr As Byte()
        Return Arr.Length ' Noncompliant
    End Function

    Public Function ReDimCompliant() As Integer
        Dim Arr As Byte()
        ReDim Arr(1024)
        Return Arr.Length ' Compliant
    End Function

    Public Sub BitOr(Arg As Integer)
        Dim O As Object
        If O Is Nothing Or Arg = 0 Then
            O.ToString()   ' Noncompliant
        Else
            O.ToString()   ' Compliant
        End If
    End Sub

    Public Async Function TestAwait() As Task
        Dim T As Task
        Await T         ' Noncompliant
    End Function

    Public Sub Linq(Items() As Object)
        Dim Result = From Item In Items Where Item IsNot Nothing
        If Result.Count > 0 Then Result(0).ToString()   ' Compliant
    End Sub

End Class

Public NotInheritable Class ValidatedNotNullAttribute
    Inherits Attribute

End Class

Public Module Guard

    Public Sub CheckNotNull(Of T)(<ValidatedNotNull> Value As T, Name As String)
        If Value Is Nothing Then Throw New ArgumentNullException(Name)
    End Sub

End Module

Public Class Sample

    Public Sub Log(Value As Object)
        CheckNotNull(Value, NameOf(Value))
        If Value Is Nothing Then
            Console.WriteLine(Value.ToString)  ' Compliant, this code is not reachable
        End If
    End Sub

End Class
