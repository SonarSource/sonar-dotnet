using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Diagnostics.CodeAnalysis;

[Route(@"A\[controller]")]    // Noncompliant {{Replace '\' with '/'.}}
//     ^^^^^^^^^^^^^^^^^
public class BackslashOnControllerUsingVerbatimString : Controller { }

[Route("A\\[controller]")]    // Noncompliant {{Replace '\' with '/'.}}
//     ^^^^^^^^^^^^^^^^^
public class BackslashOnControllerUsingEscapeCharacter : Controller { }

[Route("A\\[controller]\\B")] // Noncompliant {{Replace '\' with '/'.}}
//     ^^^^^^^^^^^^^^^^^^^^
public class MultipleBackslashesOnController : Controller { }

public class BackslashOnActionUsingVerbatimString : Controller
{
    [Route(@"A\[action]")]    // Noncompliant {{Replace '\' with '/'.}}
    //     ^^^^^^^^^^^^^
    public IActionResult Index() => View();
}

public class BackslashOnActionUsingEscapeCharacter : Controller
{
    [Route("A\\[action]")]    // Noncompliant
    //     ^^^^^^^^^^^^^
    public IActionResult Index() => View();
}

public class MultipleBackslashesOnAction : Controller
{
    [Route("A\\[action]\\B")] // Noncompliant
    //     ^^^^^^^^^^^^^^^^
    public IActionResult Index() => View();
}

[Route("\\[controller]")]    // Noncompliant
//     ^^^^^^^^^^^^^^^^
public class RouteOnControllerStartingWithBackslash : Controller { }

public class AController : Controller
{
    //[Route("A\\[action]")]  // Compliant: commented out
    public IActionResult WithoutRouteAttribute() => View();

    [Route("A\\[action]", Name = "a", Order = 3)] // Noncompliant
    public IActionResult WithOptionalAttributeParameters() => View();

    [Route("A/[action]", Name = @"a\b", Order = 3)] // Compliant: backslash is on the name
    public IActionResult WithBackslashInRouteName() => View();

    [RouteAttribute("A\\[action]")] // Noncompliant
    public IActionResult WithAttributeSuffix() => View();

    [Microsoft.AspNetCore.Mvc.RouteAttribute("A\\[action]")] // Noncompliant
    public IActionResult WithFullQualifiedName() => View();

    [Route("A\\[action]")]  // Noncompliant
    [Route("B\\[action]")]  // Noncompliant
    [Route("C/[action]")]   // Compliant: forward slash is used
    public IActionResult WithMultipleRoutes() => View();

    [Route("A%5C[action]")] // Compliant: URL-escaped backslash is used
    public IActionResult WithUrlEscapedBackslash() => View();

    [Route("A/{s:regex(^(?!index\\b)[[a-zA-Z0-9-]]+$)}.html")]
    public IActionResult WithRegexContainingBackslashInLookahead(string s)  => View();  // Compliant: backslash is in regex

    [Route("A/{s:datetime:regex(\\d{{4}}-\\d{{2}}-\\d{{4}})}/B")]
    public IActionResult WithRegexContainingBackslashInMetaEscape(string s)  => View(); // Compliant: backslash is in regex
}

namespace WithAliases
{
    using MyRoute = RouteAttribute;
    using ASP = Microsoft.AspNetCore;

    [MyRoute(@"A\[controller]")]            // Noncompliant
    public class WithAliasedRouteAttribute : Controller { }

    [ASP.Mvc.RouteAttribute("A\\[action]")] // Noncompliant
    public class WithFullQualifiedPartiallyAliasedName : Controller { }
}

namespace WithFakeRouteAttribute
{
    [Route(@"A\[controller]")]      // Compliant: not a real RouteAttribute
    public class AController : Controller { }

    [AttributeUsage(AttributeTargets.Class)]
    public class RouteAttribute : Attribute
    {
        public RouteAttribute(string template) { }
    }
}

class WithUserDefinedSyntaxRouteParameter
{
    const string RouteConst = "Route";

    void Test()
    {
        StringSyntaxWithRouteParameter("\\");                          // Noncompliant
        StringSyntaxWithRouteParameterAfterOtherParameter("\\", "\\"); // Noncompliant
        //                                                      ^^^^
        StringSyntaxWithRouteConstParameter("\\");                     // Noncompliant
        StringSyntaxWithNonRouteParameter("\\");                       // Compliant
        StringSyntaxWithRouteWrongCaseParameter("\\");                 // Compliant
        StringSyntaxWithNullParameter("\\");                           // Compliant
        StringSyntaxWithEmptyStringParameter("\\");                    // Compliant
        MultipleStringSyntaxWithRouteParameterValidFirst("\\");        // Noncompliant
        MultipleStringSyntaxWithRouteParameterInvalidFirst("\\");      // FN: only the first StringSyntax is considered
        NoStringSyntax("\\");                                          // Compliant
    }

    private void StringSyntaxWithRouteParameter([StringSyntax("Route")]string route) { }
    private void StringSyntaxWithRouteParameterAfterOtherParameter(string anotherParameter, [StringSyntax("Route")]string route) { }
    private void StringSyntaxWithRouteConstParameter([StringSyntax(RouteConst)]string route) { }
    private void StringSyntaxWithNonRouteParameter([StringSyntax("NotRoute")]string route) { }
    private void StringSyntaxWithRouteWrongCaseParameter([StringSyntax("route")]string route) { }
    private void StringSyntaxWithNullParameter([StringSyntax(null)]string route) { }
    private void StringSyntaxWithEmptyStringParameter([StringSyntax("")]string route) { }
    private void MultipleStringSyntaxWithRouteParameterValidFirst([StringSyntax("Route")][StringSyntax("route")]string route) { }   // Error [CS0579]
    private void MultipleStringSyntaxWithRouteParameterInvalidFirst([StringSyntax("route")][StringSyntax("Route")]string route) { } // Error [CS0579]
    private void NoStringSyntax(string route) { }
}

class InheritingFromFakeControllerDoesntInfluenceRouteCheck
{
    [NonController]
    [Route(@"A\[controller]")]    // Noncompliant
    public class NotAController : Controller { }

    public class Controller { }
}
