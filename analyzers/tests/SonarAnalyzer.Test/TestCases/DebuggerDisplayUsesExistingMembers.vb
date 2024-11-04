Imports System
Imports System.Diagnostics

Public Class TestOnPropertiesAndFields
    Const ConstantWithoutInvalidMembers As String = "Something"
    Const ConstantWithInvalidMember As String = "{Nonexistent}"
    Const ConstantFragment1 As String = "{Non"
    Const ConstantFragment2 As String = "Existent}"

    Public Property SomeProperty As Integer
    Public SomeField As Integer

    <DebuggerDisplayAttribute("Hardcoded text")> Property WithSuffix As Integer
    <System.Diagnostics.DebuggerDisplay("Hardcoded text")> Property WithNamespace As Integer
    <Global.System.Diagnostics.DebuggerDisplay("Hardcoded text")> Property WithGlobal As Integer

    <DebuggerDisplay(Nothing)> Property WithEmptyArgList As Integer
    <DebuggerDisplay("")> Property WithEmptyFormat As Integer
    <DebuggerDisplay(ConstantWithoutInvalidMembers)> Property WithFormatAsConstant1 As Integer
    <DebuggerDisplay(NameOf(ConstantWithoutInvalidMembers))> Property WithFormatAsNameOf As Integer

    <DebuggerDisplay("{SomeProperty}")> Property WithExistingProperty As Integer
    <DebuggerDisplay("{SomeField}")> Property WithExistingField As Integer
    <DebuggerDisplay("{SomeField}")> Property WithExistingFieldVerbatim As Integer
    <DebuggerDisplay("{1 + 1}")> Property WithNoMemberReferenced1 As Integer
    <DebuggerDisplay("{""1"" + ""1""}")> Property WithNoMemberReferenced2 As Integer

    <DebuggerDisplay("{Nonexistent}")> Property WithNonexistentMember1 As Integer                             ' Noncompliant {{'Nonexistent' doesn't exist in this context.}}
    '                ^^^^^^^^^^^^^^^
    <DebuggerDisplay("1 + {Nonexistent}")> Property WithNonexistentMember2 As Integer                         ' Noncompliant {{'Nonexistent' doesn't exist in this context.}}
    '                ^^^^^^^^^^^^^^^^^^^
    <DebuggerDisplay("{Nonexistent1} bla bla {Nonexistent2}")> Property WithMultipleNonexistent As Integer    ' Noncompliant {{'Nonexistent1' doesn't exist in this context.}}
    '                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    <DebuggerDisplay("{Nonexistent}")> Property WithNonexistentMemberVerbatim As Integer                      ' Noncompliant {{'Nonexistent' doesn't exist in this context.}}
    '                ^^^^^^^^^^^^^^^
    <System.Diagnostics.DebuggerDisplay("{Nonexistent}")> Property WithNamespaceAndNonexistent As Integer     ' Noncompliant {{'Nonexistent' doesn't exist in this context.}}
    '                                   ^^^^^^^^^^^^^^^
    <Global.System.Diagnostics.DebuggerDisplay("{Nonexistent}")> Property WithGlobalAndNonexistent As Integer ' Noncompliant {{'Nonexistent' doesn't exist in this context.}}
    '                                          ^^^^^^^^^^^^^^^

    <DebuggerDisplay(ConstantWithInvalidMember)> Property WithFormatAsConstant2 As Integer                            ' Noncompliant
    <DebuggerDisplay("{Non" & "Existing}")> Property WithFormatAsConcatenationOfLiterals As Integer                   ' Noncompliant
    <DebuggerDisplay(ConstantFragment1 & ConstantFragment2)> Property WithFormatAsConcatenationOfConstants As Integer ' Noncompliant

    <DebuggerDisplay("{Me.NonexistentProperty}")> Property PropertyWithExplicitThis As Integer                        ' FN: "Me." not supported (valid when debugging a VB.NET project)
    <DebuggerDisplay("{this.NonexistentField}")> Property FieldWithExplicitThis As Integer                            ' FN: "this." not supported (valid when debugging a C# project)
    <DebuggerDisplay("{1 + NonexistentProperty}")> Property ContainingInvalidMembers As Integer                       ' FN: expressions not supported
End Class

<DebuggerDisplay("{Me.ToString()}")>    ' Compliant, valid when debugging a VB.NET project
<DebuggerDisplay("{this.ToString()}")>  ' Compliant, valid when debugging a C# project
<DebuggerDisplay("{Nonexistent}")>      ' Noncompliant
Public Enum TopLevelEnum
    One
    Two
    Three
