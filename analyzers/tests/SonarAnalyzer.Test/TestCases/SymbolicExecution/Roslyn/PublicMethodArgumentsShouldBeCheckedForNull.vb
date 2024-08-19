Imports System.Data
Imports System.Text
Imports System.Runtime.CompilerServices

Public Class Program

    Private Field As Object = Nothing
    Private Shared StaticField As Object = Nothing

    Public Sub NotCompliantCases(o As Object, e As Exception)
        Bar(o.ToString())   ' Noncompliant {{Refactor this method to add validation of parameter 'o' before using it.}}
        '   ^
        Bar(o)         ' Compliant, we care about dereference only

        Throw e ' FN - the SE engine uses the Throw statement As branch rather than an operation
    End Sub

    Public Sub Bar(o As Object)
    End Sub

    Protected Sub NotCompliantCases_Protected(o As Object)
        o.ToString() ' Noncompliant
    End Sub

    Private Sub CompliantCases_Private(o As Object)
        o.ToString() ' Compliant - Not accessible from other assemblies
    End Sub

    Protected Friend Sub NotCompliantCases_ProtectedFriend(o As Object)
        o.ToString() ' Noncompliant
    End Sub

    Friend Sub CompliantCases_Friend(o As Object)
        o.ToString() ' Compliant - Not accessible from other assemblies
    End Sub

    Public Sub CompliantCases(b As Boolean, o1 As Object, o2 As Object, o3 As Object, o4 As Object, e As Exception)
        If o1 IsNot Nothing Then o1.ToString()  ' Compliant, we did the check

        o2 = If(o2, New Object())
        o2.ToString()      ' Compliant

        If o3 Is Nothing Then Throw New Exception()
        o3.ToString()                  ' Compliant, we did the check

        If e IsNot Nothing Then Throw e ' Compliant

        o4?.ToString()                 ' Compliant, conditional Operator
        b.ToString()                   ' Compliant, bool cannot be Nothing

        Dim v As Object = Nothing
        v.ToString()                   ' Compliant, we don't care about local variables

        Dim w As Object
        w.ToString()                   ' Compliant, we don't care about local variables

        Field.ToString()               ' Compliant

        Program.StaticField.ToString() ' Compliant
    End Sub

    Public Sub MoreCompliantCases(s1 As String, s2 As String)
        If String.IsNullOrEmpty(s1) Then
            s1.ToString() ' Nothing check was performed, so this belongs To S2259
        Else
            s1.ToString()
        End If
        If String.IsNullOrWhiteSpace(s2) Then
            s2.ToString() ' Nothing check was performed, so this belongs To S2259
        Else
            s2.ToString()
        End If
    End Sub

    Public Sub ForEachLoop(Array As Object())
        For Each o As Object In Array ' Noncompliant
        Next
    End Sub

    Public Async Sub AsyncTest(task1 As Task, task2 As Task, task3 As Task, task4 As Task)
        If task1 IsNot Nothing Then Await task1
        Await task2                                ' Noncompliant
        If task3 IsNot Nothing Then Await task3.ConfigureAwait(False)
        Await task4.ConfigureAwait(False)          ' Noncompliant
    End Sub

    Public Sub New(i As Integer)
    End Sub

    Public Sub New(s As String)
        Me.New(s.Length)     ' Noncompliant {{Refactor this constructor to avoid using members of parameter 's' because it could be Nothing.}}
    End Sub

    Public Sub NonCompliant1(o As Object)
        Dim c As String = o?.ToString()?.IsNormalized()
        If c Is Nothing Then o.GetType().GetMethods()               ' Nothing check was performed, so this belongs To S2259
    End Sub

    Public Sub Compliant1(o As Object)
        Dim c As String = o?.ToString()?.IsNormalized()
        If c IsNot Nothing Then o.GetType().GetMethods()
    End Sub

    Public Sub WithDelegate(asParameter As Object, asVariable As Object)
        Dim SomeAction As Action(Of Func(Of String))

        SomeAction(AddressOf asParameter.ToString)                            ' Noncompliant
        Dim f As Func(Of String) = AddressOf asVariable.ToString   ' Noncompliant
    End Sub

    Public Sub WithStatement(Arg As Object)
        With Arg  'FN
        End With
    End Sub

End Class

