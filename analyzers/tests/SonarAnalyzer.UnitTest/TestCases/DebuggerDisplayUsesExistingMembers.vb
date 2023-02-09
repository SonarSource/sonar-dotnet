Imports System
Imports System.Diagnostics

Public Class TestOnPropertiesAndFields
    Const ConstantWithoutInvalidMembers As String = "1"
    Const ConstantWithInvalidMember As String = "{NonExisting}"
    Const ConstantFragment1 As String = "{Non"
    Const ConstantFragment2 As String = "Existing}"

    Property SomeProperty As Integer
    Public SomeField As Integer

    <DebuggerDisplayAttribute("1")> Property WithSuffix As Integer
    <System.Diagnostics.DebuggerDisplay("1")> Property WithNamespace As Integer

    <DebuggerDisplay(Nothing)> Property WithEmptyArgList As Integer
    <DebuggerDisplay("")> Property WithEmptyFormat As Integer
    <DebuggerDisplay(ConstantWithoutInvalidMembers)> Property WithFormatAsConstant1 As Integer
    <DebuggerDisplay(NameOf(ConstantWithoutInvalidMembers))> Property WithFormatAsNameOf As Integer

    <DebuggerDisplay("{SomeProperty}")> Property WithExistingProperty As Integer
    <DebuggerDisplay("{SomeField}")> Property WithExistingField As Integer
    <DebuggerDisplay("{SomeField}")> Property WithExistingFieldVerbatim As Integer
    <DebuggerDisplay("{1 + 1}")> Property WithNoMemberReferenced1 As Integer
    <DebuggerDisplay("{""1"" + ""1""}")> Property WithNoMemberReferenced2 As Integer

    <DebuggerDisplay("{NonExisting}")> Property WithNonExistingMember1 As Integer                          ' Noncompliant {{'NonExisting' doesn't exist in this context.}}
    '                ^^^^^^^^^^^^^^^
    <DebuggerDisplay("1 + {NonExisting}")> Property WithNonExistingMember2 As Integer                      ' Noncompliant {{'NonExisting' doesn't exist in this context.}}
    '                ^^^^^^^^^^^^^^^^^^^
    <DebuggerDisplay("{NonExisting1} bla bla {NonExisting2}")> Property WithMultipleNonExisting As Integer ' Noncompliant {{'NonExisting1' doesn't exist in this context.}}
    '                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    <DebuggerDisplay("{NonExisting}")> Property WithNonExistingMemberVerbatim As Integer                   ' Noncompliant {{'NonExisting' doesn't exist in this context.}}
    '                ^^^^^^^^^^^^^^^

    <DebuggerDisplay(ConstantWithInvalidMember)> Property WithFormatAsConstant2 As Integer                            ' FN: constants are not checked
    <DebuggerDisplay("{Non" & "Existing}")> Property WithFormatAsConcatenationOfLiterals As Integer                   ' FN: only simple literal supported
    <DebuggerDisplay(ConstantFragment1 & ConstantFragment2)> Property WithFormatAsConcatenationOfConstants As Integer ' FN: only simple literal supported

    <DebuggerDisplay("{Me.NonExistingProperty}")> Property PropertyWithExplicitThis As Integer                        ' FN: "Me." not supported (valid when debugging a VB.NET project)
    <DebuggerDisplay("{this.NonExistingField}")> Property FieldWithExplicitThis As Integer                            ' FN: "this." not supported (valid when debugging a C# project)
    <DebuggerDisplay("{1 + NonExistingProperty}")> Property ContainingInvalidMembers As Integer                       ' FN: expressions not supported
End Class

<DebuggerDisplay("{this.ToString()}")>
<DebuggerDisplay("{NonExisting}")> ' Noncompliant {{'NonExisting' doesn't exist in this context.}}
Public Enum TopLevelEnum
    One
    Two
    Three
End Enum

<DebuggerDisplay("{SomeProperty}")>
<DebuggerDisplay("{SomeField}")>
<DebuggerDisplay("{NonExisting}")>      ' Noncompliant {{'NonExisting' doesn't exist in this context.}}
Public Class TestOnNestedTypes
    Property SomeProperty As Integer
    Public SomeField As Integer

    <DebuggerDisplay("{ExistingProperty}")>
    <DebuggerDisplay("{ExistingField}")>
    <DebuggerDisplay("{SomeProperty}")> ' Noncompliant {{'SomeProperty' doesn't exist in this context.}}
    <DebuggerDisplay("{SomeField}")>    ' Noncompliant {{'SomeField' doesn't exist in this context.}}
    Public Class NestedClass
        Property ExistingProperty As Integer
        Property ExistingField As Integer
    End Class

    <DebuggerDisplay("{ExistingProperty}")>
    <DebuggerDisplay("{ExistingField}")>
    <DebuggerDisplay("{SomeProperty}")> ' Noncompliant {{'SomeProperty' doesn't exist in this context.}}
    <DebuggerDisplay("{SomeField}")>    ' Noncompliant {{'SomeField' doesn't exist in this context.}}
    Public Structure NestedStruct
        Property ExistingProperty As Integer
        Property ExistingField As Integer
    End Structure

    Public Enum NestedEnum
        One
        Two
        Three
    End Enum
End Class

Class TestOnDelegates
    Property ExistingProperty As Integer

    <DebuggerDisplay("{ExistingProperty}")> ' Noncompliant
    <DebuggerDisplay("{1}")>
    Delegate Sub Delegate1()
End Class