End Enum

<DebuggerDisplay("{SomeProperty}")>
<DebuggerDisplay("{SomeField}")>
<DebuggerDisplay("{Nonexistent}")>      ' Noncompliant
Public Class TestOnNestedTypes
    Public Property SomeProperty As Integer
    Public SomeField As Integer

    <DebuggerDisplay("{ExistingProperty}")>
    <DebuggerDisplay("{ExistingField}")>
    <DebuggerDisplay("{SomeProperty}")> ' Noncompliant
    <DebuggerDisplay("{SomeField}")>    ' Noncompliant
    Public Class NestedClass
        Property ExistingProperty As Integer
        Property ExistingField As Integer
    End Class

    <DebuggerDisplay("{ExistingProperty}")>
    <DebuggerDisplay("{ExistingField}")>
    <DebuggerDisplay("{SomeProperty}")> ' Noncompliant
    <DebuggerDisplay("{SomeField}")>    ' Noncompliant
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
    <DebuggerDisplay("{42}")>
    Delegate Sub Delegate1()
End Class

Class TestOnIndexers
    Public Property ExistingProperty As Integer
    Public ExistingField As Integer

    <DebuggerDisplay("{ExistingProperty}")>
    <DebuggerDisplay("{ExistingField}")>
    <DebuggerDisplay("{Nonexistent}")> ' Noncompliant
    Default Property Item(ByVal i As Integer) As Integer
        Get
            Return 1
        End Get
        Set(value As Integer)
        End Set
    End Property
End Class

<DebuggerDisplay("{SomeProperty}"), DebuggerDisplay("{SomeField}"), DebuggerDisplay("{Nonexistent}")> ' Noncompliant
Class TestMultipleAttributes
    '                                                                               ^^^^^^^^^^^^^^^@-1

    Public Property SomeProperty As Integer
    Public SomeField As Integer = 1

    <DebuggerDisplay("{SomeProperty}"), DebuggerDisplay("{SomeField}"), DebuggerDisplay("{Nonexistent}")> Property OtherProperty1 As Integer ' Noncompliant
    '                                                                                   ^^^^^^^^^^^^^^^

    <DebuggerDisplay("{Nonexistent1}"), DebuggerDisplay("{Nonexistent2}")> Property OtherProperty2 As Integer
    '                ^^^^^^^^^^^^^^^^
    '                                                   ^^^^^^^^^^^^^^^^@-1

    <DebuggerDisplay("{Nonexistent1}")> <DebuggerDisplay("{Nonexistent2}")> Property OtherProperty3 As Integer
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
    <DebuggerDisplay("{ Nonexistent}")>                          ' Noncompliant {{'Nonexistent' doesn't exist in this context.}}
    <DebuggerDisplay("{Nonexistent }")>                          ' Noncompliant {{'Nonexistent' doesn't exist in this context.}}
    <DebuggerDisplay("{" & vbTab & "Nonexistent}")>              ' Noncompliant
    <DebuggerDisplay("{" & vbTab & "Nonexistent" & vbTab & "}")> ' Noncompliant
    Property SomeProperty As Integer
End Class

Class SupportNq
    <DebuggerDisplay("{SomeProperty,nq}")>
    <DebuggerDisplay("{SomeProperty ,nq}")>
    <DebuggerDisplay("{SomeProperty, nq}")>
    <DebuggerDisplay("{SomeProperty,nq }")>
    <DebuggerDisplay("{Nonexistent,nq}")>  ' Noncompliant
    <DebuggerDisplay("{Nonexistent ,nq}")> ' Noncompliant
    <DebuggerDisplay("{Nonexistent, nq}")> ' Noncompliant
    <DebuggerDisplay("{Nonexistent,nq }")> ' Noncompliant
    Property SomeProperty As Integer
End Class

Class SupportOptionalAttributeParameter
    <DebuggerDisplay("{SomeProperty}", Name:="Any name")>
    <DebuggerDisplay("{Nonexistent}", Name:="Any name")>                                                     ' Noncompliant
    <DebuggerDisplay("{Nonexistent}", Name:="Any name", Type:=NameOf(SupportOptionalAttributeParameter))>    ' Noncompliant
    <DebuggerDisplay("{Nonexistent}", Name:="Any name", Target:=GetType(SupportOptionalAttributeParameter))> ' Noncompliant
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

