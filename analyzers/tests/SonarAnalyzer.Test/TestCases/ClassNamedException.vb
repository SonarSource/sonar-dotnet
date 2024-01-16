Imports System

Class CustomException                            ' Noncompliant {{Rename this class to remove "Exception" or correct its inheritance.}}
    ' ^^^^^^^^^^^^^^^
End Class

Class ExceptionHandler                           ' Compliant - "Exception" is not at end of the name of the class
End Class

Class SimpleExceptionClass
End Class

Class SimpleClass
End Class

Class SimpleException
    Inherits Exception
End Class

Class OuterClass
    Class InnerException                         ' Noncompliant
    End Class
End Class

Class GenericClassDoesNotExtendException(Of T)   ' Noncompliant
    ' ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
End Class

Class GenericClassExtendsException(Of T)         ' Compliant
    Inherits Exception
End Class

Class SimpleGenericClass(Of T)                   ' Compliant - "Exception" is not in the name of the class
End Class

Interface IEmptyInterfaceException               ' Compliant - interfaces cannot inherit from Exception
End Interface

Structure StructException                        ' Compliant - structures cannot inherit from Exception
End Structure

Enum EnumException                               ' Compliant - enums cannot inherit from Exception
    EnumMember
End Enum

Class ExtendsException                           ' Compliant - direct subclass of Exception
    Inherits Exception
End Class

Class AlsoExtendsIt                              ' Compliant - it'd be better to have "Exception" at the end, but this rule doesn't deal with that
    Inherits Exception
End Class

Class ImplementsAnInterfaceAndExtendsException
    Inherits Exception
    Implements IEmptyInterfaceException
End Class

Class ExtendsNullReferenceException              ' Compliant - indirect subclass of Exception
    Inherits NullReferenceException
End Class

Class ExtendsCustomException                     ' Noncompliant - CustomException is not an Exception subclass
    Inherits CustomException
End Class

Partial Class PartialClassDoesNotExtendException ' Noncompliant
End Class

Partial Class PartialClassExtendsException       ' Compliant - the other part of the class extends Exception
End Class

Partial Class PartialClassExtendsException
    Inherits Exception
End Class

Module StaticException                           ' Noncompliant - the module should be renamed, as it cannot inherit from Exception
End Module

Module                                           ' Error [BC30179,BC30203]
End Module

Class                                            ' Error [BC30179,BC30203]
End Class



