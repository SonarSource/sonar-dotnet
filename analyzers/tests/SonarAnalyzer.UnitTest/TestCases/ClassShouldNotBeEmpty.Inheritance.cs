using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

class BaseClass
{
    int Prop => 42;
}
class SubClass: BaseClass { }                                        // Noncompliant - not derived from any special base class

abstract class AbstractBaseWithAbstractMethods
{
    public abstract void AbstractMethod();
}
abstract class AbstractBaseWithoutAbstractMethods
{
    public virtual void DefaultMethod() { }
}
class NoImplementation: AbstractBaseWithAbstractMethods { }          // Error - abstract methods should be implemented
class DefaultImplementation: AbstractBaseWithoutAbstractMethods { }  // Compliant - the class will use the default implementation of DefaultMethod

class EmptyPageModel: PageModel { }                                  // Compliant - an empty PageModel can be fully functional, the C# code can be in the cshtml file
class CustomException: Exception { }                                 // Compliant - empty exception classes are allowed, the name of the class already provides information

