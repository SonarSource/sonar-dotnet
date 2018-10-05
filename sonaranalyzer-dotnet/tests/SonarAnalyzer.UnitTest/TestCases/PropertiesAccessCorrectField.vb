Imports System

Namespace Tests.Diagnostics
    Class NonCompliantClass_FromRspec
        Private x As Integer
        Private y As Integer

        Public Property X As Integer
            Get
                Return x
            End Get
            Set(ByVal value As Integer)
                x = value
            End Set
        End Property

        Public Property Y As Integer
            Get ' Noncompliant {{Refactor this getter so that it actually refers to the field 'y'}}
                Return x
            End Get
            Set(ByVal value As Integer)
                x = value
            End Set
        End Property
    End Class

    Structure NonCompliantStruct_FromRspec
        Private x As Integer
        Private y As Integer

        Public Property X As Integer
            Get
                Return x
            End Get
            Set(ByVal value As Integer)
                x = value
            End Set
        End Property

        Public Property Y As Integer
            Get
                Return x
            End Get
            Set(ByVal value As Integer)
                x = value
            End Set
        End Property
    End Structure

    Class NonCompliant_UnderscoresInNamesAndCasing
        Private yyy As Integer
        Private __x__X As Integer

        Public Property XX As Integer
            Get
                Return yyy
            End Get
            Set(ByVal value As Integer)
                yyy = value
            End Set
        End Property

        Public ReadOnly Property _Y___Y_Y_ As String
            Get
                Return __x__X
            End Get
        End Property
    End Class

    Class NonCompliant_FieldTypeIsIgnored
        Private aaa As Integer
        Private aString As String

        Public Property AAA As String
            Get
                Return aString
            End Get
            Set(ByVal value As String)
                aString = value
            End Set
        End Property
    End Class

    Class NonCompliant_AssigningToExpression
        Private aaa As Integer
        Private aString As String

        Public WriteOnly Property AAA As String
            Set(ByVal value As String)
                aString = "foo" & value
            End Set
        End Property
    End Class

    Partial Class NonCompliant_PartialClass
        Private myProperty As Object
    End Class

    Partial Class NonCompliant_PartialClass
        Private anotherObject As Object
    End Class

    Partial Class NonCompliant_PartialClass
        Public Property MyProperty As Object
            Get
                Return Me.anotherObject
            End Get
            Set(ByVal value As Object)
                Me.anotherObject = value
            End Set
        End Property
    End Class

    Class NonCompliant_ComplexProperty
        Private field1 As Integer
        Private field2 As Integer
        Private initialized As Boolean
        Private isDisposed As Boolean

        Public Property Field1 As Integer
            Get

                If Not Me.initialized Then
                    Throw New InvalidOperationException()
                End If

                If Me.isDisposed Then
                    Throw New ObjectDisposedException("object name")
                End If

                Return Me.field2
            End Get
            Set(ByVal value As Integer)

                If value < 0 Then
                    Throw New ArgumentOutOfRangeException(NameOf(value))
                End If

                Me.field2 = value
            End Set
        End Property
    End Class

    Class NonCompliant_Parentheses
        Private field1 As Integer
        Private field2 As Integer

        Public ReadOnly Property Field2 As Integer
            Get
                Return (((Me.field1)))
            End Get
        End Property
    End Class

    Class NonCompliant_OuterClass
        Private fielda As String
        Private fieldb As String

        Structure NonCompliant_NestedClass
            Private fielda As Integer
            Private fieldb As Integer

            Public Property FieldA As Integer
                Get
                    Return Me.fieldb
                End Get
                Set(ByVal value As Integer)
                    Me.fieldb = value
                End Set
            End Property
        End Structure
    End Class

    Structure Compliant_Indexer
        Private ReadOnly arr As Integer() = New Integer(99) {}

        Default Public Property Item(ByVal i As Integer) As Integer
            Get
                Return arr(i)
            End Get
            Set(ByVal value As Integer)
                arr(i) = value
            End Set
        End Property
    End Structure

    Class CompliantClass
        Private xxx As Integer

        Public Property XXX As Integer
            Get
                Return xxx
            End Get
            Set(ByVal value As Integer)
                xxx = value
            End Set
        End Property

        Public Property UUU As Integer
            Get
                Return xxx
            End Get
            Set(ByVal value As Integer)
                xxx = value
            End Set
        End Property

        Private _a_b_c As String
        Private abc As String
        Private yyy As String

        Public Property Abc As String
            Get
                Return yyy
            End Get
            Set(ByVal value As String)
                yyy = value
            End Set
        End Property
    End Class

    Class Compliant_ImplicitProperties
        Private firstName As String
        Private secondName As String
        Public Property FirstName As String = "Jane"
        Public Property SecondName As String
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
            Get
                wrapped.field1 = value
            End Get
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
                Return field2
            End Get
            Set(ByVal value As Integer)
                field2 = value
            End Set
        End Property
    End Class
End Namespace