<AttributeUsage(AttributeTargets.Parameter, Inherited:=False)>
Public NotInheritable Class ValidatedNotNullAttribute
    Inherits Attribute

End Class

<AttributeUsage(AttributeTargets.Parameter, Inherited:=False)>
Public NotInheritable Class FooAttribute
    Inherits Attribute

End Class

<AttributeUsage(AttributeTargets.Parameter, Inherited:=False)>
Public NotInheritable Class BarAttribute
    Inherits Attribute

End Class

Public Module GuardExtensions

    <Extension>
    Public Sub GuardExtension(Of T As Class)(<ValidatedNotNullAttribute> Value As T, Optional Name As String = "")
        If Value Is Nothing Then Throw New ArgumentNullException(Name)
    End Sub

    <Extension>
    Public Sub GuardExtensionMoreAttributes(Of T As Class)(<FooAttribute, ValidatedNotNullAttribute, BarAttribute> Value As T, Optional Name As String = "")
        If Value Is Nothing Then Throw New ArgumentNullException(Name)
    End Sub

    <Extension>
    Public Sub GuardExtensionMoreArguments(Of T As Class)(<ValidatedNotNullAttribute> Value As T, Foo As String, Bar As String)
        If Value Is Nothing Then Throw New ArgumentNullException()
    End Sub

End Module

Public Class GuardedTests

    Public Sub Guarded(s1 As String, s2 As String, s3 As String, s4 As String, s5 As String, s6 As String, s7 As String)
        Guard1(s1)
        s1.ToUpper()

        Guard2(s2, "s2")
        s2.ToUpper()

        Guard3("s3", s3)
        s3.ToUpper()

        GuardShared(s4)
        s4.ToUpper()

        s5.GuardExtension()
        s5.ToUpper()

        s6.GuardExtensionMoreAttributes()
        s6.ToUpper()

        s7.GuardExtensionMoreArguments(Nothing, Nothing)
        s7.ToUpper()
    End Sub

    Public Sub Guard1(Of T As Class)(<ValidatedNotNullAttribute> Value As T)
    End Sub

    Public Sub Guard2(Of T As Class)(<ValidatedNotNullAttribute> Value As T, Name As String)
    End Sub

    Public Sub Guard3(Of T As Class)(Name As String, <ValidatedNotNullAttribute> Value As T)
    End Sub

    Public Shared Sub GuardShared(Of T As Class)(<ValidatedNotNullAttribute> Value As T)
    End Sub
End Class

Public Class ClassAccessibility

    Private Field As Object

    Public Sub PublicWithoutArgs()
        Field.ToString() ' Compliant - Not a method argument
    End Sub

    Public Sub PublicWithArgs(o As Object)
        o.ToString() ' Noncompliant
    End Sub

    Protected Sub ProtectedWithArgs(o As Object)
        o.ToString() ' Noncompliant
    End Sub

    Protected Friend Sub ProtectedFriendWithArgs(o As Object)
        o.ToString() ' Noncompliant
    End Sub

    Friend Sub FriendWithArgs(o As Object)
        o.ToString() ' Compliant - method Is Not accessible from other assemblies
    End Sub

    Private Sub PrivateWithArgs(o As Object)
        o.ToString()
    End Sub

    Sub ImplicitlyPrivateWithArgs(o As Object)
        o.ToString() ' Noncompliant, implicit means Public in VB
    End Sub

End Class

Public Structure StructAccessibility

    Private Field As Object

    Public Sub PublicWithoutArgs()
        Field.ToString() ' Compliant - Not a method argument
    End Sub

    Public Sub PublicWithArgs(o As Object)
        o.ToString()    ' Noncompliant
    End Sub

    Friend Sub FriendWithArgs(o As Object)
        o.ToString()    ' Compliant - method Is Not accessible from other assemblies
    End Sub

    Sub ImplicitlyFriendWithArgs(o As Object)
        o.ToString()    ' Noncompliant, implicit is Public in VB
    End Sub

    Private Sub PrivateWithArgs(o As Object)
        o.ToString()
    End Sub

    Public Sub LambdasAndAnonymousDelegates()
        MethodAcceptsFunction(Function(obj) obj.ToString())
        MethodAcceptsSub(Sub(obj) obj.ToString())
    End Sub

    Private Sub MethodAcceptsFunction(A As Func(Of Object, String))
    End Sub

    Private Sub MethodAcceptsSub(A As Action(Of Object))
    End Sub

