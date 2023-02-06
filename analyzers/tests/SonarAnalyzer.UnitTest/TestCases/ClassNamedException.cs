using System;

class CustomException { }                                        // Noncompliant {{Classes named like "Exception" should extend "Exception" or a subclass}}
//    ^^^^^^^^^^^^^^^
class Customexception { }                                        // Noncompliant
class CustomEXCEPTION { }                                        // Noncompliant

class ExceptionHandler { }                                       // Compliant - "Exception" is not at end of the name of the class
class SimpleClass { }
class SimpleExceptionClass: Exception { }

class OuterClass
{
    class InnerException { }                                     // Noncompliant
}

interface IEmptyInterfaceException { }                           // Compliant - interfaces cannot inherit from Exception
struct StructException { }                                       // Compliant - structs cannot inherit from Exception
enum EnumException { }                                           // Compliant - enums cannot inherit from Exception

class ExtendsException: Exception { }                            // Compliant - direct subclass of Exception
class ImplementsAnInterfaceAndExtendsException: Exception, IEmptyInterfaceException { }
class ExtendsNullReferenceException : NullReferenceException { } // Compliant - indirect subclass of Exception

class ExtendsCustomException: CustomException { }                // Noncompliant - CustomException is not an Exception subclass

partial class PartialClassExtends Exception { }                  // Noncompliant

partial class PartialClassExtendsException { }                   // Compliant - the other part of the class extends Exception
partial class PartialClassExtendsException: Exception { }

static class StaticException { }                                 // Compliant - static classes cannot inherit from Exception
