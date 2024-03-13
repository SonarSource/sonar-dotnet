using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Threading.Tasks;

public class TestController : Controller
{
    private readonly string Key = "id";

    public IActionResult Post()
    {
        _ = Request.Form["id"];                           // Noncompliant {{Use model binding instead of accessing the raw request data}}
        //  ^^^^^^^^^^^^
        _ = Request.Form.TryGetValue("id", out _);        // Noncompliant {{Use model binding instead of accessing the raw request data}}
        //  ^^^^^^^^^^^^^^^^^^^^^^^^
        _ = Request.Form.ContainsKey("id");               // Noncompliant {{Use model binding instead of accessing the raw request data}}
        //  ^^^^^^^^^^^^^^^^^^^^^^^^
        _ = Request.Headers["id"];                        // Noncompliant {{Use model binding instead of accessing the raw request data}}
        //  ^^^^^^^^^^^^^^^
        _ = Request.Headers.TryGetValue("id", out _);     // Noncompliant {{Use model binding instead of accessing the raw request data}}
        //  ^^^^^^^^^^^^^^^^^^^^^^^^^^^
        _ = Request.Headers.ContainsKey("id");            // Noncompliant {{Use model binding instead of accessing the raw request data}}
        //  ^^^^^^^^^^^^^^^^^^^^^^^^^^^
        _ = Request.Query["id"];                          // Noncompliant {{Use model binding instead of accessing the raw request data}}
        //  ^^^^^^^^^^^^^
        _ = Request.Query.TryGetValue("id", out _);       // Noncompliant {{Use model binding instead of accessing the raw request data}}
        //  ^^^^^^^^^^^^^^^^^^^^^^^^^
        _ = Request.RouteValues["id"];                    // Noncompliant {{Use model binding instead of accessing the raw request data}}
        //  ^^^^^^^^^^^^^^^^^^^
        _ = Request.RouteValues.TryGetValue("id", out _); // Noncompliant {{Use model binding instead of accessing the raw request data}}
        //  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        _ = Request.Form.Files;                           // Noncompliant {{Use IFormFile or IFormFileCollection binding instead}}
        //  ^^^^^^^^^^^^^^^^^^
        _ = Request.Form.Files["file"];                   // Noncompliant {{Use IFormFile or IFormFileCollection binding instead}}
        //  ^^^^^^^^^^^^^^^^^^
        _ = Request.Form.Files[0];                        // Noncompliant {{Use IFormFile or IFormFileCollection binding instead}}
        //  ^^^^^^^^^^^^^^^^^^
        _ = Request.Form.Files.Any();                     // Noncompliant {{Use IFormFile or IFormFileCollection binding instead}}
        //  ^^^^^^^^^^^^^^^^^^
        _ = Request.Form.Files.Count;                     // Noncompliant {{Use IFormFile or IFormFileCollection binding instead}}
        //  ^^^^^^^^^^^^^^^^^^
        _ = Request.Form.Files.GetFile("file");           // Noncompliant {{Use IFormFile or IFormFileCollection binding instead}}
        //  ^^^^^^^^^^^^^^^^^^
        _ = Request.Form.Files.GetFiles("file");          // Noncompliant {{Use IFormFile or IFormFileCollection binding instead}}
        //  ^^^^^^^^^^^^^^^^^^
        return default;
    }

    // Parameterized for "Form", "Query", "RouteValues", "Headers"
    void NoncompliantKeyVariations()
    {
        _ = Request.Form[@"key"];                                 // Noncompliant
        _ = Request.Form.TryGetValue(@"key", out _);              // Noncompliant
        _ = Request.Form["""key"""];                              // Noncompliant
        _ = Request.Form.TryGetValue("""key""", out _);           // Noncompliant

        const string key = "id";
        _ = Request.Form[key];                                    // Noncompliant
        _ = Request.Form.TryGetValue(key, out _);                 // Noncompliant
        _ = Request.Form[$"prefix.{key}"];                        // Noncompliant
        _ = Request.Form.TryGetValue($"prefix.{key}", out _);     // Noncompliant
        _ = Request.Form[$"""prefix.{key}"""];                    // Noncompliant
        _ = Request.Form.TryGetValue($"""prefix.{key}""", out _); // Noncompliant

        _ = Request.Form[key: "id"];                              // Noncompliant
        _ = Request.Form.TryGetValue(value: out _, key: "id");    // Noncompliant
    }

