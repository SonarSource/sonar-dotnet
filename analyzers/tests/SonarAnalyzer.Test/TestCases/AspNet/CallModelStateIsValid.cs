using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

[ApiController]
public class ControllerWithApiAttributeAtTheClassLevel : ControllerBase
{
    [HttpPost("/[controller]")]
    public string Add(Movie movie)                                              // Compliant, ApiController attribute is applied at the class level.
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

public class CompliantController: ControllerBase
{
    [HttpGet("/[controller]")]
    public string Get([Required, FromQuery] string id)                           // Compliant
    {
        if (!ModelState.IsValid)
        {
            return null;
        }
        return "Hello";
    }
}

public class NonCompliantController : ControllerBase
{
    [HttpGet("/[controller]")]
    public string Get([Required, FromQuery] string id)                           // Noncompliant {{ModelState.IsValid should be checked in controller actions.}}
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

    [HttpPost("/[controller]/without-validation")]
    public string DtoWithoutValidation(DtoWithoutValidation dto)                // Compliant, DtoWithoutValidation does not have any validation attributes.
    {
        return "Hello!";
    }

    [HttpPost("/[controller]/custom-attribute-validation")]
    public string DtoWithCustomAttribute([ClassicMovie] string title)           // Noncompliant
    {
        return "Hello!";
    }

    [HttpPost("/[controller]/custom-validation")]
    public string DtoImplementingIValidatableObject(ValidatableMovie movie)     // Noncompliant
    {
        return "Hello!";
    }

    [HttpPost("/[controller]/try-validate-model")]
    public string CallsTryValidateModel([Required] string email)                // Compliant, calls TryValidateModel
    {
        return TryValidateModel(email) ? "Hi!" : "Hello!";
    }

    [HttpGet("/[controller]/list")]
    public string[] List() => null;                                             // Compliant
}

[ValidateModel]
public class ControllerClassWithActionFilter: ControllerBase
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

public class DtoWithoutValidation
{
    public int? Id { get; set; }
    public string Name { get; set; }
}

public class ClassicMovieAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        return ValidationResult.Success;
    }
}

public class ValidatableMovie : IValidatableObject
{
    public int Id { get; set; }
    public string Title { get; set; } = null;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        yield return new ValidationResult("Title is required", new[] { nameof(Title) });
    }
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

namespace ModelInheritance
{
    public abstract class ModelBase
    {
        [Required] public int Id { get; set; }
    }

    public class Model : ModelBase
    {
    }

    public class MyController : ControllerBase
    {
        [HttpPost("/[controller]/create")]
        public string Create(Model model)           // Noncompliant
        {
            return "Hello!";
        }
    }
}

