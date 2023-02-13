using Microsoft.AspNetCore.Mvc.RazorPages;

public class BaseClass
{
    int Prop => 0;
}
public class SubClass: BaseClass { }   // Noncompliant - not derived from any special base class

public class EmptyPageModel: PageModel // Compliant - an empty PageModel can be fully functional, the C# code can be in the cshtml file
{
}

