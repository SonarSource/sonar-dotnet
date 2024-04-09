Imports System
Imports System.Runtime.CompilerServices
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

    Public Sub IsNothing_Known()
        Dim O As Object
        If IsNothing(O) Then
            O.ToString()    ' Noncompliant
        Else
            O.ToString()    ' Compliant
        End If
    End Sub

    Public Sub IsNothing_Unknown(Arg As Object)
        Dim B As Boolean = IsNothing(Arg)   ' Learn possible null
        Arg.ToString()  ' Noncompliant
    End Sub

    Private Sub Test_NullableValueTypes(Of T As Structure)(ByVal arg As T?)
        Dim i As Integer? = Nothing
        i.GetType()             ' Noncompliant
        i = Nothing
        i.GetType()             ' Noncompliant
        i = 42
        i.GetType()             ' Compliant
        i = Nothing
        Dim X = i.HasValue      ' Compliant - safe to call
        i = Nothing
        X = i.Value             ' Compliant - handled by rule S3655
        i = Nothing
        X = CInt(i)             ' Compliant - handled by rule S3655
        i = Nothing
        i.GetValueOrDefault()   ' Compliant - safe to call
        i = Nothing
        i.Equals(Nothing)       ' Compliant - safe to call
        i = Nothing
        i.GetHashCode()         ' Compliant - safe to call
        i = Nothing
        i.ToString()            ' Compliant - safe to call

        arg.GetType()           ' Compliant
        arg = Nothing
        arg.GetType()           ' Noncompliant
        Dim localNotNull As T? = New T()
        localNotNull.GetType()  ' Compliant
        Dim localNull As T? = Nothing
        localNull.GetType()     ' Noncompliant
    End Sub

    Class HasGenericNullable(Of T As Structure)
        Public Property Prop As T?

        Public Sub M()
            Prop = Nothing
            Dim X = Prop.HasValue   ' Compliant
            Prop = Nothing
            Prop.GetType()          ' Noncompliant
        End Sub
    End Class

End Class

Public NotInheritable Class ValidatedNotNullAttribute
    Inherits Attribute

End Class

Public Module Guard

    Public Sub CheckNotNull(Of T)(<ValidatedNotNull> Value As T, Name As String)
        If Value Is Nothing Then Throw New ArgumentNullException(Name)
    End Sub

    <Extension>
    Public Sub CheckNotNullExtension(Of T)(<ValidatedNotNull> Value As T, Name As String)
        If Value Is Nothing Then Throw New ArgumentNullException(Name)
    End Sub

End Module

Public Class Sample

    Public Sub Log(Value As Object)
        CheckNotNull(Value, "Value")
        If Value Is Nothing Then
            Console.WriteLine(Value.ToString)  ' Compliant, this code is not reachable
        End If
    End Sub

    Public Sub LogExtension(Value As String)
        Value.CheckNotNullExtension("Value")
        If Value Is Nothing Then
            Console.WriteLine(Value.ToString)  ' Compliant, this code is not reachable
        End If
    End Sub

    Public Sub LogExtension(Value As Object)
        Value.CheckNotNullExtension("Value")    ' Extension invocation on Object type is DynamicInvocationOperation
        If Value Is Nothing Then
            Console.WriteLine(Value.ToString)   ' Noncompliant FP, this code is not reachable
        End If
    End Sub

End Class

' https://github.com/SonarSource/sonar-dotnet/issues/6271
Public Class Repro_6271

    Public Function TestDataContains(Text As String) As Boolean
        Static Data As String = LoadTestData()
        Return Data.Contains(Text)  ' Compliant
    End Function

    Private Shared Function LoadTestData() As String
        Return "Lorem ipsum"
    End Function

End Class
