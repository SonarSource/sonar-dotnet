using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

public class BaseClass
{
    int Prop => 42;
}
public class SubClass: BaseClass { }    // Noncompliant - not derived from any special base class

public class EmptyPageModel: PageModel  // Compliant - an empty PageModel can be fully functional, the C# code can be in the cshtml file
{
}

public class CustomException: Exception // Compliant - empty exception classes are allowed, the name of the class already provides information
{
}