End Structure

Public Class PropertyAccessibility

    Public Property PublicProperty As Object
        Get
        End Get
        Set(Value As Object)
            Dim V As String = Value.ToString    ' Noncompliant
        End Set
    End Property

    Protected Property ProtectedProperty As Object
        Get
        End Get
        Set(Value As Object)
            Dim V As String = Value.ToString    ' Noncompliant
        End Set
    End Property

    Protected Friend Property ProtectedFriendProperty As Object
        Get
        End Get
        Set(Value As Object)
            Dim V As String = Value.ToString    ' Noncompliant
        End Set
    End Property

    Friend Property FriendProperty As Object
        Get
        End Get
        Set(Value As Object)
            Dim V As String = Value.ToString
        End Set
    End Property

    Private Property PrivateProperty As Object
        Get
        End Get
        Set(Value As Object)
            Dim V As String = Value.ToString
        End Set
    End Property

    Property ImplicitlyPrivateProperty As Object
        Get
        End Get
        Set(Value As Object)
            Dim V As String = Value.ToString    ' Noncompliant
        End Set
    End Property

    Public Property ProtectedSetter As Object
        Get
        End Get
        Protected Set(Value As Object)
            Dim V As String = Value.ToString    ' Noncompliant
        End Set
    End Property

    Public Property ProtectedFriendSetter As Object
        Get
        End Get
        Protected Friend Set(Value As Object)
            Dim V As String = Value.ToString    ' Noncompliant
        End Set
    End Property

    Public Property FriendSetter
        Get
        End Get
        Friend Set(Value As Object)
            Dim V As String = Value.ToString    ' Compliant - setter Is Not accessible from other assemblies
        End Set
    End Property

    Public Property PrivateSetter As Object
        Get
        End Get
        Private Set(Value As Object)
            Dim V As String = Value.ToString
        End Set
    End Property

End Class

Public Class ClassWithIndexer

    Default Public Property Item(Index As Object) As Object
        Get
            Return Index.ToString               ' Noncompliant
        End Get
        Set(Value As Object)
            Dim V As String = Value.ToString    ' Noncompliant
        End Set
    End Property

End Class

Public Class ClassWithEvent

    Public Event CustomEvent As EventHandler

    Public Sub Method(c As ClassWithEvent)
        AddHandler c.CustomEvent, Sub(sender, args)                 ' Noncompliant
                                      Console.WriteLine()
                                  End Sub
    End Sub
End Class

Friend Class FriendClass

    Public Sub PublicWithArgs(o As Object)
        o.ToString()                                   ' Compliant - method Is Not accessible from other assemblies
    End Sub

End Class

Class ImplicitlyFriendClass

    Public Sub PublicWithArgs(o As Object)
        o.ToString()
    End Sub

End Class

Public Class NestedClasses

    Public Class PublicNestedClass

        Public Sub Method(o As Object)
            o.ToString()                               ' Noncompliant
        End Sub

        Private Class DeeperNestedPrivateClass

            Public Sub Method(o As Object)
                o.ToString()                           ' Compliant - method Is Not accessible from other assemblies
            End Sub
        End Class
    End Class

    Protected Class ProtectedNestedClass

        Public Sub Method(o As Object)
            o.ToString()                               ' Noncompliant
        End Sub

    End Class

    Private Class PrivateNestedClass

        Public Sub Method(o As Object)
            o.ToString()                               ' Compliant - method Is Not accessible from other assemblies
        End Sub

        Public Class DeeperNestedPublicClass

            Public Sub Method(o As Object)
                o.ToString()                           ' Compliant - method Is Not accessible from other assemblies
            End Sub

        End Class

    End Class

End Class

