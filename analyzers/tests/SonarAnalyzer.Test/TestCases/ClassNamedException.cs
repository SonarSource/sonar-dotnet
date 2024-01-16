using System;

class CustomException { }                                        // Noncompliant {{Rename this class to remove "Exception" or correct its inheritance.}}
//    ^^^^^^^^^^^^^^^
class Customexception { }                                        // Noncompliant
class CustomEXCEPTION { }                                        // Noncompliant

class ExceptionHandler { }                                       // Compliant - "Exception" is not at end of the name of the class
class SimpleExceptionClass { }
class SimpleClass { }

class OuterClass
{
    class InnerException { }                                     // Noncompliant
}

class GenericClassDoesNotExtendException<T> { }                  // Noncompliant
//    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
class GenericClassExtendsException<T> : Exception { }            // Compliant
class SimpleGenericClass<T> { }                                  // Compliant - "Exception" is not in the name of the class

interface IEmptyInterfaceException { }                           // Compliant - interfaces cannot inherit from Exception
struct StructException { }                                       // Compliant - structs cannot inherit from Exception
enum EnumException { }                                           // Compliant - enums cannot inherit from Exception

class ExtendsException: Exception { }                            // Compliant - direct subclass of Exception
class AlsoExtendsIt : Exception { }                              // Compliant - it'd be better to have "Exception" at the end, but this rule doesn't deal with that
class ImplementsAnInterfaceAndExtendsException:
    Exception, IEmptyInterfaceException { }
class ExtendsNullReferenceException : NullReferenceException { } // Compliant - indirect subclass of Exception

class ExtendsCustomException: CustomException { }                // Noncompliant - CustomException is not an Exception subclass

partial class PartialClassDoesNotExtendException { }             // Noncompliant

partial class PartialClassExtendsException { }                   // Compliant - the other part of the class extends Exception
partial class PartialClassExtendsException: Exception { }

static class StaticException { }                                 // Noncompliant - the static class should be renamed, as it cannot inherit from Exception

class { }                                                        // Error [CS1001]
