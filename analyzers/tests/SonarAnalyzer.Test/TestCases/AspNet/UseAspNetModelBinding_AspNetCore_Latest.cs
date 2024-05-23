using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Threading.Tasks;

public class TestController : Controller
{
    private readonly string Key = "id";

    public IActionResult Post(string key)
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
        _ = Request.Form.File\u0073;                      // Noncompliant {{Use IFormFile or IFormFileCollection binding instead}}
        //  ^^^^^^^^^^^^^^^^^^^^^^^
        _ = Request.Form.Files["file"];                   // Noncompliant {{Use IFormFile or IFormFileCollection binding instead}}
        //  ^^^^^^^^^^^^^^^^^^
        _ = Request.Form.Files[key];                      // Noncompliant {{Use IFormFile or IFormFileCollection binding instead}}
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

    void FormCollection(IFormCollection form)
    {
        _ = form["id"]; // Compliant. Using IFormCollection is model binding
    }

    void WriteAccess()
    {
        Request.Headers["id"] = "Assignment";                     // Compliant
        Request.RouteValues["id"] = "Assignment";                 // Compliant
    }

    async Task Compliant()
    {
        _ = Request.Cookies["cookie"];        // Compliant: Cookies are not bound by default
        _ = Request.QueryString;              // Compliant: Accessing the whole raw string is fine.
    }

    async Task CompliantFormAccess()
    {
        var form = await Request.ReadFormAsync(); // Compliant: This might be used for optimization purposes e.g. conditional form value access.
        _ = form["id"];
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
        // see also parameterized test "DottedExpressions"
        _ = Request.Form["id"][0];      // Noncompliant
        _ = Request?.Form["id"][0];     // Noncompliant
        _ = Request.Form?["id"][0];     // Noncompliant
        _ = Request?.Form?["id"][0];    // Noncompliant
        _ = Request.Form?["id"][0];     // Noncompliant
        _ = Request.Form!["id"][0];     // Noncompliant
        _ = Request.Form!?["id"][0];    // Noncompliant
        _ = Request.Form["id"]![0];     // Noncompliant

        _ = Request.Form?["id"][0][0];  // Noncompliant
        _ = Request.Form?["id"][0]?[0]; // Noncompliant
        _ = Request.Form["id"][0]?[0];  // Noncompliant
    }
    ~CodeBlocksController() => _ = Request.Form["id"]; // Noncompliant
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
        _ = context.Request.Form["id"][0];      
        _ = context.Request?.Form["id"][0];     
        _ = context.Request.Form?["id"][0];     
        _ = context.Request?.Form?["id"][0];    
        _ = context.Request.Form?["id"][0];     

        _ = context.Request.Form?["id"][0][0];  
        _ = context.Request.Form?["id"][0]?[0]; 
        _ = context.Request.Form["id"][0]?[0];  
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

[Controller]
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