    void MixedAccess_Form(string key)
    {
        _ = Request.Form["id"]; // Compliant (a mixed access with constant and non-constant keys is compliant)
        _ = Request.Form[key];  // Compliant
    }

    void MixedAccess_Form_Query(string key)
    {
        _ = Request.Form["id"];  // Compliant (a mixed access with constant and non-constant keys is compliant)
        _ = Request.Query[key];  // Compliant
    }

    void FalseNegatives()
    {
        string localKey = "id";
        _ = Request.Form[localKey];                               // FN (Requires SE)
        _ = Request.Form[Key];                                    // FN: Key is a readonly field with a constant initializer (Requires cross procedure SE)
    }

    void HeaderAccess()
    {
        Request.Headers["id"] = "Assignment";                     // Compliant
    }

    // Parameterized for Form, Headers, Query, RouteValues / Request, this.Request, ControllerContext.HttpContext.Request / [FromForm], [FromQuery], [FromRoute], [FromHeader]
    // Implementation: Consider adding a CombinatorialDataAttribute https://stackoverflow.com/a/75531690
    async Task Compliant([FromForm] string key)
    {
        _ = Request.Form.Keys;
        _ = Request.Form.Count;
        foreach (var kvp in Request.Form)
        { }
        _ = Request.Form.Select(x => x);
        _ = Request.Form[key];                // Compliant: The accessed key is not a compile time constant
        _ = Request.Cookies["cookie"];        // Compliant: Cookies are not bound by default
        _ = Request.QueryString;              // Compliant: Accessing the whole raw string is fine.
        _ = await Request.ReadFormAsync();    // Compliant: This might be used for optimization purposes e.g. conditional form value access.
    }

    // parameterized test: parameters are the different forbidden Request accesses (see above)
    private static void HandleRequest(HttpRequest request)
    {
        _ = request.Form["id"]; // Noncompliant: Containing type is a controller
        void LocalFunction()
        {
            _ = request.Form["id"]; // Noncompliant: Containing type is a controller
        }
        static void StaticLocalFunction(HttpRequest request)
        {
            _ = request.Form["id"]; // Noncompliant: Containing type is a controller
        }
    }
}

public class CodeBlocksController : Controller
{
    public CodeBlocksController()
    {
        _ = Request.Form["id"]; // Noncompliant
    }

    public CodeBlocksController(object o) => _ = Request.Form["id"]; // Noncompliant

    HttpRequest ValidRequest => Request;
    IFormCollection Form => Request.Form;

    string P1 => Request.Form["id"]; // Noncompliant
    string P2
    {
        get => Request.Form["id"]; // Noncompliant
    }
    string P3
    {
        get
        {
            return Request.Form["id"]; // Noncompliant
        }
    }
    void M1() => _ = Request.Form["id"]; // Noncompliant
    void M2()
    {
        Func<string> f1 = () => Request.Form["id"];  // Noncompliant
        Func<object, string> f2 = x => Request.Form["id"];  // Noncompliant
        Func<object, string> f3 = delegate (object x) { return Request.Form["id"]; };  // Noncompliant
    }
    void M3()
    {
        _ = (true ? Request : Request).Form["id"]; // Noncompliant
        _ = ValidatedRequest().Form["id"]; // Noncompliant
        _ = ValidRequest.Form["id"]; // Noncompliant
        _ = Form["id"];      // Noncompliant
        _ = this.Form["id"]; // Noncompliant
        _ = new CodeBlocksController().Form["id"]; // Noncompliant

        HttpRequest ValidatedRequest() => Request;
    }

