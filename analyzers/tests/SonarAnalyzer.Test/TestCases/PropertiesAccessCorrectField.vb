Imports System
Imports System.Collections.Generic
Imports System.Windows
Imports GalaSoft.MvvmLight

Namespace Tests.Diagnostics
    Class NonCompliantClass_FromRspec
        Private x_ As Integer
        Private y_ As Integer

        Public Property X As Integer
            Get
                Return x_
            End Get
            Set(ByVal value As Integer)
                x_ = value
            End Set
        End Property

        Public Property Y As Integer
            Get
                Return x_ ' Noncompliant {{Refactor this getter so that it actually refers to the field 'y_'.}}
            End Get
            Set(ByVal value As Integer)
                x_ = value ' Noncompliant {{Refactor this setter so that it actually refers to the field 'y_'.}}
            End Set
        End Property
    End Class

    Structure NonCompliantStruct_FromRspec
        Private x_ As Integer
        Private y_ As Integer

        Public Property X As Integer
            Get
                Return x_
            End Get
            Set(ByVal value As Integer)
                x_ = value
            End Set
        End Property

        Public Property Y As Integer
            Get
                Return x_ ' Noncompliant
            End Get
            Set(ByVal value As Integer)
                x_ = value ' Noncompliant
            End Set
        End Property
    End Structure

    Class NonCompliant_UnderscoresInNamesAndCasing
        Private yyy As Integer
        Private __x__X As Integer

        Public Property XX As Integer ' test that underscores and casing in names are ignored
            Get
                Return yyy ' Noncompliant {{Refactor this getter so that it actually refers to the field '__x__X'.}}
            End Get
            Set(ByVal value As Integer)
                yyy = value ' Noncompliant {{Refactor this setter so that it actually refers to the field '__x__X'.}}
            End Set
        End Property

        Public ReadOnly Property _Y___Y_Y_ As Integer
            Get
                Return __x__X ' Noncompliant
            End Get
        End Property
    End Class

    Class NonCompliant_FieldTypeIsIgnored
        Private aaa_ As Integer
        Private aString As String

        Public Property AAA As String
            Get
                Return aString ' Compliant - field called 'aaa' exists, but type is different
            End Get
            Set(ByVal value As String)
                aString = value ' Compliant - field called 'aaa' exists, but type is different
            End Set
        End Property
    End Class

    Class NonCompliant_AssigningToExpression
        Private aaa_ As Integer
        Private aString As String

        Public WriteOnly Property AAA As String
            Set(ByVal value As String)
                aString = "foo" & value ' Compliant - field called 'aaa' exists, but type is different
            End Set
        End Property
    End Class

    Partial Class NonCompliant_PartialClass
        Private myProperty_ As Object
    End Class

    Partial Class NonCompliant_PartialClass
        Private anotherObject As Object
    End Class

    Partial Class NonCompliant_PartialClass
        Public Property MyProperty As Object
            Get
                Return Me.anotherObject ' Noncompliant
            End Get
            Set(ByVal value As Object)
                Me.anotherObject = value ' Noncompliant
            End Set
        End Property
    End Class

    Class NonCompliant_ComplexProperty
        Private field1_ As Integer
        Private field2_ As Integer
        Private initialized As Boolean
        Private isDisposed As Boolean

        Public Property Field1 As Integer
            Get ' Noncompliant ^13#3
                If Not Me.initialized Then
                    Throw New InvalidOperationException()
                End If

                If Me.isDisposed Then
                    Throw New ObjectDisposedException("object name")
                End If

                Return Me.field2_
            End Get
            Set(ByVal value As Integer)

                If value < 0 Then
                    Throw New ArgumentOutOfRangeException(NameOf(value))
                End If

                Me.field2_ = value ' Noncompliant
                '  ^^^^^^^
            End Set
        End Property
    End Class

    Class NonCompliant_Parentheses
        Private field1_ As Integer
        Private field2_ As Integer

        Public ReadOnly Property Field2 As Integer
            Get
                Return (((Me.field1_))) ' Noncompliant
                '            ^^^^^^^
            End Get
        End Property
    End Class

    Class NonCompliant_OuterClass
        Private fielda_ As String
        Private fieldb_ As String

        Structure NonCompliant_NestedClass
            Private fielda_ As Integer
            Private fieldb_ As Integer

            Public Property FieldA As Integer
                Get
                    Return Me.fieldb_  ' Noncompliant
                End Get
                Set(ByVal value As Integer)
                    Me.fieldb_ = value ' Noncompliant
                End Set
            End Property
        End Structure
    End Class

    Structure Compliant_Indexer
        ' Declare an array to store the data elements.
        Private Shared ReadOnly arr As Integer() = New Integer(99) {}

        ' Define the indexer to allow client code to use [] notation.
        Default Public Property Item(ByVal i As Integer) As Integer
            Get
                Return arr(i) ' Compliant - we don't know which field to check against
            End Get
            Set(ByVal value As Integer)
                arr(i) = value
            End Set
        End Property
    End Structure

    Class CompliantClass
        Private xxx_ As Integer

        Public Property XXX As Integer
            Get
                Return xxx_
            End Get
            Set(ByVal value As Integer)
                xxx_ = value
            End Set
        End Property

        Public Property UUU As Integer
            Get
                Return xxx_ ' Compliant - no matching field name
            End Get
            Set(ByVal value As Integer)
                xxx_ = value
            End Set
        End Property

        Private _a_b_c As String
        Private abc As String
        Private yyy As String

        Public Property Abc2 As String
            Get
                Return yyy ' Compliant - multiple possible matching field names, so don't raise
            End Get
            Set(ByVal value As String)
                yyy = value
            End Set
        End Property
    End Class

    Class Compliant_ImplicitProperties
        Private firstName As String
        Private secondName As String
        Public Property FirstName2 As String = "Jane"
        Public Property SecondName2 As String
    End Class

    Class WrappedClass
        Friend field1 As Integer
        Friend field2 As Integer
    End Class

    Class Compliant_WrappedObject
        Private wrapped As WrappedClass

        Public Property Field2 As Integer
            Get
                Return wrapped.field1
            End Get
            Set(Value As Integer)
                wrapped.field1 = Value
            End Set
        End Property
    End Class

    Class BaseClass
        Protected field1 As Integer
    End Class

    Class ChildClass
        Inherits BaseClass

        Private field2 As Integer

        Public Property Field1 As Integer
            Get
                Return field2 ' Noncompliant - should point to the field in the base class or change the property name.
            End Get
            Set(ByVal value As Integer)
                field2 = value ' Noncompliant - should point to the field in the base class or change the property name.
            End Set
        End Property
    End Class

    Class MultipleOperations
        Private _foo As Integer
        Private _bar As Integer

        Public Property Foo As Integer
            Get
                Return _foo
            End Get
            Set(ByVal value As Integer)
                _bar = 1
                _foo = value ' Compliant
                _bar = 0
            End Set
        End Property
    End Class

    ' this usage is specific to MVVM Light framework
    Public Class FooViewModel
        Inherits ViewModelBase

        Private _foo As Integer
        Private _bar As Integer

        Public Property Foo As Integer
            Get
                Return Me._foo
            End Get
            Set(ByVal value As Integer)
                If Me.[Set](Me._foo, 1) Then ' Compliant, it is assigned in the Set method
                    Me._bar = 1
                End If
            End Set
        End Property
    End Class

    Public Class FooViewModelWithoutSet
        Inherits ViewModelBase

        Private _foo As Integer
        Private _bar As Integer

        Public Function MySet(ByVal x As Integer, ByVal y As Integer) As Boolean
            Return True
        End Function

        Public Property Foo As Integer
            Get
                Return Me._foo
            End Get
            Set(ByVal value As Integer)
                If MySet(Me._foo, 1) Then
                    Me._bar = 1 ' Noncompliant
                End If
            End Set
        End Property
    End Class

    Public Class MultipleStatements
        Private _foo As String
        Private _bar As String

        Public Property Foo As String
            Get
                If True Then
                    Throw New System.InvalidOperationException("")
                End If

                _foo = "stuff"
                Return _bar ' Noncompliant {{Refactor this getter so that it actually refers to the field '_foo'.}}
                '      ^^^^
            End Get
            Set
                If _foo.Equals(_foo) Then
                    _bar = Value ' Noncompliant ^21#4 {{Refactor this setter so that it actually refers to the field '_foo'.}}
                End If

            End Set
        End Property

        Public Property Bar As String
            Get
                If True Then
                    Throw New System.InvalidOperationException("")
                End If

                Me._bar = "stuff"
                Return Me._foo ' Noncompliant {{Refactor this getter so that it actually refers to the field '_bar'.}}
                '         ^^^^
            End Get
            Set
                If Me._bar.Equals(Me._foo) Then
                    Me._foo = Value ' Noncompliant {{Refactor this setter so that it actually refers to the field '_bar'.}}
                    '  ^^^^
                End If

            End Set
        End Property
    End Class

    Public Class SpecialUsages
        Private _foo As String

        Public ReadOnly Property Foo As String
            Get
                Dim variable = Me._foo ' Compliant, field is read
                If True Then
                    variable = (variable + variable)
                End If
                Return variable
            End Get
        End Property

        Public WriteOnly Property DoNotSet As Integer
            Set
                Throw New System.InvalidOperationException("") ' Compliant
            End Set
        End Property

        Private _tux As String

        Public ReadOnly Property Tux As String
            Get
                Return (_tux + "salt") ' Compliant
            End Get
        End Property

        Private _mux As String

        Public ReadOnly Property Mux As String
            Get
                Return _mux.Replace("x", "y") ' Compliant
            End Get
        End Property
    End Class

    ' https://github.com/SonarSource/sonar-dotnet/issues/2774
    Public Class Class1

        Private WithEvents _RootA As Random
        Private WithEvents _RootB As Random

        Public Property RootA() As Random
            Get
                Return _RootA
            End Get
            Set(ByVal Value As Random)
                _RootA = Value
            End Set
        End Property

        Public Property RootB() As Random
            Get
                Return Me._RootB
            End Get
            Set(ByVal Value As Random)
                Me._RootB = Value
            End Set
        End Property

    End Class

    Public Class CashLess

    End Class

    ' https://github.com/SonarSource/sonar-dotnet/issues/2867
    Public Class Repro_2867

        Public Class ValueWrapper(Of T)

            Public Property Value As T

        End Class

        Private ReadOnly _someMember As New ValueWrapper(Of Double)

        Public Property SomeMember As Double
            Get
                Return _someMember.Value
            End Get
            Set(value As Double)
                _someMember.Value = value
            End Set
        End Property

    End Class

    ' https//github.com/SonarSource/sonar-dotnet/issues/2435
    Public Class CrossProcedural_Repro_2435

        Private _BodyValue As Integer
        Private _BodyValueWrong As Integer
        Private _TooNested As Integer
        Private _TooComplex As Integer

        Public Property BodyValue As Integer
            Get
                Return GetByBody()
            End Get
            Set(value As Integer)
                SetByBody(value)
            End Set
        End Property

        Public Property BodyValue_ As Integer   'With "Me."
            Get
                Return Me.GetByBody()
            End Get
            Set(value As Integer)
                Me.SetByBody(value)
            End Set
        End Property

        Public Property BodyValue__ As Integer 'Get/Set with more than one statement
            Get 'Noncompliant, only one function invocation is supported
                Try
                    IrrelevantFunction()
                    Return GetByBody()
                Catch ex As Exception
                    Return 0
                End Try
            End Get
            Set(value As Integer) 'Noncompliant, only one function invocation is supported
                Try
                    IrrelevantProcedure(value)
                    SetByBody(value)
                Catch ex As Exception
                    'Nothing
                End Try
            End Set
        End Property

        Public Property BodyValueWrong As Integer
            Get                     'Noncompliant
                Return GetByBody()
            End Get
            Set(value As Integer)   'Noncompliant
                SetByBody(value)
            End Set
        End Property

        Private Function IrrelevantFunction() As Integer
            Return 42   'Do not touch local fieds
        End Function

        Private Sub IrrelevantProcedure(Value As Integer)
            'Do not set local fields
        End Sub

        Private Function GetByBody() As Integer
            Return _BodyValue
        End Function

        Private Sub SetByBody(Value As Integer)
            _BodyValue = Value
        End Sub

        Public Property TooNested As Integer
            Get         ' Noncompliant, only one level Of nesting Is supported
                Return GetTooNestedA()
            End Get
            Set(value As Integer)  ' Noncompliant, only one level Of nesting Is supported
                SetTooNestedA(value)
            End Set
        End Property

        Private Function GetTooNestedA() As Integer
            Return GetTooNestedB()
        End Function

        Private Sub SetTooNestedA(Value As Integer)
            SetTooNestedB(Value)
        End Sub

        Private Function GetTooNestedB() As Integer
            Return _TooNested
        End Function

        Private Sub SetTooNestedB(Value As Integer)
            _TooNested = Value
        End Sub

        Public Property TooComplex As Integer
            Get ' Noncompliant, only single return scenario is supported
                If True Then
                    Return GetTooComplex()
                Else
                    Return GetTooComplex()
                End If
            End Get
            Set(Value As Integer) ' Noncompliant, only one function invocation is supported
                If True Then
                    SetTooComplex(Value)
                Else
                    SetTooComplex(Value)
                End If
            End Set
        End Property

        Private Function GetTooComplex() As Integer
            Return _TooComplex
        End Function

        Private Sub SetTooComplex(Value As Integer)
            _TooComplex = Value
        End Sub

    End Class

    ' https://github.com/SonarSource/sonar-dotnet/issues/3441
    Public Class Repro3441
        Private ReadOnly _data As List(Of Integer) = New List(Of Integer)()

        Public Property Data As List(Of Integer)
            Get
                Return _data
            End Get
            Private Set(ByVal value As List(Of Integer)) ' Noncompliant
                For Each item In value
                    _data.Add(item)
                Next
            End Set
        End Property
    End Class

