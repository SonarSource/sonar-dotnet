using System.Web.Mvc;
using System;

[Route(@"A\[controller]")]    // Noncompliant {{Replace '\' with '/'.}}
//     ^^^^^^^^^^^^^^^^^
public class BackslashOnControllerUsingVerbatimStringController : Controller { }

[Route("A\\[controller]")]    // Noncompliant {{Replace '\' with '/'.}}
//     ^^^^^^^^^^^^^^^^^
public class BackslashOnControllerUsingEscapeCharacterController : Controller { }

[Route("A\\[controller]\\B")] // Noncompliant {{Replace '\' with '/'.}}
//     ^^^^^^^^^^^^^^^^^^^^
public class MultipleBackslashesOnController : Controller { }

public class BackslashOnActionUsingVerbatimStringController : Controller
{
    [Route(@"A\[action]")]    // Noncompliant {{Replace '\' with '/'.}}
    //     ^^^^^^^^^^^^^
    public ActionResult Index() => View();
}

public class BackslashOnActionUsingEscapeCharacterController : Controller
{
    [Route("A\\[action]")]    // Noncompliant
    //     ^^^^^^^^^^^^^
    public ActionResult Index() => View();
}

public class MultipleBackslashesOnActionController : Controller
{
    [Route("A\\[action]\\B")] // Noncompliant
    //     ^^^^^^^^^^^^^^^^
    public ActionResult Index() => View();
}

[Route("\\[controller]")]    // Noncompliant
//     ^^^^^^^^^^^^^^^^
public class RouteOnControllerStartingWithBackslashController : Controller { }

public class AController : Controller
{
    //[Route("A\\[action]")]  // Compliant: commented out
    public ActionResult WithoutRouteAttribute() => View();

    [Route("A\\[action]", Name = "a", Order = 3)] // Noncompliant
    public ActionResult WithOptionalAttributeParameters() => View();

    [RouteAttribute("A\\[action]")] // Noncompliant
    public ActionResult WithAttributeSuffix() => View();

    [System.Web.Mvc.RouteAttribute("A\\[action]")] // Noncompliant
    public ActionResult WithFullQualifiedName() => View();

    [Route("A\\[action]")]  // Noncompliant
    [Route("B\\[action]")]  // Noncompliant
    [Route("C/[action]")]   // Compliant: forward slash is used
    public ActionResult WithMultipleRoutes() => View();

    [Route("A%5C[action]")] // Compliant: URL-escaped backslash is used
    public ActionResult WithUrlEscapedBackslash() => View();

    [Route("A/{s:regex(^(?!index\\b)[[a-zA-Z0-9-]]+$)}.html")]                 // Compliant: backslash is in regex
    public ActionResult WithRegexContainingBackslashInLookahead()  => View();

    [Route("A/{s:datetime:regex(\\d{{4}}-\\d{{2}}-\\d{{4}})}/B")]              // Compliant: backslash is in regex
    public ActionResult WithRegexContainingBackslashInMetaEscape() => View();

    [Route("A\\{s:regex(.*)\\[action]")]                                       // Noncompliant
    public ActionResult WithFirstBackslashBeforeFirstRegex() => View();

    [Route("{s:regex(.*)\\[action]")]                                          // FN: the backslash is not in the regex
    public ActionResult WithFirstBackslashAfterFirstRegex() => View();

    [Route("{s:regex([^\\\\]*)\\[action]")]                                    // FN: the second backslash is not in the regex
    public ActionResult WithSecondBackslashNotInRegex() => View();

    [Route("{s:regex([^\\\\]*)/regex([^\\\\]*)")]                              // Compliant: both backslashes are in the regex
    public ActionResult WithTwoBackslashesInRegex() => View();
}

namespace WithAliases
{
    using MyRoute = RouteAttribute;
    using ASP = System.Web;

    [MyRoute(@"A\[controller]")]            // Noncompliant
    public class WithAliasedRouteAttributeController : Controller { }

    [ASP.Mvc.RouteAttribute("A\\[action]")] // Noncompliant
    public class WithFullQualifiedPartiallyAliasedNameController : Controller { }
}

namespace WithFakeRouteAttribute
{
    [Route(@"A\[controller]")]      // Compliant: not a real RouteAttribute
    public class AControllerController : Controller { }

    [AttributeUsage(AttributeTargets.Class)]
    public class RouteAttribute : Attribute
    {
        public RouteAttribute(string template) { }
    }
}

class WithTuples
{
    void Test()
    {
        var tuple = (1, "A\\[controller]"); // Compliant: the argument is not in a method invocation
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/9193
namespace AttributeWithNamedArgument
{
    [AttributeUsage(AttributeTargets.All)]
    public class MyAttribute : Attribute
    {
        public string Name { get; set; }
    }

    public class MyController : Controller
    {
        [MyAttribute(Name = "Display HR\\Recruitment report")]
        public const string Text = "ABC";
    }
}