Public Class Conversion

    Public Sub DownCast(o As Object)
        DirectCast(o, String).ToString()                             ' Noncompliant
    End Sub

    Public Sub UpCast(s As String)
        DirectCast(s, Object).ToString()                             ' Noncompliant
    End Sub

    Public Sub CastWithMemberAccess(o1 As Object, o2 As Object, o3 As Object, o4 As Object)
        Dim V As Object = DirectCast(o1, CustomClass).Prop                          ' Noncompliant
        AddHandler DirectCast(o2, CustomClass).CustomEvent, Function(sender, args)  ' Noncompliant
                                                            End Function
        V = DirectCast(o3, CustomClass).Field                                       ' Noncompliant
        Dim M As Func(Of String) = AddressOf DirectCast(o4, CustomClass).ToString   ' Noncompliant
    End Sub

    Public Sub MultipleCasts(o As Object)
        DirectCast(DirectCast(DirectCast(o, String), Object), String).ToString()    ' Noncompliant
        CType(CType(CType(o, String), Object), String).ToString()                   ' FN
    End Sub


    Public Sub TryCastOperatorDownCast(o As Object)
        TryCast(o, String).ToString()                   ' Noncompliant
    End Sub

    Public Sub TryCastOperatorUpCast(s As String)
        TryCast(s, Object).ToString()                   ' Noncompliant
    End Sub

    Public Sub ForEachLoop(arr As Object(), enumerable As IEnumerable(Of Object))
        For Each o As Object In arr                     ' Noncompliant - the array Is first cast to an IEnumerable, then the GetEnumerator method Is invoked on it
        Next
        For Each o As Object In enumerable              ' Noncompliant
        Next
    End Sub

    Public Sub ForEachLoopWithCast(arr As Object())
        For Each o As Object In DirectCast(arr, IEnumerable(Of Object))         ' Noncompliant
        Next
    End Sub

    Public Sub ForEachLoopWithParentheses(arr As Object())
        For Each o As Object In ((arr))                                         ' FN
        Next
    End Sub

    Public Sub ForEachLoopWithCastAndParentheses(arr As Object())
        For Each o As Object In DirectCast(DirectCast(arr, Object), Object())   ' Noncompliant
        Next
    End Sub

    Private Class CustomClass

        Public Field As Integer
        Public Property Prop As Integer
        Public Event CustomEvent As EventHandler

    End Class

End Class

Public Class Constructor
    Inherits Base

    Public Sub New(s As String)
        Dim v = s.Length                                    ' Noncompliant {{Refactor this method to add validation of parameter 's' before using it.}}
    End Sub

    Public Sub New(o As Object)
        Me.New(o.ToString())                                ' Noncompliant {{Refactor this constructor to avoid using members of parameter 'o' because it could be Nothing.}}
    End Sub

    Public Sub New(o As Object())
        MyBase.New(o.ToString())                            ' Noncompliant {{Refactor this constructor to avoid using members of parameter 'o' because it could be Nothing.}}
    End Sub

    Public Sub New(ByVal l As List(Of Object))
        MyBase.New(If(l.Count > 0, "not empty", "empty"))   ' Noncompliant {{Refactor this constructor to avoid using members of parameter 'l' because it could be Nothing.}}
    End Sub

End Class

Public Class Base

    Public Sub New()
    End Sub

    Public Sub New(s As String)
    End Sub

End Class

Public Class Nancy_Repros

    Public Function IfStartsWith(Arg As String) As String
        If Arg.StartsWith("Value") Then Arg = Arg.Substring(5)  ' Noncompliant
        Return Arg.ToString     ' Compliant - learned NotNull from previous method invocation on Arg
    End Function

    Sub WithCapture(Ex As Exception, Condition As Boolean)
        Dim F As Func(Of Object) = Function() Ex    ' LVA tracks capturing of this parameter
        Dim Value As Object
        Value = Ex.Message   ' FN
        Value = Ex.Message
        Value = If(Condition, Ex.Message, "")
        Value = Ex.Message
    End Sub

End Class

Public Class ShouldExecuteLambdas

    Public Sub Lambdas(Arg As Object)
        Dim SingleLineFunction As Func(Of String) = Function() Arg.ToString
        Dim MultiLineFunction As Func(Of String) = Function()
                                                       Return Arg.ToString
                                                   End Function
        Dim SingleLineAction As Action = Sub() Arg.ToString()
        Dim MultiLineAction As Action = Sub()
                                            Arg.ToString()
                                        End Sub
    End Sub

End Class

Public Class Keywords
    Public Sub Method(ByVal [event] As Object)
        [event].ToString() ' Noncompliant {{Refactor this method to add validation of parameter '[event]' before using it.}}
    End Sub
End Class