Class TestOnIndexers
    Property ExistingProperty As Integer
    Public ExistingField As Integer

    <DebuggerDisplay("{ExistingProperty}")>
    <DebuggerDisplay("{ExistingField}")>
    <DebuggerDisplay("{NonExisting}")> ' Noncompliant
    Default Property Item(ByVal i As Integer) As Integer
        Get
            Return 1
        End Get
        Set(value As Integer)
        End Set
    End Property
End Class

<DebuggerDisplay("{SomeProperty}"), DebuggerDisplay("{SomeField}"), DebuggerDisplay("{NonExisting}")> ' Noncompliant
Class TestMultipleAttributes
    '                                                                               ^^^^^^^^^^^^^^^@-1

    Property SomeProperty As Integer
    Public SomeField As Integer = 1

    <DebuggerDisplay("{SomeProperty}"), DebuggerDisplay("{SomeField}"), DebuggerDisplay("{NonExisting}")> Property OtherProperty1 As Integer ' Noncompliant
    '                                                                                   ^^^^^^^^^^^^^^^

    <DebuggerDisplay("{NonExisting1}"), DebuggerDisplay("{NonExisting2}")> Property OtherProperty2 As Integer
    '                ^^^^^^^^^^^^^^^^
    '                                                   ^^^^^^^^^^^^^^^^@-1

    <DebuggerDisplay("{NonExisting1}")> <DebuggerDisplay("{NonExisting2}")> Property OtherProperty3 As Integer
    '                ^^^^^^^^^^^^^^^^
    '                                                    ^^^^^^^^^^^^^^^^@-1
End Class

Class SupportCaseInsensitivity
    Property SomeProperty As Integer = 1

    <DebuggerDisplay("{SOMEPROPERTY}")>
    <DebuggerDisplay("{SomeProperty}")>
    <DebuggerDisplay("{someProperty}")>
    <DebuggerDisplay("{someproperty}")>
    Property OtherProperty As Integer
End Class

Class SupportNonAlphanumericChars
    Property Aa1_뿓 As Integer

    <DebuggerDisplay("{Aa1_뿓}")>
    <DebuggerDisplay("{Aa1_㤬}")> ' Noncompliant {{'Aa1_㤬' doesn't exist in this context.}}
    Property SomeProperty1 As Integer
End Class

Class SupportWhitespaces
    <DebuggerDisplay("{ SomeProperty}")>
    <DebuggerDisplay("{SomeProperty }")>
    <DebuggerDisplay("{" & vbTab & "SomeProperty}")>
    <DebuggerDisplay("{" & vbTab & "SomeProperty" & vbTab & "}")>
    <DebuggerDisplay("{ NonExisting}")>                          ' Noncompliant {{'NonExisting' doesn't exist in this context.}}
    <DebuggerDisplay("{NonExisting }")>                          ' Noncompliant {{'NonExisting' doesn't exist in this context.}}
    <DebuggerDisplay("{" & vbTab & "NonExisting}")>              ' FN: string concatenation not supported
    <DebuggerDisplay("{" & vbTab & "NonExisting" & vbTab & "}")> ' FN: string concatenation not supported
    Property SomeProperty As Integer
End Class

Class SupportNq
    <DebuggerDisplay("{SomeProperty,nq}")>
    <DebuggerDisplay("{SomeProperty ,nq}")>
    <DebuggerDisplay("{SomeProperty, nq}")>
    <DebuggerDisplay("{SomeProperty,nq }")>
    <DebuggerDisplay("{NonExisting,nq}")>  ' Noncompliant
    <DebuggerDisplay("{NonExisting ,nq}")> ' Noncompliant
    <DebuggerDisplay("{NonExisting, nq}")> ' Noncompliant
    <DebuggerDisplay("{NonExisting,nq }")> ' Noncompliant
    Property SomeProperty As Integer
End Class

Class SupportOptionalAttributeParameter
    <DebuggerDisplay("{SomeProperty}", Name:="Any name")>
    <DebuggerDisplay("{NonExisting}", Name:="Any name")>                                                     ' Noncompliant {{'NonExisting' doesn't exist in this context.}}
    <DebuggerDisplay("{NonExisting}", Name:="Any name", Type:=NameOf(SupportOptionalAttributeParameter))>    ' Noncompliant {{'NonExisting' doesn't exist in this context.}}
    <DebuggerDisplay("{NonExisting}", Name:="Any name", Target:=GetType(SupportOptionalAttributeParameter))> ' Noncompliant {{'NonExisting' doesn't exist in this context.}}
    Property SomeProperty As Integer
    '                ^^^^^^^^^^^^^^^@-3
    '                ^^^^^^^^^^^^^^^@-3
    '                ^^^^^^^^^^^^^^^@-3
End Class

Class SupportInheritance
    Class BaseClass
        Property SomeProperty As Integer
    End Class

    Class SubClass
        Inherits BaseClass

        <DebuggerDisplay("{SomeProperty}")> ' Compliant, defined in base class
        Property OtherProperty As Integer
    End Class
End Class

Class SupportAccessModifiers
    Class BaseClass
        Public Property PublicProperty As Integer
        Friend Property InternalProperty As Integer
        Protected Property ProtectedProperty As Integer
        Private Property PrivateProperty As Integer

        <DebuggerDisplay("{PublicProperty}")>
        <DebuggerDisplay("{InternalProperty}")>
        <DebuggerDisplay("{ProtectedProperty}")>
        <DebuggerDisplay("{PrivateProperty}")>
        Property SomeProperty As Integer
    End Class

    Class SubClass
        Inherits BaseClass

        <DebuggerDisplay("{PublicProperty}")>
        <DebuggerDisplay("{InternalProperty}")>
        <DebuggerDisplay("{ProtectedProperty}")>
        <DebuggerDisplay("{PrivateProperty}")>
        Property OtherProperty As Integer
    End Class
End Class

