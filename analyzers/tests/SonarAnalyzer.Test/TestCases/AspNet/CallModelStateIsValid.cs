using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class NonCompliantController : ControllerBase
{
    [HttpGet("/[controller]")]
    public string Get([Required, FromQuery] string id)                          // Noncompliant {{ModelState.IsValid should be checked in controller actions.}}
    //            ^^^
    {
        return "Hello!";
    }

    [HttpPost("/[controller]")]
    public string Post(Movie movie)                                             // Noncompliant
    {
        return "Hello!";
    }

    [HttpPut("/[controller]")]
    public string Put(Movie movie)                                              // Noncompliant
    {
        return "Hello!";
    }

    [HttpDelete("/[controller]")]
    public string Delete(Movie movie)                                           // Noncompliant
    {
        return "Hello!";
    }

    [HttpPatch("/[controller]")]
    public string Patch(Movie movie)                                            // Noncompliant
    {
        return "Hello!";
    }

    [HttpGet]
    [HttpPost]
    [HttpPut]
    [HttpDelete]
    [HttpPatch]
    [Route("/[controller]/mix")]
    public string Mix([Required, FromQuery, EmailAddress] string email)         // Noncompliant
    {
        return "Hello!";
    }

    [AcceptVerbs("GET")]
    [Route("/[controller]/accept-verbs")]
    public string AGet([Required] string id)                                    // Noncompliant
    {
        return "Hello!";
    }

    [AcceptVerbs("POST")]
    [Route("/[controller]/accept-verbs")]
    public string APost(Movie movie)                                            // Noncompliant
    {
        return "Hello!";
    }

    [AcceptVerbs("PUT")]
    [Route("/[controller]/accept-verbs")]
    public string APut(Movie movie)                                             // Noncompliant
    {
        return "Hello!";
    }

    [AcceptVerbs("DELETE")]
    [Route("/[controller]/accept-verbs")]
    public string ADelete(Movie movie)                                          // Noncompliant
    {
        return "Hello!";
    }

    [AcceptVerbs("PATCH")]
    [Route("/[controller]/accept-verbs")]
    public string APatch(Movie movie)                                           // Noncompliant
    {
        return "Hello!";
    }

    [AcceptVerbs("GET", "POST", "PUT", "DELETE", "PATCH")]
    [Route("/[controller]/many")]
    public string Many([Required, FromQuery, EmailAddress] string email)        // Noncompliant
    {
        return "Hello!";
    }

    [HttpGet("/[controller]/list")]
    public string[] List() => null;                                             // Compliant

    [HttpGet("/[controller]/listasync")]
    public Task<string[]> ListAsync(CancellationToken token) => null;           // Compliant
}

[ApiController]
public class ControllerWithApiAttributeAtTheClassLevel : ControllerBase
{
    [HttpPost("/[controller]")]
    public string Add(Movie movie)                                              // Compliant, ApiController attribute is applied at the class level.
    {
        return "Hello!";
    }
}

public class BaseClassHasApiControllerAttribute : ControllerWithApiAttributeAtTheClassLevel
{
    [HttpDelete("/[controller]")]
    public string Remove(Movie movie)                                           // Compliant, base class is decorated with the ApiController attribute
    {
        return "Hello!";
    }
}

[Controller]
public class ControllerThatDoesNotInheritFromControllerBase
{
    [HttpPost("/[controller]")]
    public string Add(Movie movie)                                              // Compliant, ModelState is not available in this context.
    {
        return "Hello!";
    }
}

public class SimpleController
{
    [HttpPost("/[controller]")]
    public string Add(Movie movie)                                              // Compliant, ModelState is not available in this context.
    {
        return "Hello!";
    }
}

public class CompliantController : ControllerBase
{
    [HttpGet("/[controller]")]
    public string Get([Required, FromQuery] string id)                          // Compliant
    {
        if (!ModelState.IsValid)
        {
            return null;
        }
        return "Hello";
    }

    [HttpGet("/[controller]/GetNoParam")]
    public string GetNoParam()                                                  // Compliant
    {
        return "Hello";
    }

    [HttpPost("/[controller]")]
    public string Post([Required, FromBody] string id)                          // Compliant
    {
        var state = ModelState;
        if (!state.IsValid)
        {
            return null;
        }
        return "Hello";
    }
}

[ValidateModel]
public class ControllerClassWithActionFilter : ControllerBase
{
    [HttpPost("/[controller]/create")]
    public string Create(Movie movie)                                           // Compliant, Controller class is decorated with an Action Filter
    {
        return "Hello!";
    }
}

public class ControllerMethodsWithActionFilter : ControllerBase
{
    [ValidateModel]
    [HttpPost("/[controller]/create")]
    public string Create(Movie movie)                                           // Compliant, Controller action is decorated with an Action Filter
    {
        return "Hello!";
    }
}

public class Movie
{
    [Required]
    public string Title { get; set; }

    [Range(1900, 2200)]
    public int Year { get; set; }
}

public class ValidateModelAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            context.Result = new BadRequestObjectResult(context.ModelState);
        }
    }
}

public class ControllerCallsTryValidate : ControllerBase
{
    [HttpPost]
    public string CallsTryValidateModel1([Required] string email)               // Compliant, calls TryValidateModel
    {
        return TryValidateModel(email) ? "Hi!" : "Hello!";
    }

    [HttpPost]
    public string CallsTryValidateModel([Required] string email)                // Compliant, calls TryValidateModel
    {
        return TryValidateModel(email, "prefix") ? "Hi!" : "Hello!";
    }
}

public class ControllerCallsTryValidateOverride : ControllerBase
{
    public override bool TryValidateModel(object model) => true;
    public override bool TryValidateModel(object model, string prefix) => true;

    [HttpPost]
    public string CallsTryValidateModel1([Required] string email)               // Compliant, calls TryValidateModel
    {
        return TryValidateModel(email) ? "Hi!" : "Hello!";
    }

    [HttpPost]
    public string CallsTryValidateModel([Required] string email)                // Compliant, calls TryValidateModel
    {
        return TryValidateModel(email, "prefix") ? "Hi!" : "Hello!";
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/9325
namespace Repro_9325
{
    public class MyController : ControllerBase
    {
        [HttpGet("/[controller]")]
        public string Get([Required, FromQuery] string id)                      // Noncompliant - FP: the Model State is checked in a method that's called from the Controller Action
        {
            if (!CheckModelState())
            {
                return "Error!";
            }
            return "Hello!";
        }

        private bool CheckModelState() => ModelState.IsValid;
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/9262
namespace Repro_9262
{
    public class MyController : ControllerBase
    {
        [HttpGet("/[controller]")]
        public string OnlyIngoredTypes(string str, object obj, dynamic dyn)
        {
            return "Hello!";
        }

        [HttpGet("/[controller]")]
        public string WithValidationAttribute(string str, [Required] object obj, dynamic dyn)   // Noncompliant
        {
            return "Hello!";
        }
    }
}