    void M4()
    {
        _ = this.Request.Form["id"]; // Noncompliant
        _ = Request?.Form?["id"]; // Noncompliant
        _ = Request?.Form?.TryGetValue("id", out _); // Noncompliant
        _ = Request.Form?.TryGetValue("id", out _); // Noncompliant
        _ = Request.Form?.TryGetValue("id", out _).ToString(); // Noncompliant
        _ = HttpContext.Request.Form["id"]; // Noncompliant
        _ = Request.HttpContext.Request.Form["id"]; // Noncompliant
        _ = this.ControllerContext.HttpContext.Request.Form["id"]; // Noncompliant
        var r1 = HttpContext.Request;
        _ = r1.Form["id"]; // Noncompliant
        var r2 = ControllerContext;
        _ = r2.HttpContext.Request.Form["id"]; // Noncompliant
    }
    ~CodeBlocksController() => _ = Request.Form["id"]; // Noncompliant
}


// parameterized test: Repeat for Controller, ControllerBase, MyBaseController, MyBaseBaseController base classes
// consider adding "PageModel" to the parametrized test but functional tests and updates to the RSpec are needed.
public class MyBaseController : ControllerBase { }
public class MyBaseBaseController : MyBaseController { }
public class MyTestController : MyBaseBaseController
{
    public void Action()
    {
        _ = Request.Form["id"]; // Noncompliant
    }
}

public class OverridesController : Controller
{
    public void Action()
    {
        _ = Request.Form["id"]; // Noncompliant 
    }
    private void Undecidable(HttpContext context)
    {
        // Implementation: It might be difficult to distinguish between access to "Request" that originate from overrides vs. "Request" access that originate from action methods.
        // This is especially true for "Request" which originate from parameters like here. We may need to redeclare such cases as FNs (see e.g HandleRequest above).
        _ = context.Request.Form["id"]; // Undecidable: request may originate from an action method (which supports binding), or from one of the following overrides (which don't).
    }
    private void Undecidable(HttpRequest request)
    {
        _ = request.Form["id"]; // Undecidable: request may originate from an action method (which supports binding), or from one of the following overloads (which don't).
    }
    public override void OnActionExecuted(ActionExecutedContext context)
    {
        _ = context.HttpContext.Request.Form["id"]; // Compliant: Model binding is not supported here
    }
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        _ = context.HttpContext.Request.Form["id"]; // Compliant: Model binding is not supported here
    }
    public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        _ = context.HttpContext.Request.Form["id"]; // Compliant: Model binding is not supported here
        return base.OnActionExecutionAsync(context, next);
    }
}

// parameterized test for PocoController, [Controller]Poco
// consider adding "PageModel" to the parametrized test but functional tests and updates to the RSpec are needed.
public class PocoController : IActionFilter, IAsyncActionFilter
{
    public void OnActionExecuted(ActionExecutedContext context)
    {
        _ = context.HttpContext.Request.Form["id"]; // Compliant: Model binding is not supported here
    }
    void IActionFilter.OnActionExecuted(Microsoft.AspNetCore.Mvc.Filters.ActionExecutedContext context)
    {
        _ = context.HttpContext.Request.Form["id"]; // Compliant: Model binding is not supported here
    }
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _ = context.HttpContext.Request.Form["id"]; // Compliant: Model binding is not supported here
    }
    void IActionFilter.OnActionExecuting(ActionExecutingContext context)
    {
        _ = context.HttpContext.Request.Form["id"]; // Compliant: Model binding is not supported here
    }
    public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        _ = context.HttpContext.Request.Form["id"]; // Compliant: Model binding is not supported here
        return Task.CompletedTask;
    }
    Task IAsyncActionFilter.OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        _ = context.HttpContext.Request.Form["id"]; // Compliant: Model binding is not supported here
        return Task.CompletedTask;
    }
}

static class HttpRequestExtensions
{
    // parameterized test: parameters are the different forbidden Request accesses (see above)
    public static void Ext(this HttpRequest request)
    {
        _ = request.Form["id"]; // Compliant: Not in a controller
    }
}

class RequestService
{
    public HttpRequest Request { get; }
    // parameterized test: parameters are the different forbidden Request accesses (see above)
    public void HandleRequest(HttpRequest request)
    {
        _ = Request.Form["id"]; // Compliant: Not in a controller
        _ = request.Form["id"]; // Compliant: Not in a controller
    }
}