#If NETFRAMEWORK Then ' System.Windows.Controls.Primitives.ButtonBase is in a different assembly in NETCOREAPP
    ' https://github.com/SonarSource/sonar-dotnet/issues/3442
    ' Also see "Add WPF support in unit tests when targeting .Net Core" https://github.com/SonarSource/sonar-dotnet/issues/4883
    Public Class SampleFor3442
        Inherits System.Windows.Controls.Primitives.ButtonBase

        Public Shared ReadOnly IsSpinningProperty As DependencyProperty = DependencyProperty.Register("IsSpinning", GetType(Boolean), GetType(SampleFor3442))

        Public Property IsSpinning As Boolean
            Get
                Return CBool(GetValue(IsSpinningProperty))
            End Get
            Set(ByVal value As Boolean)
                SetValue(IsSpinningProperty, value)
            End Set
        End Property
    End Class
#End If

    Public Class TestCases
        Private pause_ As Boolean

        Public Property Pause As Boolean
            Get
                Return pause_
            End Get
            Set(ByVal value As Boolean)
                pause_ = pause_ Or value
            End Set
        End Property

        Private textBufferUndoHistory_ As Integer

        Public ReadOnly Property TextBufferUndoHistory As Integer
            Get
                Return textBufferUndoHistory_ = GetValue()
            End Get
        End Property

        Private Shared Function GetValue() As Integer
            Return 1
        End Function

        ' https://github.com/SonarSource/sonar-dotnet/issues/5259
        Private attributes_ As Integer()
        Public WriteOnly Property Attributes As Integer()
            Set(ByVal value As Integer()) ' Noncompliant - FP
                value.CopyTo(attributes_, 0)
            End Set
        End Property

        Private Const PREFIX_ As String = "pre"
        Private m_prefix As String

        Public Property Prefix As String
            Get
                Return m_prefix ' Compliant - PREFIX is const
            End Get
            Set(ByVal value As String)
                m_prefix = value
            End Set
        End Property
    End Class

    Public Class ContainsConstraint
        Private _ignoreCase As Boolean

        Public ReadOnly Property IgnoreCase As ContainsConstraint
            Get ' Compliant - _IgnoreCase has different type
                _ignoreCase = True
                Return Me
            End Get
        End Property
    End Class

    Public Class AClass
        Public Shared WRITE_LOCK_TIMEOUT As Long = 1000
        Public longValue As Long

        Public Property WriteLockTimeout As Long
            Get
                Return longValue ' Compliant - WRITE_LOCK_TIMEOUT is shared field when the property is not shared.
            End Get
            Set(ByVal value As Long)
                longValue = value
            End Set
        End Property

        Public Shared TEST_STATIC_CASE As Long = 1000
        Public Shared ALong As Long = 2000

        Public Shared Property TestStaticCase As Long
            Get
                Return ALong ' Noncompliant
            End Get
            Set(ByVal value As Long)
                ALong = value ' Noncompliant
            End Set
        End Property
    End Class

    Public Class A
        Private _chargeId as Integer

        Protected Overridable Property ChargeId As Integer
            Get
                Return _chargeId
            End Get
            Set(value As Integer)
                If (value <> _chargeId)
                    _chargeId = value ' strictly speaking, we're doing other validation, but a scenario where auto properties aren't a solution
                End If
            End Set
        End Property
    End Class

    Public Class B
        Inherits A

        Protected Overrides Property ChargeId As Integer
            Get ' Noncompliant FP
                Return If(true, MyBase.ChargeId, 1234)
            End Get
            Set(value As Integer) ' Noncompliant FP
                MyBase.ChargeId = value
            End Set
        End Property
    End Class

    ' https://github.com/SonarSource/sonar-dotnet/issues/9688
    Public Class Repro_9688
        Private _feedback As String

        Friend Property Feedback As String
            Get
                Return _feedback
            End Get
            Private Set(value As String)
                _feedback &= value ' Compliant
            End Set
        End Property
    End Class
End Namespace
