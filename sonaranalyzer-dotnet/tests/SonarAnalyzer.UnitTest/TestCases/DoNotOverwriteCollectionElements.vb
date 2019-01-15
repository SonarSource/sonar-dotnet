Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text

Namespace Tests.TestCases
  Class TestCases

    Public Shared dictionaryField As IDictionary(Of Integer, Integer)
    Public Shared dictionaryField2 As IDictionary(Of Integer, Integer)
    Public fooList() As Foo

    Private Sub SameIndexOnDictionaryItem(ByVal dict As Dictionary(Of Integer, Integer))
      dict.Item(0) = 0 ' Secondary
'     ^^^^^^^^^^^^^^^^
      dict.Item(0) = 1 ' Noncompliant {{Verify this is the index/key that was intended; a value has already been set for it.}}
'     ^^^^^^^^^^^^^^^^
    End Sub

    Private Sub MeUsage
      Me.dictionaryField.Item(0) = 1 ' Secondary
      Me.dictionaryField.Item(0) = 1 ' Noncompliant
    End Sub

    Private Sub MyClassUsage
      MyClass.dictionaryField.Item(0) = 1 ' Secondary
      MyClass.dictionaryField.Item(0) = 1 ' Noncompliant
    End Sub

    Private Sub Parenthesis_Indexer(ByVal array() As Integer)
      array((0)) = 0 ' Secondary
      array(0) = 1 ' Noncompliant
    End Sub

    Private Sub Parenthesis_Invocation(ByVal dict As Dictionary(Of Integer, Integer))
      dict((0)) = 0 ' Secondary
      dict.Add((0), 1) ' Noncompliant
    End Sub

    Private Sub SameIndexOnArray1(ByVal array() As Integer)
      array(0) = 0 ' Secondary
      array(0) = 1 ' Noncompliant
    End Sub

    Private Sub SameIndexOnArray2(ByVal array() As Integer)
      array(0) = 1 ' Secondary
      array(0) = (array(0) + 1) ' Noncompliant
    End Sub

    Private Sub SameIndexOnArray3(ByVal obj As CustomIndexerOneArg)
      obj("foo") = 0
      obj("foo") = 1 ' Compliant, obj is not a collection
    End Sub

    Private Sub SameIndexOnArray4(ByVal obj As CustomIndexerMultiArg)
      obj("s", 1, 1) = 0
      obj("s", 1, 1) = 1 ' Compliant obj is not a dictionary
    End Sub

    Private Sub SameIndexOnList(ByVal list As List(Of Integer))
      list(0) = 0 ' Secondary
      list(0) = 1 ' Noncompliant
    End Sub

    Private Sub SameIndexSpacedOut(ByVal names() As String)
      names("a") = "a" ' Secondary
      names("b") = "b"
      names("a") = "c" ' Noncompliant
    End Sub

    Private Sub NonSequentialAccessOnSameIndex(ByVal values() As Integer)
      Dim index As Integer = 0
      values(0) = 1
      index = (index + 1)
      values(0) = 2 ' FN - We only take consecutive element access
    End Sub

    Private Sub NonConstantConsecutiveIndexAccess(ByVal values() As Integer)
      Dim index As Integer = 0
      values(index) = 1 ' Secondary
      values(index) = 2 ' Noncompliant
    End Sub

    Private Sub IDictionaryAdd(ByVal dict As IDictionary(Of Integer, Integer))
      dict.Add(0, 0) ' Secondary
'     ^^^^^^^^^^^^^^
      dict.Add(0, 1) ' Noncompliant
'     ^^^^^^^^^^^^^^
    End Sub

    Private Sub IDictionaryAddLowercase(ByVal dict As IDictionary(Of Integer, Integer))
      dict.add(0, 0) ' Secondary
'     ^^^^^^^^^^^^^^
      dict.add(0, 1) ' Noncompliant
'     ^^^^^^^^^^^^^^
    End Sub

    Private Sub IDictionaryAddMixedCase(ByVal dict As IDictionary(Of Integer, Integer))
      dict.add(0, 0) ' Secondary
'     ^^^^^^^^^^^^^^
      dict.ADD(0, 1) ' Noncompliant
'     ^^^^^^^^^^^^^^
    End Sub

    Private Sub DictionaryAdd(ByVal dict As Dictionary(Of Integer, Integer))
      dict.Add(0, 0) ' Secondary
      dict.Add(0, 1) ' Noncompliant
    End Sub

    Private Sub IDictionaryAddOnMultiMemberAccess(ByVal c As TestCases)
      ' The below code will throw an ArgumentException at runtime
      c.dictionaryField.Add(0, 0) ' Secondary
      c.dictionaryField.Add(0, 1) ' Noncompliant
    End Sub

    Private Sub DoNotReportOnNonDictionaryAdd(ByVal c As CustomAddItem)
      c.Add(0, 1)
      c.Add(0, 2) ' Compliant this is not on a dictionary
    End Sub


    Private Sub MeWithDifferent()
      Me.dictionaryField(0) = 1 ' Compliant
      Me.dictionaryField2(0) = 1 ' Compliant
    End Sub

    Private Sub MyClassWithDifferent()
      MyClass.dictionaryField(0) = 1 ' Compliant
      MyClass.dictionaryField2(0) = 1 ' Compliant
    End Sub

    Private Sub ArrayFields()
      fooList(0).FooField = 1
      fooList(0).BarField = 1
      AddHandler fooList(0).Ev1, AddressOf Handler
      AddHandler fooList(0).Ev2, AddressOf Handler
    End Sub

    Private Sub Handler()
    End Sub

    Private Sub ConditionalAccess1()
      dictionaryField.Add(0, 1) ' Secondary
      dictionaryField?.Add(0, 1) ' Noncompliant
    End Sub

    Private Sub ConditionalAccess2()
      dictionaryField?.Add(0, 1) ' Secondary
      dictionaryField.Add(0, 1) ' Noncompliant
    End Sub

  End Class

  Class InheritTestCases
    Inherits TestCases

    Private Sub BaseSame()
      MyBase.dictionaryField(0) = 1 ' Secondary
      MyBase.dictionaryField(0) = 1 ' Noncompliant
    End Sub

    Private Sub BaseDifferent()
      MyBase.dictionaryField(0) = 1 ' Compliant
      MyBase.dictionaryField2(0) = 1 ' Compliant
    End Sub

  End Class

  Class Foo
    Public FooField
    Public BarField
    Public Event Ev1()
    Public Event Ev2()
  End Class

  Class CustomIndexerOneArg

    Default Property Item(ByVal key As String) As Integer
      Get
        Return 1
      End Get
      Set

      End Set
    End Property
  End Class
  Class CustomIndexerMultiArg

    Default Property Item(ByVal s As String, ByVal i As Integer, ByVal d As Double) As Integer
      Get
        Return 1
      End Get
      Set

      End Set
    End Property
  End Class
  Class CustomAddItem
    Public Sub Add(ByVal a As Integer, ByVal b As Integer)
    End Sub
  End Class

    'See https://github.com/SonarSource/sonar-dotnet/issues/2235
    Class NullReferenceReproducer
        Sub FooBar()
            Bar ' AD0001 NullReferenceException in GetFirstArgumentExpression
        End Sub

        Public Sub Bar()
        End Sub
    End Class
End Namespace
