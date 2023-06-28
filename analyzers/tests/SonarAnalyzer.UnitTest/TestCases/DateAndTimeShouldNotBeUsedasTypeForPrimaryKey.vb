Imports System
Imports System.Collections.Generic
Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports DateTimeAlias = System.DateTime
Imports KeyAttributeAlias = System.ComponentModel.DataAnnotations.KeyAttribute

Class TemporalTypes
    Class Entity
        Public Property EntityId As Date              ' Noncompliant {{'Date' should not be used as a type for primary keys}}
        '                           ^^^^
    End Class

    Class DateTimeKey
        Public Property Id As Date                    ' Noncompliant
    End Class

    Class DateTimeOffsetKey
        Public Property Id As DateTimeOffset          ' Noncompliant {{'DateTimeOffset' should not be used as a type for primary keys}}
    End Class

    Class TimeSpanKey
        Public Property Id As TimeSpan                ' Noncompliant {{'TimeSpan' should not be used as a type for primary keys}}
    End Class

    Class DateTimeNoKey
        Public Property Identifier As Date            ' Compliant - only Id and [class-name]Id is recognized as key by Entity Framework
    End Class

    Class DifferentCasingForProprtyName
        Public Property ID As Date                    ' Noncompliant
    End Class

    Class DifferentCasingForProprtyType
        Public Property Id As Date                    ' Noncompliant
    End Class

    Class TemporalTypeWithFullName
        Public Property Id As Date                    ' Noncompliant
    End Class

    Class AliasedTemporalType
        Public Property Id As DateTimeAlias           ' FN - the chance of using a type aliased temporal type as a database key is slim, so we don't cover it to improve performance
    End Class
End Class

Class NonTemporalTypes
    Class IntKey
        Public Property Id As Integer                 ' Compliant - not a temporal type
    End Class

    Class GuidKey
        Public Property Id As Guid
    End Class

    Class StringKey
        Public Property Id As String
    End Class
End Class

Class Attributes
    Class SingleKeyAttribute
        <Key>
        Public Property KeyProperty As Date           ' Noncompliant
    End Class

    Class KeyAndOtherAttributes
        <Key, Column("KeyColumn")>
        Public Property KeyProperty As Date           ' Noncompliant
    End Class

    Class KeyAndOtherAttributeLists
        <Column("KeyColumn")>
        <Key>
        Public Property KeyProperty As Date           ' Noncompliant
    End Class

    Class KeyWithAttributeName
        <KeyAttribute>
        Public Property KeyProperty As Date           ' Noncompliant
    End Class

    Class KeyAttributeWithFullName
        <ComponentModel.DataAnnotations.KeyAttribute>
        Public Property KeyProperty As Date           ' Noncompliant
    End Class

    Class KeyAttributeWithDifferentCasing
        <KEY>
        Public Property KeyProperty As Date           ' Noncompliant
    End Class

    Class AliasedKeyAttribute
        <KeyAttributeAlias>
        Public Property KeyProperty As Date           ' FN - we don't cover aliased attributes to improve the analzyer's performance
    End Class

    Class NoKeyAttribute
        <Column("KeyColumn")>
        Public Property KeyProperty As Date           ' Compliant - not marked with Key attribute and not called Id or [ClassName]Id
    End Class

    Class ForeignKeyRelationship
        Class Author
            <Key>
            Public Property DateOfBirth As Date       ' Noncompliant
            Public Property Books As ICollection(Of Book)
        End Class

        Class Book
            Public Property Title As String
            Public Property Author As Author
            <ForeignKey("Author")>
            Public Property AuthorFK As Date          ' Compliant - only raise where the key is declared
        End Class
    End Class
End Class

Class PropertyTypes
    Class NotProperties
        Public idField As Date                        ' Compliant - only properties are validated
        Public Function Id() As Date
            Return Date.Now
        End Function
    End Class

    Class StaticProperty
        Public Shared Property Id As Date             ' Compliant - shared properties cannot be keys
    End Class

    NotInheritable Class StaticPropertyInStaticClass
        Public Shared Property Id As Date
    End Class

    Class ImplicitPublicProperty
        Property Id As Date                           ' Noncompliant
    End Class

    Class NotPublicProperty
        <Key>
        Friend Property Identifier As Date            ' Compliant - Entity Framework only maps public properties to keys
    End Class

    Class NotReadWriteProperty
        Public ReadOnly Property Id As Date           ' Compliant - not a read/write property
            Get
                Return Date.Now
            End Get
        End Property
    End Class

    Class FullProperty
        Private idField As Date
        Public Property Id As Date                    ' Noncompliant
            Get
                Return idField
            End Get
            Private Set(ByVal value As Date)          ' Note: private setters are supported by Entity Framework (private getters are not)
                idField = value
            End Set
        End Property
    End Class
End Class

Class NonClassTypes
    Structure StructEntity
        Public Property Id As Date                    ' Compliant - struct types cannot be directly mapped to tables in Entity Framework
    End Structure

    Interface IEntity
        Property Id As Date                           ' Compliant - issue will be raised in the implementing class
    End Interface
End Class
