Imports System

Class CustomException                            ' Noncompliant {{Rename this class to remove "(e|E)xception" or correct its inheritance.}}
    ' ^^^^^^^^^^^^^^^
End Class

Class ExceptionHandler                           ' Compliant - "Exception" is not at end of the name of the class
End Class

Class SimpleClass
End Class

Class SimpleExceptionClass
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

Module StaticException                           ' Compliant - modules cannot inherit from Exception
End Module

