using Microsoft.AspNetCore.Mvc;
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

class CustomException: Exception { }                                 // Compliant - empty exception classes are allowed, the name of the class already provides information
class CustomAttribute: Attribute { }                                 // Compliant - empty attribute classes are allowed, the name of the class already provides information

class EmptyPageModel: PageModel { }                                  // Compliant - an empty PageModel can be fully functional, the C# code can be in the cshtml file
class CustomActionResult: ActionResult { }                           // Compliant - an empty action result can still provide information by its name

